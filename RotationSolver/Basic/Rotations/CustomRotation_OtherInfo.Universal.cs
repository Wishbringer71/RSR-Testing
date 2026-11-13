using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace RotationSolver.Basic.Rotations;

public partial class CustomRotation
{
	/// <summary>
	/// No spell with a cast time can start now: the player moves without the movement lock, BossMod's AI moves him, a
	/// knockback runs or no-casting is commanded, or the job's shortest cast fits neither under BossMod's cast limit nor
	/// before a status that damages every action lands; and no Swiftcast, Triplecast or Dualcast makes the next spell
	/// instant. What a caster can still use then is an instant GCD (concept 03, "C6").
	/// </summary>
	[Description("Cast start blocked")]
	public static bool CastStartBlocked => !HasSwift
		&& (ActionBasicInfo.CastStartBlockedNow(false) || ActionBasicInfo.NoCastFitsNow());

	/// <summary>
	/// Whether a melee job's ranged attack should wait because the player is closing on his target: reaching the range of
	/// the combo starter in t seconds, waiting pays while t &lt; GCD * (1 - ranged potency / starter potency) - the whole
	/// sequence shifts by t and loses t / GCD of a melee GCD at the end, while the first GCD lands as a melee hit instead of
	/// the ranged one. Potencies from the tooltips; standing, stepping aside or moving away the ranged attack goes as before
	/// (concept 03, "C4", A605).
	/// </summary>
	/// <param name="starter">The job's melee combo starter.</param>
	/// <param name="ranged">The job's ranged attack.</param>
	protected static bool MeleeWorthWaitingFor(IBaseAction starter, IBaseAction ranged)
	{
		if (CurrentTarget is not { } target || !target.IsAttackable())
		{
			return false;
		}

		var gap = target.DistanceToPlayer() - starter.TargetInfo.Range;
		var speed = DataCenter.ClosingSpeed(target);
		if (gap <= 0f || speed <= 0f
			|| !ActionTooltip.TryStatedPotency(starter.Info.AdjustedID, out var meleePotency)
			|| !ActionTooltip.TryStatedPotency(ranged.Info.AdjustedID, out var rangedPotency)
			|| meleePotency <= rangedPotency)
		{
			return false;
		}

		return gap / speed < DataCenter.DefaultGCDTotal * (1f - (rangedPotency / meleePotency));
	}

	/// <summary>
	/// Whether a damage over time on <paramref name="target"/> is worth putting on again before it is due, because no spell
	/// with a cast time can start (<see cref="CastStartBlocked"/>) and the GCD would otherwise stay empty. With R seconds left
	/// of its duration D it pushes the next refresh back by D - R and so saves that share of a later filler; its MP net of
	/// the saved refresh is the share R / D. That pays per MP at least as well as the filler while
	/// R &lt;= D * fillerMp / (fillerMp + dotMp), on a target that outlives R. It yields while the filler heal would heal,
	/// which only runs when no attack does (concept 03, "C6").
	/// </summary>
	/// <param name="dot">The damage over time, with its <see cref="ActionSetting.Dot"/> duration and target statuses.</param>
	/// <param name="filler">The GCD it replaces when standing, for its MP.</param>
	/// <param name="target">The enemy it would go on.</param>
	protected static bool DotWorthWhileCastBlocked(IBaseAction dot, IBaseAction filler, IBattleChara? target)
	{
		var setting = dot.Setting;
		if (!CastStartBlocked || target == null || setting.Dot == null || setting.TargetStatusProvide == null)
		{
			return false;
		}

		if (Service.Config.HealWhenNothingTodo && DataCenter.ForecastMinHp(false) < Service.Config.HealWhenNothingTodoBelow)
		{
			return false;
		}

		var left = target.StatusTime(setting.StatusFromSelf, setting.TargetStatusProvide);
		if (left == float.MaxValue)
		{
			return false;
		}

		var ttk = target.GetTTK();
		if (!float.IsNaN(ttk) && ttk <= left)
		{
			return false;
		}

		float dotMp = dot.Info.MPNeed;
		float fillerMp = filler.Info.MPNeed;
		var limit = dotMp + fillerMp > 0 ? setting.Dot.Duration * fillerMp / (fillerMp + dotMp) : setting.Dot.Duration;
		return left <= limit;
	}

	/// <summary>
	/// <see cref="PartyMembersAverHP"/> carried forward to the moment a heal begun now would land, each member at the
	/// health he is heading for - what a healing threshold on the party's average reads, as "Heal ahead of incoming
	/// damage" states for every healing threshold. With that setting off it equals <see cref="PartyMembersAverHP"/>
	/// (A358).
	/// </summary>
	/// <param name="instant">An off-GCD heal, which looks ahead only as far as the current animation lock.</param>
	/// <returns>The forecast average health ratio.</returns>
	public static float PartyMembersForecastAverHP(bool instant = false)
	{
		DataCenter.ComputeForecastAreaStats(instant, out var avgHp, out _, out _, out _);
		return avgHp;
	}

	/// <summary>
	/// Surveys the stun state of the hostiles an area action would actually hit.
	/// </summary>
	/// <param name="radius">
	/// The action's effect radius, not the job's attack range: a caster reaches 25 yalms while Holy
	/// covers 8, and measuring over the wider set would count enemies the cast never touches.
	/// </param>
	/// <param name="allStunned">True when every hostile inside the radius is currently stunned.</param>
	/// <param name="headroom">
	/// True when at least one of them is not stunned and could still be stunned
	/// (<see cref="StunWatch.CanStillBeStunned"/>). The game shows the halving of repeated stuns as no
	/// status, so the immunity it ends in is what the fight has shown: a stun reported as resisted, or
	/// a stunning action that hit without stunning (A272).
	/// </param>
	/// <returns>How many hostiles were inside the radius. Zero means neither flag says anything.</returns>
	protected static int SurveyStuns(float radius, out bool allStunned, out bool headroom)
		=> SurveyStuns(radius, out _, out allStunned, out headroom);

	/// <summary>
	/// As <see cref="SurveyStuns(float, out bool, out bool)"/>, and additionally reports how many of
	/// the hostiles inside the radius are stunned.
	/// </summary>
	/// <param name="stunnedCount">
	/// How many are stunned right now. The count is what separates an area stun from a single-target
	/// one: Holy stops everything within its radius, while Low Blow, Shield Bash and Leg Sweep stop
	/// exactly one enemy while the rest of a pull keeps hitting. A rule that reacts to "the damage
	/// stream is interrupted" has to read the share, not the presence, of stunned enemies.
	/// </param>
	/// <param name="radius">How far around the player hostiles are counted, in yalms.</param>
	/// <param name="allStunned">Whether every hostile inside the radius is stunned or immune.</param>
	/// <param name="headroom">
	/// Whether anything inside the radius could still be stunned. Without it there is nothing to
	/// stretch and nothing to save, so a hold rule has no reason to fire.
	/// </param>
	protected static int SurveyStuns(float radius, out int stunnedCount, out bool allStunned,
		out bool headroom)
	{
		allStunned = false;
		headroom = false;
		stunnedCount = 0;

		var hostiles = DataCenter.AllHostileTargets;
		if (hostiles == null || hostiles.Count == 0)
		{
			return 0;
		}

		int inRange = 0, stunned = 0;
		for (int i = 0, n = hostiles.Count; i < n; i++)
		{
			var hostile = hostiles[i];
			if (hostile == null || hostile.DistanceToPlayer() > radius)
			{
				continue;
			}

			inRange++;

			// Any source counts: a stun applied by the tank protects just as well as our own.
			if (hostile.HasStatus(false, StatusHelper.StunStatus))
			{
				stunned++;
			}
			else if (StunWatch.CanStillBeStunned(hostile))
			{
				headroom = true;
			}
		}

		allStunned = inRange > 0 && stunned == inRange;
		stunnedCount = stunned;
		return inRange;
	}

	/// <summary>
	/// How much longer a slowed enemy takes between attacks. Arm's Length (7548) afflicts
	/// "Slow +20%", and the sign is the point: the delay grows, the hit does not shrink.
	/// </summary>
	/// <remarks>
	/// Read from the action's effect text in the game, the English sheet evaluated like the tooltip, as the trait
	/// durations are (A340). A text that cannot be read answers 0: the slow then leaves an enemy's output whole, the
	/// larger figure, which releases the one ceiling that reads it rather than holding Holy on a guess.
	/// </remarks>
	private static float SlowDelayIncrease => _slowDelayIncrease ??= StatedSlow(ActionID.ArmsLengthPvE);

	private static float? _slowDelayIncrease;

	private static float StatedSlow(ActionID action)
	{
		if (ActionTooltip.TryStated((uint)action, StatedSlowPercent, out var percent))
		{
			return percent / 100f;
		}

		return 0f;
	}

	/// <summary>
	/// What share of its damage output a hostile still has, in percent, given the throttles on it.
	/// </summary>
	/// <remarks>
	/// The generalisation of a head count. A rule that asks "is this pull still worth an area cast"
	/// is really asking about output, and a throttled enemy has not left the fight - it contributes
	/// less. Counting heads answers that only in the special case where every throttle is all or
	/// nothing.
	/// <para>
	/// Every factor is quoted from the action's own effect text in ActionId.resx, and they multiply,
	/// as <see cref="GetCurrentMitigationPercent"/> already has them do: Slow +20% (Arm's Length,
	/// 7548), Reprisal 10% (7535), Feint 10%
	/// physical (7549), Addle 5% physical (7560), Dismantle 10% (2887).
	/// </para>
	/// <para>
	/// The physical column is the one taken, without the magical/physical heuristic
	/// <see cref="GetCurrentMitigationPercent"/> uses for a single imminent hit. The question here is
	/// a standing pull's continuous stream, and that stream is auto-attacks - which is also why the
	/// slow belongs in the product at all: it raises auto-attack delay, so it thins exactly the
	/// damage this measures.
	/// </para>
	/// <para>
	/// Reprisal carries one figure at every level: its Enhanced trait extends the debuff from 10s to
	/// 15s and leaves the 10% alone. An earlier version of this method split it by level into 10%
	/// and 15%, which was wrong, and the level it read now serves only
	/// <see cref="MitigationDebuffDuration"/>, where the trait does change the answer.
	/// </para>
	/// </remarks>
	/// <param name="hostile">The enemy to weigh.</param>
	/// <returns>100 for an untouched enemy, less for a throttled one; 0 if there is no enemy.</returns>
	protected static int HostileOutputPercent(IBattleChara hostile)
	{
		if (hostile == null)
		{
			return 0;
		}

		var factor = 1.0f;

		if (hostile.HasStatus(false, StatusHelper.SlowStatus))
		{
			// Slow is the one entry here that is not a damage reduction, so it cannot be multiplied
			// in like one. The effect text of Arm's Length (7548) reads "Slow +20%" - a *longer*
			// delay between the enemy's attacks, not a smaller hit. Twenty percent more time per
			// attack leaves 1/1.20 of the attacks in the same span, so the stream runs at 83% of its
			// rate and not at 80%. The other four below reduce the damage of each hit outright and
			// do multiply in directly.
			factor *= 1f / (1f + SlowDelayIncrease);
		}

		// The physical column of each effect text, through the generated figures (A340).
		if (hostile.HasStatus(false, StatusHelper.ReprisalStatus))
		{
			factor *= 1f - DefensiveValues.For((uint)ActionID.ReprisalPvE).EnemyPhysical;
		}

		if (hostile.HasStatus(false, StatusID.Feint))
		{
			factor *= 1f - DefensiveValues.For((uint)ActionID.FeintPvE).EnemyPhysical;
		}

		if (hostile.HasStatus(false, StatusID.Addle))
		{
			factor *= 1f - DefensiveValues.For((uint)ActionID.AddlePvE).EnemyPhysical;
		}

		if (hostile.HasStatus(false, StatusID.Dismantled))
		{
			factor *= 1f - DefensiveValues.For((uint)ActionID.DismantlePvE).EnemyPhysical;
		}

		return (int)Math.Round(factor * 100f);
	}

	/// <summary>
	/// Counts the hostiles within <paramref name="radius"/> and sums what output they still have.
	/// </summary>
	/// <remarks>
	/// The measure an area rule wants: <c>AoeCount</c> enemies at full output is what a threshold of
	/// <c>AoeCount * 100</c> expresses, and throttled enemies move the sum without being struck from
	/// the count. Three enemies with one slowed come to 283; four with two come to 366.
	/// </remarks>
	/// <param name="radius">The radius to measure over, in yalms.</param>
	/// <param name="output">Summed output in percent across the hostiles inside the radius.</param>
	/// <returns>How many hostiles were inside the radius. Zero means the sum says nothing.</returns>
	protected static int SurveyHostileOutput(float radius, out int output)
	{
		output = 0;
		var hostiles = DataCenter.AllHostileTargets;
		if (hostiles == null || hostiles.Count == 0)
		{
			return 0;
		}

		var inRange = 0;
		for (int i = 0, n = hostiles.Count; i < n; i++)
		{
			var hostile = hostiles[i];
			if (hostile == null || hostile.DistanceToPlayer() > radius)
			{
				continue;
			}

			inRange++;
			output += HostileOutputPercent(hostile);
		}

		return inRange;
	}

	/// <summary>
	/// Counts the hostiles within <paramref name="radius"/> and how many of them carry any of
	/// <paramref name="statuses"/>.
	/// </summary>
	/// <remarks>
	/// The plain counterpart to <see cref="SurveyStuns(float, out int, out bool, out bool)"/>, which
	/// carries stun-specific reasoning (resistance, headroom) that no other effect needs. Written for
	/// the question "is the incoming damage stream being throttled": a share of slowed enemies
	/// answers it the same way a share of stunned ones does, only weaker and for longer.
	/// </remarks>
	/// <param name="radius">The radius to measure over, in yalms.</param>
	/// <param name="statuses">The status group to look for; any one of them counts.</param>
	/// <param name="carrying">How many hostiles inside the radius carry one.</param>
	/// <returns>How many hostiles were inside the radius. Zero means the count says nothing.</returns>
	protected static int SurveyHostileStatus(float radius, StatusID[] statuses, out int carrying)
	{
		carrying = 0;
		var hostiles = DataCenter.AllHostileTargets;
		if (hostiles == null || hostiles.Count == 0)
		{
			return 0;
		}

		var inRange = 0;
		for (int i = 0, n = hostiles.Count; i < n; i++)
		{
			var hostile = hostiles[i];
			if (hostile == null || hostile.DistanceToPlayer() > radius)
			{
				continue;
			}

			inRange++;

			// Any source counts, as with stuns: what matters is the state of the enemy, not who put
			// it there.
			if (hostile.HasStatus(false, statuses))
			{
				carrying++;
			}
		}

		return inRange;
	}

	/// <summary>
	/// Whether the area hit behind the area-defence flag reaches the player himself. The flag asks
	/// whether the party is hit, for party mitigations; an action that protects only the player asks
	/// this as well (concept 13).
	/// </summary>
	public static bool AreaHitOnMe => DataCenter.AreaHitReachesPlayer;
}
