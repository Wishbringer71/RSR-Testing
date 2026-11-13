using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using Lumina.Excel.Sheets;
using RotationSolver.Basic.Configuration;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace RotationSolver.Basic.Helpers;

public static partial class ObjectHelper
{
	/// <summary>
	/// Concept 03, "Gegner, die keine Fläche treffen darf" (A572, A588): an enemy no attack may hit, not even an area one -
	/// spikes that damage attackers, a stop marker while "Never attack targets with stop markers" is on, a guarded zu egg,
	/// an enemy a kill rule holds where its fall would harm.
	/// </summary>
	internal static bool MustNotBeHit(this IBattleChara battleChara, ICollection<long> stopTargets)
	{
		return battleChara.HasSpikes() || battleChara.IsGuardedZuEgg() || KillOrder.MustNotBeHit(battleChara)
			|| (Service.Config.FilterStopMark2 && !DataCenter.IsPvP && stopTargets.Contains((long)battleChara.GameObjectId));
	}

	/// <summary>
	/// Concept 03, "Gegner mit Stacheln" (A571, A588): whether this enemy carries a status that turns the player's attacks
	/// back on him - spikes, a counter or a reflection of the kind of attack his job makes - while "Do not attack enemies
	/// whose spikes damage their attackers" is on: no target of an attack, only of an action that removes the status.
	/// </summary>
	internal static bool HasSpikes(this IBattleChara battleChara)
	{
		return SpikeOf(battleChara) != null;
	}

	// A734: the sides Directional Parry guards, in the low bits of its param (BossModReborn's Components/DirectionalParry;
	// front = 1 seen in the owner's run of 11.11.2026). Which of 4 and 8 is left is open there; both mean the flanks here.
	[Flags]
	private enum ParrySide : byte
	{
		None = 0,
		Front = 1,
		Back = 2,
		Left = 4,
		Right = 8,
	}

	private const ParrySide AllParrySides = ParrySide.Front | ParrySide.Back | ParrySide.Left | ParrySide.Right;

	/// <summary>
	/// Concept 03, "Gegner, die deine Seite pariert" (A734): whether this enemy's Directional Parry guards the quarter around
	/// its facing the player stands in - front, back or a flank, split at the diagonals. Not while it targets the player: it
	/// turns to him, and every side he steps to is its front.
	/// </summary>
	internal static bool ParriesPlayerSide(this IBattleChara enemy)
	{
		if (Player.Object is not { } player || enemy.TargetObjectId == player.GameObjectId)
		{
			return false;
		}

		foreach (var status in enemy.StatusList)
		{
			if (status == null || status.StatusId != (uint)StatusID.DirectionalParry)
			{
				continue;
			}

			var guarded = (ParrySide)status.Param & AllParrySides;
			var facing = enemy.GetFaceVector();
			var toPlayer = player.Position - enemy.Position;
			facing.Y = 0;
			toPlayer.Y = 0;
			var along = Vector3.Dot(facing, toPlayer);
			var across = Vector3.Cross(facing, toPlayer).Length();
			var side = MathF.Abs(along) >= across
				? along >= 0f ? ParrySide.Front : ParrySide.Back
				: ParrySide.Left | ParrySide.Right;
			return (guarded & side) != ParrySide.None;
		}

		return false;
	}

	/// <summary>
	/// Concept 03, "Tötungsregeln", rule 1 (A588): whether the next damage kills this enemy - a status whose text says so,
	/// Stone Curse on a petrified enemy.
	/// </summary>
	internal static bool DiesToNextHit(this IBattleChara battleChara)
	{
		foreach (var status in battleChara.StatusList)
		{
			if (status != null && status.StatusId != 0 && StatusHelper.DiesToAnyHit(status.StatusId))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>The status by which <see cref="HasSpikes"/> leaves this enemy alone, or null.</summary>
	internal static Dalamud.Game.ClientState.Statuses.IStatus? SpikeOf(IBattleChara battleChara)
	{
		if (!Service.Config.AvoidSpikedEnemies || DataCenter.IsPvP)
		{
			return null;
		}

		var mine = PlayerAttackKinds;
		foreach (var status in battleChara.StatusList)
		{
			if (status != null && status.StatusId != 0 && (StatusHelper.CounteredAttacks(status.StatusId) & mine) != AttackKinds.None)
			{
				return status;
			}
		}

		return null;
	}

	// The class job's rows (xivapi, 05.11.2026): ClassJobCategory 30 the disciples of war, the beastmaster among them; 31
	// the disciples of magic; Role 3 the ranged, 4 the healers (concept 03, "Gegner mit Stacheln", A588).
	private const uint DisciplesOfWar = 30;

	private const uint DisciplesOfMagic = 31;

	private const byte RangedRole = 3;

	private const byte HealerRole = 4;

	/// <summary>
	/// The kinds of attack the player's job makes: physical for a disciple of war, magic for a disciple of magic, from range
	/// for the ranged and healer roles; every kind for the blue mage, whose spells are of either, and when the sheet cannot
	/// be read.
	/// </summary>
	internal static AttackKinds PlayerAttackKinds
	{
		get
		{
			var job = Player.Object?.ClassJob.RowId ?? 0;
			if (job == _attackKindsJob)
			{
				return _attackKinds;
			}

			_attackKindsJob = job;
			_attackKinds = AttackKinds.Any;
			var sheet = Svc.Data.GetExcelSheet<ClassJob>();
			if (job != (uint)Job.BLU && sheet != null && sheet.TryGetRow(job, out var row))
			{
				var kinds = row.ClassJobCategory.RowId switch
				{
					DisciplesOfWar => AttackKinds.Physical,
					DisciplesOfMagic => AttackKinds.Magic,
					_ => AttackKinds.None,
				};
				if (row.Role is RangedRole or HealerRole)
				{
					kinds |= AttackKinds.Ranged;
				}

				if (kinds != AttackKinds.None)
				{
					_attackKinds = kinds;
				}
			}

			return _attackKinds;
		}
	}

	/// <summary>
	/// Whether the native object behind this character carries the cast and status state Dalamud's getters read.
	/// <c>false</c> means <c>IsCasting</c>, the other cast getters and <c>StatusList</c> would throw.
	/// </summary>
	/// <remarks>
	/// Dalamud reads both through virtual functions of the native object and checks neither result:
	/// <c>IsCasting</c> is <c>Struct->GetCastInfo()->IsCasting</c>, <c>StatusList</c> wraps
	/// <c>GetStatusManager()</c>. An object in the table without a virtual table, or of a class that returns no
	/// cast info, throws a NullReferenceException wherever it is read. One did in
	/// <c>TargetUpdater.UpdateAreaCasters</c>, and that ended the target update before the death, dispel, provoke
	/// and interrupt targets and the time to kill were refreshed (A291). FFXIVClientStructs guards its own
	/// <c>Character.IsCasting</c> against the same two cases (9ff7ae79, 03.03.2026). The check sits where the
	/// target lists are built, so every reader of the lists gets objects that pass it.
	/// </remarks>
	internal static unsafe bool HasNativeState(this IBattleChara battleChara)
	{
		var gameObject = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)battleChara.Address;
		if (gameObject == null || gameObject->VirtualTable == null)
		{
			return false;
		}

		var character = (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)gameObject;
		return character->GetCastInfo() != null && character->GetStatusManager() != null;
	}

	/// <summary>
	/// Whether <paramref name="enemy"/> takes part in a fight: the game counts it in combat, it has a target, or its cast
	/// aims at someone other than itself. An enemy that has been pulled shows at least one of them; one that stands idle,
	/// casting for itself, none (concept 03, "Unterbrechen, Betäuben, Provozieren: welcher Gegner", A612).
	/// </summary>
	/// <param name="enemy">The enemy.</param>
	/// <returns>True for an enemy in a fight.</returns>
	internal static bool IsEngaged(this IBattleChara enemy)
	{
		return enemy != null
			&& (enemy.InCombat() || enemy.TargetObject is not null
				|| (enemy.IsCasting && enemy.CastTargetObjectId != enemy.GameObjectId
					&& Svc.Objects.SearchById(enemy.CastTargetObjectId) is not null));
	}

	/// <summary>
	/// Whether <paramref name="enemy"/> is casting an area attack that reaches the party - the player or two living
	/// members, by the shape of the action (<see cref="DataCenter.AreaCastReachesParty"/>).
	/// </summary>
	/// <param name="enemy">The enemy.</param>
	/// <returns>True for such a cast.</returns>
	internal static bool CastAreaReachesParty(this IBattleChara enemy)
	{
		if (enemy == null || !enemy.IsCasting || enemy.CastActionId == 0)
		{
			return false;
		}

		var act = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(enemy.CastActionId) ?? default;
		if (act.RowId == 0)
		{
			return false;
		}

		var shape = (CastType)act.CastType;
		if (act.EffectRange == 0 && shape is CastType.Targeted or CastType.Unknown)
		{
			return false;
		}

		return DataCenter.AreaCastReachesParty(enemy, act);
	}

	/// <summary>
	/// Whether stopping <paramref name="enemy"/>'s cast protects anyone: the enemy takes part in the fight
	/// (<see cref="IsEngaged"/>), or its cast is an area attack that reaches the party (<see cref="CastAreaReachesParty"/>).
	/// Read by every interrupt, stun and sleep that stops a cast (concept 03, A612).
	/// </summary>
	/// <param name="enemy">The enemy.</param>
	/// <returns>True when stopping the cast protects someone.</returns>
	internal static bool CastThreatensParty(this IBattleChara enemy)
	{
		return enemy.IsEngaged() || enemy.CastAreaReachesParty();
	}

	private static readonly Dictionary<ulong, (long Tick, float Whole, float Remaining)> _ttkCache = [];

	/// <summary>
	/// Drops the time-to-kill answers of this frame; called when a new health sample is recorded.
	/// </summary>
	internal static void ForgetTimeToKill()
	{
		_ttkCache.Clear();
	}

	private static float ComputeTTK(IBattleChara battleChara, out float remaining)
	{
		remaining = float.NaN;
		const int movingAverageWindow = 5;
		var objId = battleChara.GameObjectId;

		var startTime = DateTime.MinValue;
		var initialHpRatio = 0f;

		// Small fixed-size window for last ratios without copying the whole queue
		Span<float> window = stackalloc float[movingAverageWindow];
		var wCount = 0;

		foreach ((var time, var hpRatiosDict) in DataCenter.RecordedHP)
		{
			if (hpRatiosDict != null && hpRatiosDict.TryGetValue(objId, out var ratio) && ratio != 1f)
			{
				// The fall is measured from the last time the member stood this high: a sample at or above the start moves
				// it on. Kept at the first one, a hard first hit of a pull left every later level above it, and no fall
				// was seen for the four minutes that sample stayed in the history (A356).
				if (startTime == DateTime.MinValue || ratio >= initialHpRatio)
				{
					startTime = time;
					initialHpRatio = ratio;
				}

				if (wCount < movingAverageWindow)
				{
					window[wCount++] = ratio;
				}
				else
				{
					// shift left by one; window is very small so this is cheap (CopyTo handles the overlap)
					window[1..].CopyTo(window);
					window[movingAverageWindow - 1] = ratio;
				}
			}
		}

		if (startTime == DateTime.MinValue || (DateTime.Now - startTime) < CheckSpan)
		{
			return float.NaN;
		}

		var currentHealthRatio = battleChara.GetHealthRatio();
		if (float.IsNaN(currentHealthRatio))
		{
			return float.NaN;
		}

		var sum = 0f;
		for (var i = 0; i < wCount; i++)
		{
			sum += window[i];
		}

		var avg = wCount > 0 ? sum / wCount : 0f;

		var hpRatioDifference = initialHpRatio - avg;
		if (hpRatioDifference <= 0)
		{
			return float.NaN;
		}

		var elapsedTime = (float)(DateTime.Now - startTime).TotalSeconds;
		var whole = elapsedTime / hpRatioDifference;
		remaining = whole * currentHealthRatio;
		return whole;
	}

	private static readonly ConcurrentDictionary<ulong, (DateTime At, float Ttk, float Ratio)> _ttkForecast = [];

	/// <summary>
	/// How far <see cref="GetTTK"/> has been off for this character lately: 1 means its forecasts
	/// held, above 1 means health fell faster than predicted, below 1 slower.
	/// </summary>
	/// <remarks>
	/// The estimate does not need an outside observer to be judged. It made a prediction a moment
	/// ago - "this one reaches zero in N seconds" - and the health since then either matches it or
	/// does not. Comparing the two is arithmetic the plugin can do on itself, every second, for
	/// every member, which is the whole of what a play session would have contributed and more
	/// besides: it never looks away.
	///
	/// Why it matters for the thing being built rather than for the statistic: a forecast that runs
	/// optimistic is worse than none, because the rule hanging on it would step in too late. With
	/// the bias measured, the same forecast divided by it is the corrected one, and the correction
	/// costs no configured value - it comes out of the observation. That is the adaptation the user
	/// asks for in the other concepts as well: assume, watch, correct.
	///
	/// Only falls are compared. A rising ratio means healing reached the target, which says nothing
	/// about how well the fall was predicted, so those samples are skipped rather than folded in as
	/// a negative. The value is smoothed across samples so that one spike does not move it.
	/// </remarks>
	internal static float GetTtkBias(this IBattleChara battleChara)
	{
		return battleChara != null && _ttkBias.TryGetValue(battleChara.GameObjectId, out var bias)
			? bias
			: 1f;
	}

	/// <summary>
	/// <see cref="GetTTK"/> corrected by how wrong it has been lately, counted from now - the value a rule should read.
	/// </summary>
	/// <remarks>
	/// Reads the forecast that <see cref="ScoreTtkForecast"/> filed rather than recomputing it.
	/// GetTTK walks the whole recorded history, which is four minutes of samples; the heal target
	/// selection asks this question several times per member and runs in the combat path, so
	/// recomputing there would put a four-minute walk per member into every frame. The filed value
	/// is at most a second old, and the quantity it carries averages over far longer than that.
	/// </remarks>
	internal static float GetCorrectedTTK(this IBattleChara battleChara)
	{
		if (battleChara == null
			|| !_ttkForecast.TryGetValue(battleChara.GameObjectId, out var last)
			|| float.IsNaN(last.Ttk)
			|| DateTime.Now - last.At > ForecastFreshness)
		{
			return float.NaN;
		}

		// Filed once per history sample, so up to a second old: counted from now, not from when it was filed (A523).
		var bias = battleChara.GetTtkBias();
		var filed = bias > 0f ? last.Ttk / bias : last.Ttk;
		return Math.Max(0f, filed - (float)(DateTime.Now - last.At).TotalSeconds);
	}

	/// <summary>
	/// The share of its health a character is expected to still hold by the time a heal begun now
	/// could land: 1 when nothing says it is falling, 0 when it would be dead before then.
	/// </summary>
	/// <remarks>
	/// The whole of the forward-looking behaviour sits in this one number, and every rule below
	/// inherits it by multiplying its own quantity with it. That is deliberate: the alternative was
	/// a second trigger and a second rank beside the existing ones, and two mechanisms deciding the
	/// same thing drift apart. Here the question a rule asks changes from "how does this member
	/// stand" to "how will it stand when my heal arrives", and every threshold, rank and short-cut
	/// that already exists follows without being touched.
	///
	/// Why a level threshold alone is late: it is a level, and the danger is a rate. A tank falling
	/// at 33% of its pool per second passes the 70% mark with 2.1 seconds left, while the heal that
	/// mark triggers needs the rest of the GCD plus a cast to land. The cast goes out after the
	/// death. At a gentle rate the same threshold is early enough, which is why the correction has
	/// to scale with the rate rather than be a fixed offset.
	///
	/// The lead time is read from the game rather than configured: what remains of the current GCD
	/// plus one full GCD for the cast. A healer under Presence of Mind therefore looks ahead less
	/// far, which is correct - they can act sooner.
	///
	/// Self-limiting in the case that matters for cost: while health is not falling on balance,
	/// GetTTK answers NaN and this returns 1, so a party held steady sees no change at all. The
	/// look-ahead appears exactly when the net trend is downward, and grows as it steepens.
	///
	/// On by default (the owner's rule for defaults, 29.09.2026). The effect is not shown in play -
	/// static analysis and a compile say nothing about whether a tank lives.
	/// </remarks>
	internal static float GetForecastSurvivingShare(this IBattleChara battleChara)
		=> battleChara.GetForecastSurvivingShare(false);

	/// <summary>
	/// <see cref="GetForecastSurvivingShare(IBattleChara)"/> for a heal of the given kind.
	/// </summary>
	/// <param name="battleChara">The member.</param>
	/// <param name="instant">True for an off-GCD heal, which lands as soon as the current animation
	/// lock and any cast in progress let it go out; false for a GCD heal, which needs the rest of this GCD and its cast. The
	/// setting's text promises the health "by the time a heal started now would land", and for an
	/// instant heal that is now: read with the GCD lead, Benediction or Essential Dignity went out up
	/// to a GCD before they were needed (A213). An off-GCD action that heals only with the next GCD
	/// (Synastry, Krasis, Soteria, Emergency Tactics) passes false (A221).</param>
	internal static float GetForecastSurvivingShare(this IBattleChara battleChara, bool instant)
	{
		if (battleChara == null || !Service.Config.HealAheadOfDamage)
		{
			return 1f;
		}

		var ttk = battleChara.GetCorrectedTTK();
		if (float.IsNaN(ttk) || ttk <= 0f)
		{
			return 1f;
		}

		var lead = instant ? GetInstantHealLeadTime() : GetHealLeadTime();
		return lead <= 0f ? 1f : Math.Clamp(1f - (lead / ttk), 0f, 1f);
	}

	/// <summary>
	/// Whether <paramref name="actionId"/> reaches <paramref name="target"/> past walls, by the game's own range and sight check
	/// (concept 03, "Sichtlinie: das Urteil des Spiels zuerst", A621): in sight or blocked as the game answers; any other answer -
	/// out of range among them - leaves it to RSR's ray (<see cref="CanSee"/>).
	/// </summary>
	internal static unsafe bool InSightFor(this IBattleChara target, uint actionId)
	{
		if (target == null || Player.Object is not { } player || target.Struct() == null)
		{
			return false;
		}

		MaybeResetLosCache();

		var playerPos = player.Position;
		var targetPos = target.Position;
		var id = target.GameObjectId;
		var now = Environment.TickCount64;
		if (_gameSightCache.TryGetValue(id, out var cached) && now <= cached.ExpiresAt
			&& Vector3.DistanceSquared(playerPos, cached.PlayerPos) <= LosPosEpsilonSq
			&& Vector3.DistanceSquared(targetPos, cached.TargetPos) <= LosPosEpsilonSq)
		{
			return cached.Visible;
		}

		var ray = target.CanSee();
		var answer = (ActionStatus)ActionManager.GetActionInRangeOrLoS(actionId,
			(FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)player.Address,
			(FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)target.Address);
		if (answer is not (ActionStatus.None or ActionStatus.NotInLineOfSight))
		{
			return ray;
		}

		var seen = answer == ActionStatus.None;
		_gameSightCache[id] = new LosCacheEntry
		{
			PlayerPos = playerPos,
			TargetPos = targetPos,
			ExpiresAt = now + LosTtlMs,
			Visible = seen,
		};

		if (seen == ray)
		{
			_ = _sightDisagreement.Remove(id);
		}
		else if (_sightDisagreement.Add(id))
		{
			DefenseTrace.Line($"line of sight: the game {(seen ? "sees" : "does not see")} {target.Name} for {actionId}, RSR's ray "
				+ $"{(ray ? "does" : "does not")}; the game's answer counts");
		}

		return seen;
	}

	/// <summary>
	/// Whether healing is being withheld from <paramref name="battleChara"/> so that the death its
	/// own ability is waiting for can happen - the user's <c>WithholdHealingForLivingDead</c>
	/// setting together with <see cref="StatusHelper.InDeathTriggerWindow"/>.
	/// <para>
	/// The pairing lives here rather than in <see cref="StatusHelper"/>, which deliberately reads no
	/// configuration, and it lives in one place rather than at each decision because the hold has to
	/// hold on every path a heal can reach the bearer through. Missing one of them does not weaken
	/// the hold, it defeats it: the bearer is healed out of the trigger and the ability is spent for
	/// nothing.
	/// </para>
	/// </summary>
	public static bool IsHeldForDeathTrigger(this IBattleChara battleChara)
	{
		return battleChara != null
			&& Service.Config != null
			&& Service.Config.WithholdHealingForLivingDead
			&& battleChara.InDeathTriggerWindow();
	}

	/// <inheritdoc cref="IsHeldForDeathTrigger(IBattleChara)"/>
	public static bool PlayerIsHeldForDeathTrigger()
	{
		return Service.Config != null
			&& Service.Config.WithholdHealingForLivingDead
			&& StatusHelper.PlayerInDeathTriggerWindow();
	}

	/// <summary>
	/// Whether this member counts as unprotected for the heal decision - its threshold, the heal target
	/// order, the critical class and the heal-first reading of an announced hit. That is
	/// <see cref="StatusHelper.CanBeHealed"/>, except that with <c>WithholdHealingForLivingDead</c> off
	/// Living Dead does not count as protection.
	/// <para>
	/// Living Dead does not spare its bearer the fall to zero, it turns it into Walking Dead, after which
	/// healing worth his whole maximum HP has to go in within ten seconds or he is KO'd (ActionId.resx,
	/// action 3638). The setting's text promises for Off that he is healed normally and the death never
	/// happens. Read as an invulnerability, his threshold dropped to HealthProtectedRatio for the first
	/// half of the effect, so a member RSR had just pulled above it at the last-ditch line - the very use
	/// the text names as the reason for Off - was left there, and the next hit converted him (A306). With
	/// the setting on, the hold (<see cref="IsHeldForDeathTrigger"/>) governs Living Dead, and this reads
	/// as CanBeHealed.
	/// </para>
	/// <para>
	/// The readers that ask whether a hit can kill him now keep CanBeHealed: the co-tank provoke, which
	/// does not pull the enemy off a tank under Living Dead (451d9e90), and the own tankbuster cooldowns.
	/// The setting speaks of healing.
	/// </para>
	/// </summary>
	/// <remarks>
	/// With the setting off, a member carrying no other invulnerability is unprotected; one who carries
	/// another beside Living Dead is read by CanBeHealed over the whole list, as before.
	/// </remarks>
	public static bool IsUnprotectedForHealing(this IBattleChara battleChara)
	{
		return (Service.Config != null && !Service.Config.WithholdHealingForLivingDead
				&& !battleChara.HasStatus(false, StatusHelper.NoNeedHealingStatusBesidesDeathTrigger))
			|| battleChara.CanBeHealed();
	}

	/// <summary>
	/// The single-heal threshold for <paramref name="battleChara"/> from <paramref name="normal"/>, or false where a heal
	/// on him is excluded: healing does nothing on him (<see cref="StatusHelper.HealingIneffectiveStatus"/>, the recovery
	/// penalty in Windurst), or the hold for his death trigger (<see cref="IsHeldForDeathTrigger"/>). One place for the
	/// single-heal decision and the heal over time against damage over time (A432), so they cannot disagree.
	/// </summary>
	/// <remarks>
	/// A protective status lowers the threshold instead of removing it. "Cannot die right now" is not "does not need
	/// healing": Superbolide puts the gunbreaker at 1 HP on purpose, and when the window closes the target stands exactly
	/// where it left them. Math.Min keeps the invariant that a protected target is never healed more readily than an
	/// unprotected one, whatever the two settings are configured to. Living Dead with its hold switched off is no
	/// protection here: its fall to zero is Walking Dead, not a spared hit (A306).
	/// </remarks>
	internal static bool TryHealSingleThreshold(this IBattleChara battleChara, float normal, out float threshold)
	{
		threshold = 0f;
		if ((DataCenter.IsInWindurst && StatusHelper.HasStatus(battleChara, false, StatusID.HpRecoveryDown))
			|| battleChara.HasStatus(false, StatusHelper.HealingIneffectiveStatus)
			|| battleChara.IsHeldForDeathTrigger())
		{
			return false;
		}

		threshold = battleChara.IsUnprotectedForHealing() ? normal : Math.Min(normal, Service.Config.HealthProtectedRatio);
		return true;
	}

	/// <inheritdoc cref="IsUnprotectedForHealing(IBattleChara)"/>
	public static bool PlayerIsUnprotectedForHealing()
	{
		if (Player.Object == null)
		{
			return false;
		}

		return (Service.Config != null && !Service.Config.WithholdHealingForLivingDead
				&& !StatusHelper.PlayerHasStatus(false, StatusHelper.NoNeedHealingStatusBesidesDeathTrigger))
			|| StatusHelper.PlayerCanBeHealed();
	}
}
