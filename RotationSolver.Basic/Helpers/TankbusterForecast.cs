using ECommons.DalamudServices;
using ECommons.GameHelpers;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// What a tankbuster now being cast at the player is expected to do to him, against what he has to
/// take it - the measurement behind the tankbuster plan (concept 09, "Das geringste Mittel gegen einen
/// gemessenen Tankbuster").
/// </summary>
/// <remarks>
/// <para>Detection only: what is the case, under which figures. No option is read here, and no
/// action of the rotation is chosen. Which mitigation, whether the invulnerability, and whether the rest
/// is held, is decided in <c>CustomRotation</c> (<c>UpdateTankbusterPlan</c>,
/// <c>TankbusterPlanAbility</c>, <c>HoldDefenceForTankbuster</c>) and, for healers, in
/// <c>StateUpdater.ShouldAddDefenseSingle</c>.</para>
///
/// <para>The figure needs the action: only a cast names it. A marker or a BossModReborn prediction says
/// that a tankbuster comes, not which, so there is no figure. An action the learned table
/// (<see cref="TankbusterTable"/>) has not measured has none either.</para>
///
/// <para>Without risk, as the owner asks of every means short of the invulnerability: the prediction is
/// the highest figure ever measured for the action, under only the mitigation that still stands when the
/// hit arrives - the player's own, a co-tank's Reprisal, a healer's - and the budget is the HP the player
/// has now plus the barriers that still stand then. A heal that might come before the hit is not
/// counted, being an assumption about what someone will do.</para>
/// </remarks>
internal static class TankbusterForecast
{
	/// <summary>One tankbuster being cast at the player, and the figures on it.</summary>
	/// <param name="Predicted">Share of maximum HP the hit takes under what stands at impact; zero when
	/// the action has not been measured.</param>
	/// <param name="Budget">Share of maximum HP the player has to take it: HP now plus the barriers that
	/// still stand at impact.</param>
	/// <param name="Horizon">Seconds until the hit can arrive: the cast's rest plus one GCD.</param>
	/// <param name="Serial">Tells this cast from every other, also from a later cast of the same action
	/// by the same enemy with the same cast time.</param>
	internal sealed record Cast(int Serial, IBattleChara Source, uint ActionId, uint AttackType, float Remaining,
		float Total, float Predicted, float Budget, float Horizon)
	{
		/// <summary>Measured at all: the table has a figure for the action.</summary>
		public bool Measured => Predicted > 0f;
	}

	private static readonly List<Cast> _casts = [];

	// The player's own actions that put a status on, as the server confirmed them: action, target, when
	// (monotonic, so a clock change does not stretch anything), its statuses, how long an own copy of them
	// still ran at that moment, and whether the new one has shown since. Kept while the effect can last.
	private sealed class Execution
	{
		public required uint ActionId { get; init; }
		public required ulong Target { get; init; }
		public required long Tick { get; init; }
		public required StatusID[] OnSelf { get; init; }
		public required StatusID[] Where { get; init; }
		public required bool Outward { get; init; }
		public required float PriorOnSelf { get; init; }
		public required float PriorWhere { get; init; }
		public bool StatusSeen { get; set; }
	}

	private static readonly List<Execution> _own = [];

	// The player's own presses (RSR executed the action), by action, with target and when: they count as
	// standing from the press until the server confirms them, for at most one GCD.
	private static readonly Dictionary<uint, (ulong Target, long Tick)> _presses = [];
	private static readonly Dictionary<uint, (StatusID[] OnSelf, StatusID[] Where, bool Outward)> _factsOfAction = [];
	private static ICustomRotation? _statusesFor;
	private static readonly Dictionary<(ulong Source, uint ActionId, float Total), (int Serial, float Elapsed)> _serials = [];
	private static readonly HashSet<(ulong Source, uint ActionId, float Total)> _live = [];
	private static int _nextSerial;

	/// <summary>Every known tankbuster now being cast at the player, the soonest first.</summary>
	public static IReadOnlyList<Cast> Casts => _casts;

	/// <summary>The soonest cast with a figure, or null.</summary>
	public static Cast? SoonestMeasured { get; private set; }

	/// <summary>
	/// Whether the coming hit cannot touch the player: Hallowed Ground or Superbolide lasts past the
	/// soonest tankbuster being cast at him (plus one GCD for the hit to arrive) or, with no cast to time
	/// it, for at least one more GCD - long enough to still press a mitigation once it runs low.
	/// </summary>
	public static bool PlayerImpervious { get; private set; }

	/// <summary>
	/// Whether an invulnerability on the player - Hallowed Ground, Holmgang, Superbolide, Living Dead,
	/// Walking Dead or Undead Rebirth - is still up in <paramref name="seconds"/>, so that it covers a hit
	/// landing then.
	/// </summary>
	public static bool PlayerInvulnerableThrough(float seconds)
	{
		return LongestCopy(Player.Object, StatusHelper.InvulnerabilityStatus, false) >= seconds;
	}

	/// <summary>
	/// RSR executed one of the player's actions (<c>BaseAction.Use</c> succeeded). Called on the game thread.
	/// </summary>
	public static void RecordOwnPress(uint actionId, ulong targetId)
	{
		if (DataCenter.InCombat && DataCenter.Role == JobRole.Tank && DefensiveValues.DurationOf(actionId) > 0f)
		{
			_presses[actionId] = (targetId, Environment.TickCount64);
		}
	}

	/// <summary>
	/// An action of the player went out, as the effect handler saw it. Called on the game thread.
	/// </summary>
	public static void RecordOwnAction(uint actionId, ulong targetId)
	{
		var now = Environment.TickCount64;
		_ = _own.RemoveAll(e => SecondsSince(e.Tick, now) > DefensiveValues.DurationOf(e.ActionId));
		var player = Player.Object;
		var lasts = DefensiveValues.DurationOf(actionId);
		if (player == null || lasts <= 0f || !DataCenter.InCombat || DataCenter.Role != JobRole.Tank)
		{
			return;
		}

		// Party tools (a barrier or mitigation spread over the party) never enter a tankbuster plan and are
		// not followed; Reprisal, a debuff around the player on enemies, is.
		var value = DefensiveValues.For(actionId);
		var (onSelf, where, outward) = FactsOfAction(actionId);
		if ((onSelf.Length == 0 && where.Length == 0) || (outward && (value.Self > 0f || value.Barrier > 0f)))
		{
			return;
		}

		// The client may have put the new copy on before this handler ran: a copy with more than the
		// duration less one GCD left cannot be an older one, so it is already the new status.
		var priorOnSelf = OwnCopy(player, onSelf);
		var priorWhere = LongestOwnCopy(player, targetId, where, outward, float.MaxValue);
		var fresh = lasts - DataCenter.DefaultGCDTotal;
		_own.Add(new Execution
		{
			ActionId = actionId,
			Target = targetId,
			Tick = now,
			OnSelf = onSelf,
			Where = where,
			Outward = outward,
			PriorOnSelf = priorOnSelf,
			PriorWhere = priorWhere,
			StatusSeen = priorOnSelf > fresh || priorWhere > fresh,
		});
	}

	/// <summary>
	/// For the gap between the server confirming one of the player's own actions and its status showing:
	/// the seconds it will still last by its duration from that moment, while its status has not been seen
	/// yet; zero once it has (from then on the status itself tells, also when it ends early - a barrier
	/// broken, a mitigation dispelled) or when the action did not go out within its duration. Whether the
	/// status showed is recorded every cycle in <see cref="Update"/>. <paramref name="anyTarget"/>: the
	/// action counts whatever it was aimed at (Holmgang on an enemy, Reprisal around the player).
	/// </summary>
	public static float OwnPendingCover(uint actionId, bool anyTarget)
	{
		var player = Player.Object;
		var lasts = DefensiveValues.DurationOf(actionId);
		if (player == null || lasts <= 0f)
		{
			return 0f;
		}

		// Pressed, not yet confirmed: for at most one GCD it counts with its full duration from the press.
		var now = Environment.TickCount64;
		if (_presses.TryGetValue(actionId, out var press) && (anyTarget || press.Target == player.GameObjectId)
			&& SecondsSince(press.Tick, now) <= DataCenter.DefaultGCDTotal && !ConfirmedSince(actionId, press.Tick))
		{
			return Math.Max(0f, lasts - SecondsSince(press.Tick, now));
		}

		Execution? latest = null;
		foreach (var execution in _own)
		{
			if (execution.ActionId == actionId && (anyTarget || execution.Target == player.GameObjectId)
				&& (latest == null || execution.Tick > latest.Tick))
			{
				latest = execution;
			}
		}

		return latest == null || latest.StatusSeen
			? 0f
			: Math.Max(0f, lasts - SecondsSince(latest.Tick, Environment.TickCount64));
	}

	// Marks every execution whose new status shows: an own copy of what the action puts on its user, on
	// the player (Holmgang, whatever it was aimed at), or of what it puts where it lands - on its target,
	// or for an effect around the player (Reprisal) on any enemy -, running longer than an own copy ran
	// there when the action went out. Read from the status list itself, not from the status the confirming
	// packet predicts, and a renewal counts only once the renewed copy is on.
	private static void RecordStatusesSeen(IBattleChara player)
	{
		foreach (var execution in _own)
		{
			if (!execution.StatusSeen
				&& (OwnCopy(player, execution.OnSelf) > execution.PriorOnSelf
					|| LongestOwnCopy(player, execution.Target, execution.Where, execution.Outward, execution.PriorWhere)
						> execution.PriorWhere))
			{
				execution.StatusSeen = true;
			}
		}
	}

	// The longest an own copy of the statuses still runs where the action lands: on the player when he
	// is the target, on the target when it is someone else, on any enemy for an effect around the player;
	// -1 when there is none. Stops at the first copy above <paramref name="enough"/>.
	private static float LongestOwnCopy(IBattleChara player, ulong targetId, StatusID[] statuses, bool outward, float enough)
	{
		if (!outward)
		{
			if (targetId == 0 || targetId == player.GameObjectId)
			{
				return OwnCopy(player, statuses);
			}

			return Svc.Objects.SearchById(targetId) is IBattleChara target
				? OwnCopy(target, statuses)
				: -1f;
		}

		var longest = -1f;
		foreach (var obj in Svc.Objects)
		{
			if (obj is IBattleNpc enemy)
			{
				longest = Math.Max(longest, OwnCopy(enemy, statuses));
				if (longest > enough)
				{
					break;
				}
			}
		}

		return longest;
	}

	// The longest of the action's statuses an own copy of which runs on <paramref name="chara"/> - an action
	// can put several on, of different lengths (Bloodwhetting and Stem the Flow), and the longest is its own.
	private static float OwnCopy(IBattleChara chara, StatusID[] statuses)
	{
		return LongestCopy(chara, statuses, true);
	}

	// The statuses an action puts on its user (its setting's StatusProvide) and where it lands (its
	// setting's TargetStatusProvide and the statuses its effect text is tied to; The Blackest Night's barrier
	// has no mitigation figure, only a status), and whether it works around the player (an effect radius,
	// game data). Kept per action until the rotation changes.
	private static (StatusID[] OnSelf, StatusID[] Where, bool Outward) FactsOfAction(uint actionId)
	{
		var rotation = DataCenter.CurrentRotation;
		if (!ReferenceEquals(rotation, _statusesFor))
		{
			_statusesFor = rotation;
			_factsOfAction.Clear();
		}

		if (_factsOfAction.TryGetValue(actionId, out var known))
		{
			return known;
		}

		List<StatusID> onSelf = [];
		List<StatusID> where = [];
		if (DefensiveValues.MitigatingStatusesByActionId.TryGetValue(actionId, out var tied))
		{
			foreach (var status in tied)
			{
				where.Add((StatusID)status);
			}
		}

		var actions = rotation?.AllBaseActions;
		if (actions != null)
		{
			foreach (var action in actions)
			{
				if (action == null || (action.ID != actionId && action.AdjustedID != actionId))
				{
					continue;
				}

				onSelf.AddRange(action.Setting.StatusProvide ?? []);
				where.AddRange(action.Setting.TargetStatusProvide ?? []);
			}
		}

		var outward = Service.GetSheet<Lumina.Excel.Sheets.Action>().TryGetRow(actionId, out var row) && row.EffectRange > 0;
		(StatusID[] OnSelf, StatusID[] Where, bool Outward) result = ([.. onSelf], [.. where], outward);
		_factsOfAction[actionId] = result;
		return result;
	}

	private static bool ConfirmedSince(uint actionId, long tick)
	{
		foreach (var execution in _own)
		{
			if (execution.ActionId == actionId && execution.Tick >= tick)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// The longest any copy of the statuses still runs on <paramref name="chara"/>, read from the status list
	/// itself - never the status a confirming packet predicts - and guarded; -1 when there is none or it has
	/// no end. <paramref name="fromPlayer"/> counts only the player's own copies.
	/// </summary>
	public static float LongestCopy(IBattleChara? chara, StatusID[] statuses, bool fromPlayer)
	{
		var longest = -1f;
		var player = Player.Object;
		if (chara == null || player == null)
		{
			return longest;
		}

		try
		{
			var list = chara.StatusList;
			if (list == null)
			{
				return longest;
			}

			foreach (var status in list)
			{
				if (status == null || status.RemainingTime <= 0f || (fromPlayer && status.SourceId != player.GameObjectId))
				{
					continue;
				}

				foreach (var id in statuses)
				{
					if (status.StatusId == (uint)id)
					{
						longest = Math.Max(longest, status.RemainingTime);
						break;
					}
				}
			}
		}
		catch (Exception)
		{
			// A status list that throws (an object going away) answers nothing.
		}

		return longest;
	}

	/// <summary>Whether the cast with <paramref name="serial"/> is still coming at the player.</summary>
	public static bool IsRunning(int serial)
	{
		foreach (var cast in _casts)
		{
			if (cast.Serial == serial)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>Seconds between two readings of <see cref="Environment.TickCount64"/>.</summary>
	internal static float SecondsSince(long tick, long now)
	{
		return (float)TimeSpan.FromMilliseconds(now - tick).TotalSeconds;
	}

	/// <summary>Recomputes the figures. Called once per framework cycle, on the game thread.</summary>
	public static void Update()
	{
		_casts.Clear();
		SoonestMeasured = null;
		PlayerImpervious = false;

		var player = Player.Object;
		if (!DataCenter.InCombat || player == null || player.IsDead || DataCenter.Role != JobRole.Tank)
		{
			_own.Clear();
			_presses.Clear();
			_serials.Clear();
			return;
		}

		_live.Clear();
		RecordStatusesSeen(player);

		var gcd = DataCenter.DefaultGCDTotal;
		var maxHp = (float)Math.Max(1u, player.MaxHp);
		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile == null || !hostile.IsCasting || hostile.CastTargetObjectId != player.GameObjectId)
			{
				continue;
			}

			var id = hostile.CastActionId;
			if (!OtherConfiguration.HostileCastingTank.Contains(id) && !IsLearned(id))
			{
				continue;
			}

			var left = hostile.TotalCastTime - hostile.CurrentCastTime;
			var horizon = left + gcd;
			var predicted = TankbusterTable.PredictedShare(id, player, hostile, horizon);

			// The barrier counts only when it still stands then; the earliest barrier's end decides, so a
			// short one beside a long one counts none (HasSurvivingShield).
			var barrier = player.HasSurvivingShield(horizon) ? player.GetObjectShield() / maxHp : 0f;
			var attackType = Service.GetSheet<Lumina.Excel.Sheets.Action>().TryGetRow(id, out var row)
				? row.AttackType.RowId
				: 0u;
			// A cast whose elapsed time went back is a new cast of the same action, even without a frame
			// between the two.
			var identity = (hostile.GameObjectId, id, hostile.TotalCastTime);
			var isNew = !_serials.TryGetValue(identity, out var known) || hostile.CurrentCastTime < known.Elapsed;
			var serial = isNew ? ++_nextSerial : known.Serial;
			_serials[identity] = (serial, hostile.CurrentCastTime);

			_ = _live.Add(identity);
			var cast = new Cast(serial, hostile, id, attackType, left, hostile.TotalCastTime, predicted,
				(player.CurrentHp / maxHp) + barrier, horizon);
			_casts.Add(cast);
			if (isNew)
			{
				Trace(cast);
			}
		}

		_casts.Sort((a, b) => a.Remaining.CompareTo(b.Remaining));
		foreach (var cast in _casts)
		{
			if (cast.Measured)
			{
				SoonestMeasured = cast;
				break;
			}
		}

		// Only the casts still running keep their serial; the next cast of the same action is a new one.
		foreach (var identity in _serials.Keys)
		{
			if (!_live.Contains(identity))
			{
				_ = _serials.Remove(identity);
			}
		}

		// Over every cast now coming at the player, or with none for at least one more GCD: a cover that ends
		// between two casts keeps nothing back for the second.
		var latest = gcd;
		foreach (var cast in _casts)
		{
			latest = Math.Max(latest, cast.Horizon);
		}

		PlayerImpervious = LongestCopy(player, StatusHelper.ImperviousStatus, false) >= latest;
	}

	// One line per cast, written when it is first seen, so the file shows what the plan was built on.
	private static void Trace(Cast cast)
	{
		var name = Service.GetSheet<Lumina.Excel.Sheets.Action>().TryGetRow(cast.ActionId, out var row)
			? row.Name.ExtractText()
			: $"#{cast.ActionId}";
		DefenseTrace.Line($"tankbuster coming at you: {name} #{cast.ActionId} in {cast.Remaining:F1} s - "
			+ (!cast.Measured
				? "not in the tankbuster table"
				: $"takes {cast.Predicted:P0} of max HP under what stands at impact, you have {cast.Budget:P0}"));
	}

	private static bool IsLearned(uint actionId)
	{
		lock (OtherConfiguration.TankbusterPotentialGate)
		{
			return OtherConfiguration.TankbusterPotential.ContainsKey(actionId);
		}
	}
}
