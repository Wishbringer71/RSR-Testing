using ECommons.DalamudServices;
using ECommons.GameHelpers;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// What a tankbuster now being cast at the player is expected to do to him, against what he has to
/// take it - the measurement behind the tankbuster plan (concept 09, "Gemessener Tankbuster: das
/// geringste Mittel").
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
	/// <param name="Source">The enemy casting it.</param>
	/// <param name="ActionId">The action being cast.</param>
	/// <param name="AttackType">The action's attack type row in the game data, which decides which mitigation
	/// counts against it; zero when the row is not readable.</param>
	/// <param name="Remaining">Seconds of the cast still to run.</param>
	/// <param name="Total">The cast's whole length in seconds.</param>
	/// <param name="ByMarker">Not aimed at the player: taken for him by a tankbuster marker on him (see
	/// <see cref="Update"/>).</param>
	/// <param name="SourceId">The caster's id, read with the cast. A reader in a later frame resolves the caster from
	/// the object table by it instead of trusting <paramref name="Source"/>: Dalamud keeps one object per table slot and
	/// points it at whatever occupies the slot when anyone next walks the table (A309).</param>
	/// <param name="Learned">No cast at all: a tankbuster marker on the player, met with the hit learned after it before
	/// (<see cref="TankbusterMarkerWatch.LearnedHitOn"/>, A340). <paramref name="Source"/> is then the player's target,
	/// not known to be the hitter, so nothing laid on it counts; <paramref name="Remaining"/> is the learned time.</param>
	/// <param name="Announced">Known as a tankbuster by the tankbuster list or a learned hit after list or marker. False for
	/// a cast the table knows only from an enemy casting it at its own target, a tank (A452): in a pull that is any trash
	/// cast at the tank, and the rest of the defence is for the blows of the others.</param>
	internal sealed record Cast(int Serial, IBattleChara Source, uint ActionId, uint AttackType, float Remaining,
		float Total, float Predicted, float Budget, float Horizon, bool ByMarker = false, ulong SourceId = 0,
		bool Learned = false, bool Announced = true)
	{
		/// <summary>Measured at all: the table has a figure for the action.</summary>
		public bool Measured => Predicted > 0f;
	}

	private static readonly List<Cast> _casts = [];

	// The player's own actions that put a status on, from RSR's press on (or, for one RSR did not press, from
	// the server's confirmation): action, target, when (monotonic, so a clock change does not stretch
	// anything), whether the server confirmed it, its statuses, how long an own copy of them still ran at
	// that moment, and whether the new one has shown since. Kept while the effect can last.
	private sealed class Execution
	{
		public required uint ActionId { get; init; }
		public required ulong Target { get; init; }
		public required long Tick { get; init; }
		public required bool Confirmed { get; set; }
		public required int CoveredSerial { get; init; }
		public required StatusID[] OnSelf { get; init; }
		public required StatusID[] Where { get; init; }
		public required bool Outward { get; init; }
		public required float PriorOnSelf { get; init; }
		public required float PriorWhere { get; init; }
		public bool StatusSeen { get; set; }
	}

	private static readonly List<Execution> _own = [];
	private static long _readAt;

	// The player's latest request without a cast, at the moment it left for the server - pressed by RSR, by the player
	// or sent from the game's queue -, for the two figures the plan's deadline needs (concept 09, "Spielerweite Sperre
	// und Frist"): the round trip to the effect that carries its sequence back, and how long it kept the next press back.
	// Timed from the press instead, a GCD pressed ahead of the recast's end (CalculatedActionAhead) counted the time it
	// waited in the queue, and its lock was not set yet when the spacing was read (A340).
	private static ushort _timedSequence;
	private static long _timedTick;
	private static bool _timedConfirmed = true;
	private static bool _timedLockSeen = true;

	/// <summary>
	/// Seconds from the player's latest request without a cast leaving to the effect that answers it: how long before
	/// the end of a cast, as the client shows it, a press has to go out to reach the server before the snapshot there.
	/// -1 before the first measurement.
	/// </summary>
	public static float RoundTrip { get; private set; } = -1f;

	/// <summary>
	/// Seconds the player's latest request without a cast kept the next one back: until the animation lock it set ran
	/// out, the server's figure and the delay to it together. -1 before the first measurement.
	/// </summary>
	public static float PressSpacing { get; private set; } = -1f;

	/// <summary>The clock of this forecast: high resolution, monotonic.</summary>
	public static long Now => System.Diagnostics.Stopwatch.GetTimestamp();

	/// <summary>
	/// Seconds since the last reading of the casts: what a reader has to take off their remaining time to
	/// compare it with a status or cooldown read now, on the same clock.
	/// </summary>
	public static float SinceReading => Math.Max(0f, SecondsSince(_readAt, Now));

	/// <summary>
	/// An own action pressed or gone out whose new status has not shown yet.
	/// </summary>
	/// <param name="Cover">Seconds it still lasts by its duration from the press; zero when there is no such record.</param>
	/// <param name="Tick">When it was pressed (<see cref="Now"/>).</param>
	/// <param name="PriorLeft">Seconds the own copy that ran at the press still runs; negative when there was none.</param>
	/// <param name="CoveredSerial">The soonest measured cast whose horizon its duration reached when it went out
	/// (inside its window); zero when none.</param>
	public readonly record struct Pending(float Cover, long Tick, float PriorLeft, int CoveredSerial);
	private static readonly Dictionary<uint, (StatusID[] OnSelf, StatusID[] Where, bool Outward)> _factsOfAction = [];
	private static ICustomRotation? _statusesFor;
	private static readonly Dictionary<(ulong Source, uint ActionId, float Total), (int Serial, float Elapsed)> _serials = [];
	private static readonly HashSet<(ulong Source, uint ActionId, float Total)> _live = [];

	// Casts taken for the player by a marker on him, with the elapsed time last seen: they stay his while they run,
	// also once the marker has left the queue; a cast whose elapsed time went back is a new one.
	private static readonly Dictionary<(ulong Source, uint ActionId, float Total), float> _byMarker = [];

	// When each tankbuster action was last seen coming at the player, for the trace of the hit (TookCastFor).
	private static readonly Dictionary<uint, long> _lastComing = [];
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
		_ = Follow(actionId, targetId, false);
	}

	/// <summary>
	/// A request of the player left for the server without starting a cast (<see cref="ActionManagerEx"/>: its sequence
	/// changed) - the moment the round trip and the spacing are measured from. A queued or refused press changes no
	/// sequence and is not one. Called on the game thread.
	/// </summary>
	internal static void RecordRequest(ushort sequence)
	{
		_timedSequence = sequence;
		_timedTick = Now;
		_timedConfirmed = false;
		_timedLockSeen = false;
	}

	/// <summary>
	/// The effect of one of the player's requests arrived and carries its sequence back: the round trip of the latest
	/// request, if it is that one. Called on the game thread.
	/// </summary>
	internal static void RecordRequestAnswered(ushort sequence)
	{
		if (!_timedConfirmed && sequence == _timedSequence)
		{
			RoundTrip = SecondsSince(_timedTick, Now);
			_timedConfirmed = true;
		}
	}

	/// <summary>
	/// An action of the player went out, as the effect handler saw it: it confirms the press it belongs to,
	/// or - pressed by hand or by another tool - starts its own record. Called on the game thread.
	/// </summary>
	public static void RecordOwnAction(uint actionId, ulong targetId)
	{
		var now = Now;
		Execution? pressed = null;
		foreach (var execution in _own)
		{
			if (!execution.Confirmed && execution.ActionId == actionId
				&& SecondsSince(execution.Tick, now) <= DataCenter.DefaultGCDTotal
				&& (pressed == null || execution.Tick > pressed.Tick))
			{
				pressed = execution;
			}
		}

		if (pressed != null)
		{
			pressed.Confirmed = true;
			return;
		}

		var started = Follow(actionId, targetId, true);
		if (started == null)
		{
			return;
		}

		// The client may have put the new copy on before this handler ran: a copy with more than the
		// duration less one GCD left cannot be an older one, so it is already the new status.
		var fresh = DefensiveValues.DurationOf(actionId) - DataCenter.DefaultGCDTotal;
		started.StatusSeen = started.PriorOnSelf > fresh || started.PriorWhere > fresh;
	}

	// Starts the record of one of the player's own rated mitigations or barriers with a known duration, or of an
	// invulnerability, with how long an own copy of its statuses ran at that moment; null for what is not
	// followed - damage buffs and damage over time among them, which no plan asks about. Party tools (a barrier
	// or mitigation spread over the party) never enter a tankbuster plan and are not followed.
	private static Execution? Follow(uint actionId, ulong targetId, bool confirmed)
	{
		var now = Now;
		_ = _own.RemoveAll(e => SecondsSince(e.Tick, now) > DefensiveValues.DurationOf(e.ActionId));
		var player = Player.Object;
		var lasts = DefensiveValues.DurationOf(actionId);
		if (player == null || lasts <= 0f || !DataCenter.InCombat || DataCenter.Role != JobRole.Tank)
		{
			return null;
		}

		var value = DefensiveValues.For(actionId);
		var (onSelf, where, outward) = FactsOfAction(actionId);
		if ((onSelf.Length == 0 && where.Length == 0) || (outward && value.ProtectsBearer)
			|| (value == default && !GivesInvulnerability(onSelf)))
		{
			return null;
		}

		// Inside its window: its duration reaches the horizon of the soonest measured cast as it goes out. Read
		// now, so the cast's time is taken to now as well.
		var soonest = SoonestMeasured;
		var execution = new Execution
		{
			ActionId = actionId,
			Target = targetId,
			Tick = now,
			Confirmed = confirmed,
			CoveredSerial = soonest != null && lasts >= soonest.Horizon - SinceReading ? soonest.Serial : 0,
			OnSelf = onSelf,
			Where = where,
			Outward = outward,
			PriorOnSelf = OwnCopy(player, onSelf),
			PriorWhere = LongestOwnCopy(player, targetId, where, outward, float.MaxValue),
		};
		_own.Add(execution);
		return execution;
	}

	/// <summary>
	/// For the gap between one of the player's own actions going out and its status showing: its latest
	/// record while the new status has not been seen yet - for at most one GCD while the server has not
	/// confirmed it; <c>default</c> once the status has shown (from then on the status itself tells, also when
	/// it ends early - a barrier broken, a mitigation dispelled). Measured now, on <see cref="Now"/>. Whether
	/// the status showed is recorded every cycle in <see cref="Update"/>. <paramref name="anyTarget"/>: the
	/// action counts whatever it was aimed at (Holmgang on an enemy, Reprisal around the player).
	/// </summary>
	public static Pending OwnPending(uint actionId, bool anyTarget)
	{
		var player = Player.Object;
		var lasts = DefensiveValues.DurationOf(actionId);
		if (player == null || lasts <= 0f)
		{
			return default;
		}

		// A press the server has not confirmed within a GCD did not go out.
		var now = Now;
		var gcd = DataCenter.DefaultGCDTotal;
		Execution? latest = null;
		foreach (var execution in _own)
		{
			if (execution.ActionId == actionId
				&& (anyTarget || execution.Target == player.GameObjectId)
				&& (execution.Confirmed || SecondsSince(execution.Tick, now) <= gcd)
				&& (latest == null || execution.Tick > latest.Tick))
			{
				latest = execution;
			}
		}

		if (latest == null || latest.StatusSeen)
		{
			return default;
		}

		var since = SecondsSince(latest.Tick, now);
		var prior = Math.Max(latest.PriorOnSelf, latest.PriorWhere);
		return new Pending(Math.Max(0f, lasts - since), latest.Tick, prior < 0f ? -1f : prior - since, latest.CoveredSerial);
	}

	private static bool GivesInvulnerability(StatusID[] statuses)
	{
		foreach (var status in statuses)
		{
			if (Array.IndexOf(StatusHelper.InvulnerabilityStatus, status) >= 0)
			{
				return true;
			}
		}

		return false;
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

	// The action's own statuses: where it lands, the statuses its effect text is tied to and its setting's
	// TargetStatusProvide (The Blackest Night's barrier has no mitigation figure, only a status); on its user,
	// its setting's StatusProvide - only for an action whose effect text names none, the invulnerabilities,
	// because for the big mitigations that setting is the shared lock-out list (StatusHelper.RampartStatus), in
	// which a standing Rampart would look like Damnation's new status. And whether it works around the player
	// (an effect radius, game data). Kept per action until the rotation changes.
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

		var namedByEffectText = where.Count > 0;
		var actions = rotation?.AllBaseActions;
		if (actions != null)
		{
			foreach (var action in actions)
			{
				if (action == null || (action.ID != actionId && action.AdjustedID != actionId))
				{
					continue;
				}

				if (!namedByEffectText)
				{
					onSelf.AddRange(action.Setting.StatusProvide ?? []);
				}

				where.AddRange(action.Setting.TargetStatusProvide ?? []);
			}
		}

		var outward = Service.GetSheet<Lumina.Excel.Sheets.Action>().TryGetRow(actionId, out var row) && row.EffectRange > 0;
		(StatusID[] OnSelf, StatusID[] Where, bool Outward) result = ([.. onSelf], [.. where], outward);
		_factsOfAction[actionId] = result;
		return result;
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

	/// <summary>
	/// The shortest any copy of the statuses still runs on <paramref name="chara"/>, read like
	/// <see cref="LongestCopy"/>: from the status list itself, guarded, copies without an end left out; -1 when
	/// there is none.
	/// </summary>
	public static float ShortestCopy(IBattleChara? chara, StatusID[] statuses)
	{
		var shortest = float.MaxValue;
		if (chara == null)
		{
			return -1f;
		}

		try
		{
			var list = chara.StatusList;
			if (list == null)
			{
				return -1f;
			}

			foreach (var status in list)
			{
				if (status == null || status.RemainingTime <= 0f)
				{
					continue;
				}

				foreach (var id in statuses)
				{
					if (status.StatusId == (uint)id)
					{
						shortest = Math.Min(shortest, status.RemainingTime);
						break;
					}
				}
			}
		}
		catch (Exception)
		{
			// A status list that throws (an object going away) answers nothing.
			return -1f;
		}

		return shortest == float.MaxValue ? -1f : shortest;
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

	/// <summary>Seconds between two readings of <see cref="Now"/>.</summary>
	internal static float SecondsSince(long tick, long now)
	{
		return (float)((double)(now - tick) / System.Diagnostics.Stopwatch.Frequency);
	}

	// The tankbuster hits on the player in this fight, on this forecast's clock; a hit in several parts within a GCD
	// is one. Any role: a damage dealer holding an enemy takes its tankbusters too. Cleared out of combat (A361).
	private static readonly List<long> _hitsOnPlayer = [];
	private static readonly object _hitsGate = new();

	/// <summary>
	/// A tankbuster reached the player: a listed one, or any enemy action after a tankbuster marker on him. Called from
	/// the effect handler, on the game thread.
	/// </summary>
	internal static void RecordHitOnPlayer()
	{
		var now = Now;
		lock (_hitsGate)
		{
			if (_hitsOnPlayer.Count == 0 || SecondsSince(_hitsOnPlayer[^1], now) > DataCenter.DefaultGCDTotal)
			{
				_hitsOnPlayer.Add(now);
			}
		}
	}

	/// <summary>
	/// Seconds until the next tankbuster on the player is due. BossModReborn's prediction while it has one - for a tank,
	/// or for anyone else while its mask names him or no tank is in the party, as everywhere its tankbuster is read
	/// (A220); otherwise the spacing measured in this fight: the last hit plus the shortest gap seen, the one from the
	/// start of the fight to the first hit included. Positive infinity when none is due - none in this fight yet, or
	/// none for two such gaps after the last, the fight having moved on -, zero when one is due now.
	/// </summary>
	/// <remarks>
	/// Detection only: the reader decides what a mitigation does with it (<c>BaseAction</c>, "KeptForTheNextTankbuster").
	/// The fight's own record before any prediction would serve the owner's preference for what the game shows at
	/// runtime; BossModReborn comes first only because a prediction names this fight's next hit, and the record only
	/// its past. Without either, nothing is known, and nothing is held: in the model the cost of that guess falls only
	/// where the healer died before the boss's first tankbuster (tank_pull_defense_model.py).
	/// </remarks>
	internal static float NextOnPlayerIn
	{
		get
		{
			var predicted = DataCenter.BMRNextTankbusterIn;
			if (Service.Config.UseBmrTimeline && predicted is > 0f and < float.MaxValue
				&& (DataCenter.BMRTankbusterHitsPlayer ?? (DataCenter.Role == JobRole.Tank || DataCenter.PartyTank == null)))
			{
				return predicted;
			}

			var now = Now;
			lock (_hitsGate)
			{
				if (_hitsOnPlayer.Count == 0)
				{
					return float.PositiveInfinity;
				}

				// The start of the fight, on the same clock: the combat time is the plugin's own count of it.
				var gap = SecondsSince(now - (long)(DataCenter.CombatTimeRaw * System.Diagnostics.Stopwatch.Frequency), _hitsOnPlayer[0]);
				for (var i = 1; i < _hitsOnPlayer.Count; i++)
				{
					gap = Math.Min(gap, SecondsSince(_hitsOnPlayer[i - 1], _hitsOnPlayer[i]));
				}

				if (gap <= 0f)
				{
					return float.PositiveInfinity;
				}

				var since = SecondsSince(_hitsOnPlayer[^1], now);
				return since > 2f * gap ? float.PositiveInfinity : Math.Max(0f, gap - since);
			}
		}
	}

	/// <summary>Recomputes the figures. Called once per framework cycle, on the game thread.</summary>
	public static void Update()
	{
		_casts.Clear();
		SoonestMeasured = null;
		PlayerImpervious = false;
		_readAt = Now;

		if (!DataCenter.InCombat)
		{
			lock (_hitsGate)
			{
				_hitsOnPlayer.Clear();
			}
		}

		// The timed press has stopped holding the next one back once its animation lock has run out; read once a
		// frame, after the rotation, so the frame of the press itself still shows the lock the client set.
		if (!_timedLockSeen && ECommons.GameHelpers.Player.AnimationLock <= 0f)
		{
			PressSpacing = SecondsSince(_timedTick, _readAt);
			_timedLockSeen = true;
		}

		var player = Player.Object;
		if (!DataCenter.InCombat || player == null || player.IsDead || DataCenter.Role != JobRole.Tank)
		{
			_own.Clear();
			_serials.Clear();
			_byMarker.Clear();
			_lastComing.Clear();
			return;
		}

		_live.Clear();
		RecordStatusesSeen(player);

		var gcd = DataCenter.DefaultGCDTotal;
		var maxHp = (float)Math.Max(1u, player.MaxHp);
		var markerOnPlayer = DataCenter.IsTankbusterVfxOnPlayer();
		// The helpers too (A303): a tankbuster an untargetable helper casts at the player is planned like any other.
		foreach (var hostile in DataCenter.AreaCasters)
		{
			if (hostile == null || !hostile.IsCasting)
			{
				continue;
			}

			var id = hostile.CastActionId;
			var listed = OtherConfiguration.HostileCastingTank.Contains(id);
			var measured = TableKnows(id, out var fromAimOnly);
			// A cast learned only from an enemy aiming it at its own target (A452) is planned for the same way: at the
			// player, by a targetable enemy attacking him, as it was learned.
			var aimedOnly = !listed && measured && fromAimOnly && hostile.CastTargetObjectId == player.GameObjectId
				&& hostile.TargetObjectId == player.GameObjectId && !DataCenter.AreaCastHelperIds.Contains(hostile.GameObjectId);
			if (!listed && !(measured && !fromAimOnly) && !aimedOnly)
			{
				continue;
			}

			// A tankbuster aimed at no other party member - at the caster itself, at the ground - while a tankbuster
			// marker (not falsified) stands on the player is his: many fights mark the tank and let the boss or a
			// helper cast without a target (A308). Once taken for him, the cast stays his while it runs.
			var byMarker = false;
			if (hostile.CastTargetObjectId != player.GameObjectId)
			{
				var marked = (hostile.GameObjectId, id, hostile.TotalCastTime);
				byMarker = (_byMarker.TryGetValue(marked, out var seenAt) && hostile.CurrentCastTime >= seenAt)
					|| (markerOnPlayer && !AimedAtOtherMember(hostile.CastTargetObjectId, player.GameObjectId));
				if (!byMarker)
				{
					continue;
				}

				_byMarker[marked] = hostile.CurrentCastTime;
			}

			var left = hostile.TotalCastTime - hostile.CurrentCastTime;
			var horizon = left + gcd;
			var predicted = TankbusterTable.PredictedShare(id, player, hostile, horizon);
			var barrier = BarrierAt(player, horizon, maxHp);
			var attackType = AttackTypeOf(id);
			// A cast whose elapsed time went back is a new cast of the same action, even without a frame
			// between the two.
			var identity = (hostile.GameObjectId, id, hostile.TotalCastTime);
			var isNew = !_serials.TryGetValue(identity, out var known) || hostile.CurrentCastTime < known.Elapsed;
			var serial = isNew ? ++_nextSerial : known.Serial;
			_serials[identity] = (serial, hostile.CurrentCastTime);

			_ = _live.Add(identity);
			_lastComing[id] = _readAt;
			var cast = new Cast(serial, hostile, id, attackType, left, hostile.TotalCastTime, predicted,
				(player.CurrentHp / maxHp) + barrier, horizon, byMarker, hostile.GameObjectId, Announced: !aimedOnly);
			_casts.Add(cast);
			if (isNew)
			{
				Trace(cast);
			}
		}

		// A marker on the player with no cast coming at him at all: met with the hit that followed the same marker
		// before, once the table has measured it (A340). Before, the table learned such a hit and the plan never used it,
		// and the invulnerability came only below the line - too late for a hit that takes a full tank under it.
		if (_casts.Count == 0 && markerOnPlayer)
		{
			AddLearnedCast(player, gcd, maxHp);
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

		foreach (var identity in _byMarker.Keys)
		{
			if (!_live.Contains(identity))
			{
				_ = _byMarker.Remove(identity);
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

	// The learned hit as a cast for the plan. Its hitter is not known - the boss, or a helper no debuff on the boss
	// reaches -, so it is looked at from the player's target, and only for the plan's figures: nothing on the target
	// counts against the hit (TankbusterTable.PredictedShare without an attacker), so the plan errs to more mitigation.
	// Without a target there is no plan, as before.
	private static void AddLearnedCast(IBattleChara player, float gcd, float maxHp)
	{
		if (!TankbusterMarkerWatch.LearnedHitOn(player.GameObjectId, out var id, out var remaining, out var delay)
			|| Svc.Targets.Target is not IBattleChara aim || !aim.IsEnemy())
		{
			return;
		}

		// Past the learned time the hit is due any moment, or the marker announced something else this time: nothing to
		// plan for, and the defence of the marker goes on as without a plan.
		if (remaining <= 0f)
		{
			return;
		}

		var horizon = remaining + gcd;
		var predicted = TankbusterTable.PredictedShare(id, player, null, horizon);
		if (predicted <= 0f)
		{
			return;
		}

		var identity = (player.GameObjectId, id, delay);
		var elapsed = delay - remaining;
		var isNew = !_serials.TryGetValue(identity, out var known) || elapsed < known.Elapsed;
		var serial = isNew ? ++_nextSerial : known.Serial;
		_serials[identity] = (serial, elapsed);
		_ = _live.Add(identity);
		_lastComing[id] = _readAt;

		var cast = new Cast(serial, aim, id, AttackTypeOf(id), remaining, delay, predicted,
			(player.CurrentHp / maxHp) + BarrierAt(player, horizon, maxHp), horizon, true, aim.GameObjectId, true);
		_casts.Add(cast);
		if (isNew)
		{
			Trace(cast);
		}
	}

	// The barrier counts only when it still stands at the horizon; the earliest barrier's end decides, so a short one
	// beside a long one counts none, and one without a known status counts none (as in StatusHelper.HasSurvivingShield).
	// Read from the status list itself, like every status here. Also read for another member (DataCenter.AimedTankbusterLeaves).
	internal static float BarrierAt(IBattleChara chara, float horizon, float maxHp)
	{
		return chara.GetObjectShield() > 0 && ShortestCopy(chara, StatusHelper.ShieldStatus) >= horizon
			? chara.GetObjectShield() / maxHp
			: 0f;
	}

	private static uint AttackTypeOf(uint actionId)
	{
		return Service.GetSheet<Lumina.Excel.Sheets.Action>().TryGetRow(actionId, out var row) ? row.AttackType.RowId : 0u;
	}

	// One line per cast, written when it is first seen, so the file shows what the plan was built on.
	private static void Trace(Cast cast)
	{
		var name = Service.GetSheet<Lumina.Excel.Sheets.Action>().TryGetRow(cast.ActionId, out var row)
			? row.Name.ExtractText()
			: $"#{cast.ActionId}";
		DefenseTrace.Line($"tankbuster coming at you{(cast.Learned ? " (learned from the marker on you)" : cast.ByMarker ? " (by the marker on you)" : !cast.Announced ? " (known only as a cast at you, nothing else held)" : string.Empty)}:"
			+ $" {name} #{cast.ActionId} in {cast.Remaining:F1} s - "
			+ (!cast.Measured
				? "not in the tankbuster table"
				: $"takes {cast.Predicted:P0} of max HP under what stands at impact, you have {cast.Budget:P0}"));
	}

	/// <summary>
	/// Whether a cast of <paramref name="actionId"/> was taken as coming at the player until at most one GCD ago -
	/// the time the hit needs to arrive after its cast. For the trace of a hit after a tankbuster marker: one without
	/// is a marker the plan had neither a cast nor a learned hit for (concept 09, "Ein Marker ohne Wirken").
	/// </summary>
	public static bool TookCastFor(uint actionId)
	{
		return _lastComing.TryGetValue(actionId, out var tick) && SecondsSince(tick, Now) <= DataCenter.DefaultGCDTotal;
	}

	// Whether a cast aimed at this object is aimed at a party member other than the player: then it is that member's.
	private static bool AimedAtOtherMember(ulong targetId, ulong playerId)
	{
		if (targetId == 0 || targetId == playerId)
		{
			return false;
		}

		foreach (var member in DataCenter.PartyMembers)
		{
			if (member != null && member.GameObjectId == targetId)
			{
				return true;
			}
		}

		return false;
	}

	internal static bool IsLearned(uint actionId)
	{
		// A raidwide the table took in before A337 stays in the file - a save merges with it and never removes -
		// and is passed over here instead (TankbusterTable.IsPartyWide).
		if (TankbusterTable.IsPartyWide(actionId))
		{
			return false;
		}

		// A reading learned only from casts an enemy aimed at its own target is no known tankbuster: the readers that open
		// the defence or call for help would take every learned trash cast in a pull for one (A452, A395).
		lock (OtherConfiguration.TankbusterPotentialGate)
		{
			return OtherConfiguration.TankbusterPotential.TryGetValue(actionId, out var reading) && !reading.AimedOnly;
		}
	}

	/// <summary>
	/// Whether the table holds a reading of <paramref name="actionId"/> that is no raidwide, and whether it knows it only
	/// from the landings of an enemy's cast at its own target, a tank (<see cref="TankbusterReading.AimedOnly"/>, A452):
	/// the plan reads such a cast at the player and holds nothing else for it. One lookup for the forecast's loop.
	/// </summary>
	private static bool TableKnows(uint actionId, out bool aimedOnly)
	{
		aimedOnly = false;
		if (TankbusterTable.IsPartyWide(actionId))
		{
			return false;
		}

		lock (OtherConfiguration.TankbusterPotentialGate)
		{
			if (!OtherConfiguration.TankbusterPotential.TryGetValue(actionId, out var reading))
			{
				return false;
			}

			aimedOnly = reading.AimedOnly;
			return true;
		}
	}
}
