using System.Collections.Concurrent;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// What a Sleep or Repose aimed at an enemy's cast did, as the fight shows it: whether it landed, whether the cast
/// stopped or went on, and how long a sleep takes from the end of its cast to its effect. The experiment the owner
/// asked for on 18.10.2026 (concept 08, "Ein gegnerisches Wirken stoppen"): can Sleep stop enemy casts, and which?
/// </summary>
/// <remarks>
/// <para>No game text says that Sleep interrupts a cast. Its status (row 3) locks actions and movement as Stun
/// (row 2) does, and a stun stops the cast of an enemy that is not immune (the tree's Low Blow rule); community
/// sources say the same of Sleep (Final Fantasy Wiki: "Sleep can potentially be used to interrupt casting if the
/// target is vulnerable to the status"). His traces of 19.-22.10.2026 show it stopping casts the game marks
/// uninterruptible, when it lands early enough (A334).</para>
///
/// <para>What it learns is kept over every session, after his model, the learned area table (23.10.2026): the enemy
/// kinds that resisted a sleep as a status (<see cref="OtherConfiguration.SleepResistantKinds"/>), and per enemy
/// action the casts a sleep stopped and the ones that went off (<see cref="OtherConfiguration.SleepOutcomes"/>). The
/// conclusions - how long a sleep takes to stop a cast, how much of a cast it has to leave, which action it cannot
/// stop - are worked out from those observations at each decision, so a later observation corrects an earlier
/// conclusion instead of being shut out by it. Each outcome is a line in the defence trace.</para>
///
/// <para>The three locks among them - a resistant kind, a sleep limit, an action a sleep did not stop - hold in the game
/// version that saw them (concept 13, "Stufe 4", 4b): one from an earlier version, or from before stage 4, gets one try
/// in the running one; a sleep that lands refutes the kind or the limit, one that stops a cast refutes the action. The
/// marks for that are <see cref="OtherConfiguration.SleepLocks"/>, the logic <see cref="SleepLockRules"/>.</para>
///
/// <para>Recognition and its own measure only; the rule that presses the sleep decides
/// (<c>CustomRotation.StopEnemyCast</c>).</para>
/// </remarks>
public static class SleepInterruptWatch
{
	/// <summary>One target of the player's Sleep or Repose.</summary>
	/// <param name="Target">The target's object id.</param>
	/// <param name="Applied">A status was put on it.</param>
	/// <param name="StatusResisted">The game reported the status as fully resisted - an immunity of the kind.</param>
	/// <param name="Unaffected">The game reported the action as resisted or the target as invulnerable - which says
	/// nothing about the kind outside that moment.</param>
	internal readonly record struct Observation(ulong Target, bool Applied, bool StatusResisted, bool Unaffected);

	private sealed record Aim(ulong EnemyId, string EnemyName, uint BaseId, uint ActionId, string ActionName, DateTime CastEnd,
		bool Interruptible)
	{
		/// <summary>The player began the sleep's cast on this enemy.</summary>
		public bool Cast { get; init; }

		/// <summary>The locks taken over from an earlier game version that a sleep on this aim tries (concept 13, "Stufe 4").</summary>
		public IReadOnlyList<SleepLockKey> Tries { get; init; } = [];
	}

	private sealed class Watch
	{
		public required Aim Aim;
		public DateTime AppliedAt;
		public float LeftAtLanding;
		public DateTime ExpectedEnd;
		public bool CastGone;
		public DateTime CastGoneAt;

		// How long before its time the cast bar ended, and whether a stun stood on the enemy then.
		public float EndedEarlyBy;
		public bool StunnedAtEnd;
	}

	// Kinds that could not be slept this session without that proving an immunity (an invulnerable phase).
	private static readonly ConcurrentDictionary<uint, byte> _unaffectedThisSession = new();

	// Sleeps that landed on each enemy in this fight, from any source, and the enemies that resisted one after
	// earlier sleeps - the halving of repeated sleeps ending in a temporary immunity. Cleared when the fight ends.
	private static readonly ConcurrentDictionary<ulong, int> _sleptThisFight = new();
	private static readonly ConcurrentDictionary<ulong, byte> _diminishedThisFight = new();

	private static Aim? _aim;

	// Why the rule did not choose the sleep for the aim it had chosen it for: the first reason since it last chose it, and
	// the last one. The first fell when the GCD went elsewhere; "the cast ended" follows every lapsed sleep and, kept alone,
	// hid it (A738). Null while the rule still chooses it.
	private static string? _lapse;
	private static string? _lastLapse;

	private static Watch? _watch;
	private static uint _pressedAction;
	private static DateTime _pressedAt;
	private static float _pressedCastTime;

	// The end of the player's own sleep cast, as the client shows it.
	private static uint _castAction;
	private static DateTime _castEnd;

	// The cast a job's own stopper was last written for, so the trace names it once.
	private static (ulong Enemy, uint Action) _jobStopTraced;

	// The conclusions drawn from the stored outcomes, worked out again whenever they change.
	private static Dictionary<uint, SleepReading>? _derivedFrom;
	private static int _derivedVersion = -1;
	private static int _version;
	private static float _holdTime = -1f;
	private static float _landingMargin;

	/// <summary>
	/// Seconds from the end of the sleep's own cast to its effect on the server's answer, last measured; negative
	/// before the first measurement. Measured anew each session: it follows the connection.
	/// </summary>
	public static float Lag { get; private set; } = -1f;

	/// <summary>
	/// Seconds of the enemy's cast a sleep has to leave when it lands: the most a sleep had left when a cast still went
	/// off, among the sleeps that came too late - those that landed sooner before the end than the longest a cut-off
	/// cast took to stop - plus one frame. Zero until one has.
	/// </summary>
	/// <remarks>
	/// A sleep does not stop a cast the moment its effect arrives. In his traces of 19.-22.10.2026 every cast it cut
	/// short ended 0.7 to 1.2 s after the sleep landed, and both that went off had landed 0.5 s before their end
	/// (A334). Such a cast proves the timing wrong, not the action unstoppable.
	/// </remarks>
	public static float LandingMargin
	{
		get
		{
			Derive();
			return _landingMargin;
		}
	}

	/// <summary>Whether <paramref name="actionId"/> is the casters' Sleep or the healers' Repose.</summary>
	/// <param name="actionId">The action.</param>
	/// <returns>True for either.</returns>
	public static bool IsSleepAction(uint actionId)
		=> actionId is (uint)ActionID.SleepPvE or (uint)ActionID.ReposePvE;

	/// <summary>
	/// Whether a sleep could still land on <paramref name="enemy"/>: nothing of this fight or session bars it
	/// (<see cref="IsOpenToSleep"/>), and no stored lock on its kind stands - one taken over from an earlier game version
	/// counts as standing here; only the rule's own try passes it (<see cref="LocksOn"/>).
	/// </summary>
	/// <param name="enemy">The enemy.</param>
	/// <returns>True while a sleep can still land.</returns>
	public static bool CanStillBeSlept(IBattleChara enemy)
	{
		return IsOpenToSleep(enemy) && LocksOn(enemy, null, withCast: false) == LockState.None;
	}

	/// <summary>
	/// Whether nothing of this fight or session bars a sleep on <paramref name="enemy"/>: not a boss (BNpcBase rank), no
	/// Sleep Resistance (row 1354, "Immune to sleep effects"), not of a kind a sleep could not reach this session, not
	/// worn down by repeated sleeps in this fight, and no sleep has just landed on it. The stored locks are
	/// <see cref="LocksOn"/>'s.
	/// </summary>
	/// <param name="enemy">The enemy.</param>
	/// <returns>True when nothing of the fight or session bars it.</returns>
	internal static bool IsOpenToSleep(IBattleChara enemy)
	{
		return enemy != null && !enemy.IsBossFromIcon() && !enemy.HasStatus(false, StatusID.SleepResistance)
			&& !_unaffectedThisSession.ContainsKey(enemy.BaseId)
			&& !_diminishedThisFight.ContainsKey(enemy.GameObjectId)
			// An enemy a sleep has just landed on is asleep before its status shows; a second sleep on the same cast
			// is a GCD for nothing (trace of 21.10.2026, A334).
			&& _watch?.Aim.EnemyId != enemy.GameObjectId;
	}

	/// <summary>
	/// The stored locks on a sleep at <paramref name="enemy"/> (concept 13, "Stufe 4", 4b): its kind resisted a first
	/// sleep; the sleeps that landed on it in this fight reached the limit learned for its kind, otherwise the lowest
	/// learned for any; and with <paramref name="withCast"/>, a sleep did not stop the action it casts.
	/// <see cref="LockState.Holds"/> when one holds in the running game version, its one try there spent, or the version
	/// unreadable; <see cref="LockState.TakenOver"/> when only locks from an earlier version or from before stage 4 stand,
	/// each listed in <paramref name="tries"/>.
	/// </summary>
	/// <remarks>
	/// Repeated sleeps on one enemy halve until it resists for a while, counted apart from stuns (community sources:
	/// Square Enix forum, 2013 and 2016, "30s, 15s, 8s, then immune"; no game text, and the limit is not set here but
	/// learned: <see cref="OtherConfiguration.SleepDiminishing"/>). The game shows neither the count nor the immunity as
	/// a status, so the sleeps that landed in this fight are counted here, as StunWatch counts stuns.
	/// </remarks>
	/// <param name="enemy">The enemy.</param>
	/// <param name="tries">Filled with the taken-over locks a sleep now would try; null when not wanted.</param>
	/// <param name="withCast">Whether the lock on the action it casts counts.</param>
	/// <returns>The strongest lock standing.</returns>
	internal static LockState LocksOn(IBattleChara enemy, List<SleepLockKey>? tries, bool withCast = true)
	{
		tries?.Clear();
		var state = LockState.None;
		var game = OtherConfiguration.GameVersion;
		var slept = _sleptThisFight.GetValueOrDefault(enemy.GameObjectId);
		var action = withCast && enemy.IsCasting ? enemy.CastActionId : 0u;
		var unstoppable = action != 0 && IsUnstoppable(action);
		lock (OtherConfiguration.SleepStoreGate)
		{
			var marks = OtherConfiguration.SleepLocks;
			Add(SleepLockRules.KindState(OtherConfiguration.SleepResistantKinds, marks, enemy.BaseId, game),
				new SleepLockKey(SleepLockType.Kind, enemy.BaseId));
			if (slept > 0 && SleepLockRules.LimitFor(OtherConfiguration.SleepDiminishing, marks, enemy.BaseId, game, out var limitState) is { } limit
				&& slept >= limit)
			{
				Add(limitState, new SleepLockKey(SleepLockType.Limit, enemy.BaseId));
			}

			Add(SleepLockRules.ActionState(unstoppable, marks, action, game), new SleepLockKey(SleepLockType.Action, action));
		}

		return state;

		void Add(LockState found, SleepLockKey key)
		{
			if (found == LockState.TakenOver)
			{
				tries?.Add(key);
			}

			if (found > state)
			{
				state = found;
			}
		}
	}

	/// <summary>The sleep locks in the trace's head line: per type, how many hold, how many are taken over, how many are refuted.</summary>
	internal static string LockSummary()
	{
		var game = OtherConfiguration.GameVersion;
		lock (OtherConfiguration.SleepStoreGate)
		{
			var marks = OtherConfiguration.SleepLocks;
			int[] kinds = [0, 0, 0], limits = [0, 0, 0], actions = [0, 0, 0];
			foreach (var kind in OtherConfiguration.SleepResistantKinds.Keys)
			{
				kinds[(int)SleepLockRules.KindState(OtherConfiguration.SleepResistantKinds, marks, kind, game)]++;
			}

			foreach (var kind in OtherConfiguration.SleepDiminishing.Keys)
			{
				if (!marks.Limits.ContainsKey(kind))
				{
					limits[(int)LockState.TakenOver]++;
				}
			}

			foreach (var mark in marks.Limits.Values)
			{
				limits[mark.Refuted ? (int)LockState.None : game != null && mark.Game == game ? (int)LockState.Holds : (int)LockState.TakenOver]++;
			}

			foreach (var action in OtherConfiguration.SleepOutcomes.Keys)
			{
				if (IsUnstoppable(action))
				{
					actions[(int)SleepLockRules.ActionState(true, marks, action, game)]++;
				}
			}

			const int holds = (int)LockState.Holds, takenOver = (int)LockState.TakenOver, refuted = (int)LockState.None;
			return $"kinds {kinds[holds]} hold, {kinds[takenOver]} taken over, {kinds[refuted]} refuted;"
				+ $" limits {limits[holds]} hold, {limits[takenOver]} taken over, {limits[refuted]} refuted;"
				+ $" actions a sleep did not stop {actions[holds]} hold, {actions[takenOver]} taken over (game version {game ?? "NOT READABLE"})";
		}
	}

	// The locks a list of keys names, for the trace.
	private static string Describe(IReadOnlyList<SleepLockKey> keys)
	{
		var text = new System.Text.StringBuilder();
		foreach (var key in keys)
		{
			_ = text.Append(text.Length > 0 ? ", " : string.Empty).Append(key.Type switch
			{
				SleepLockType.Kind => $"kind {key.Id} resisted",
				SleepLockType.Limit => $"sleep limit of kind {key.Id}",
				_ => $"action #{key.Id} not stopped",
			});
		}

		return text.ToString();
	}

	/// <summary>
	/// Counts the sleeps that landed on each target of a sleep from any source - the player's or another player's.
	/// Called by the effect handler for every set of Sleep or Repose, on the game thread.
	/// </summary>
	/// <param name="targets">Its targets.</param>
	internal static void CountApplications(IReadOnlyList<Observation> targets)
	{
		for (var i = 0; i < targets.Count; i++)
		{
			if (!targets[i].Applied)
			{
				continue;
			}

			var before = _sleptThisFight.GetValueOrDefault(targets[i].Target);
			_sleptThisFight.AddOrUpdate(targets[i].Target, 1, (_, count) => count + 1);
			if (ECommons.DalamudServices.Svc.Objects.SearchById(targets[i].Target) is IBattleChara enemy)
			{
				RefuteLocks(enemy, before);
			}
		}
	}

	// A sleep that landed, from any source, refutes a lock that said it would not (concept 13, "Stufe 4", 4b): its kind
	// stored as resistant, or a limit at or below the sleeps it had taken in this fight.
	private static void RefuteLocks(IBattleChara enemy, int sleptBefore)
	{
		var game = OtherConfiguration.GameVersion;
		bool kind, limit;
		lock (OtherConfiguration.SleepStoreGate)
		{
			kind = SleepLockRules.RefuteKind(OtherConfiguration.SleepResistantKinds, OtherConfiguration.SleepLocks, enemy.BaseId, game);
			limit = SleepLockRules.RefuteLimit(OtherConfiguration.SleepDiminishing, OtherConfiguration.SleepLocks, enemy.BaseId, sleptBefore, game);
		}

		if (!kind && !limit)
		{
			return;
		}

		_ = OtherConfiguration.SaveSleepLocks();
		if (kind)
		{
			DefenseTrace.Line($"sleep landed on {enemy.Name.TextValue} (kind {enemy.BaseId}), a kind stored as resistant: the lock is refuted"
				+ $" (game version {game ?? "NOT READABLE"}), its casts are slept again");
		}

		if (limit)
		{
			DefenseTrace.Line($"sleep landed on {enemy.Name.TextValue} (kind {enemy.BaseId}) after {sleptBefore} sleep(s) in this fight, at or above"
				+ $" its sleep limit: the limit is refuted, it lies above {sleptBefore} (game version {game ?? "NOT READABLE"})");
		}
	}

	/// <summary>
	/// Whether <paramref name="enemy"/> is casting an area attack that reaches the party - the player or two living
	/// members, by the shape of the action - and that is not known to go on under a sleep.
	/// </summary>
	/// <param name="enemy">The enemy.</param>
	/// <returns>True for such a cast.</returns>
	public static bool IsAreaCastToStop(IBattleChara enemy)
	{
		return enemy != null && enemy.IsCasting && enemy.CastActionId != 0 && !IsUnstoppable(enemy.CastActionId)
			&& enemy.CastAreaReachesParty();
	}

	/// <summary>
	/// Whether <paramref name="enemy"/>'s cast is worth stopping by any means: an area attack that reaches the party
	/// (<see cref="IsAreaCastToStop"/>), where each sleep goes on testing whether it stops it; any cast the game marks
	/// interruptible; or a cast on a single target that a sleep has stopped before. The stop is the healers' and casters'
	/// counterpart to the melee interrupt (his rule of 23.10.2026: "schlaf sollte doch als meleependant für
	/// unterbrechungen genutzt werden, also auch auf unterbrechbare casts, solange das zeitfenster reicht"), and since his
	/// rule of 27.10.2026 for every cast known to be stoppable: "sleep auf alle als unterbrechbare aktionen ausweiten,
	/// nicht nur auf aoes. wenn die zeit reicht und die aktion bekannt unterbrechbar ist, sleep versuchen".
	/// </summary>
	/// <remarks>
	/// The interruptible cast is taken as the game marks it, without the melee interrupt's limits (ObjectHelper.CanInterrupt:
	/// at least 2 s, no cone, no ring, no area under 8 y, and its setting "Interrupt enemy casts", which casters never see):
	/// the decision checks the time, and the shape does not make a stopped cast less stopped. Whether sleeps have failed on
	/// the action is not read here: a stun stops what a sleep may not.
	/// The interruptible and the known stoppable cast count only from an enemy in the fight, as for the melee interrupt
	/// (concept 03, "Unterbrechen, Betäuben, Provozieren: welcher Gegner", A612): an idle enemy aims its cast at nobody, and
	/// a stop would only pull it. An area attack that reaches the party counts from any enemy.
	/// </remarks>
	/// <param name="enemy">The enemy.</param>
	/// <returns>True for such a cast.</returns>
	public static bool IsCastWorthStopping(IBattleChara enemy)
	{
		return enemy != null && enemy.IsCasting && enemy.CastActionId != 0
			&& (enemy.CastAreaReachesParty()
				|| (enemy.IsEngaged() && (enemy.IsCastInterruptible || IsKnownStoppedOnOne(enemy.CastActionId))));
	}

	// A cast on a single target - a debuff on one member - that a sleep has stopped before: known to be stoppable. An
	// area cast counts only while it reaches the party; stopping one that reaches nobody costs a GCD for nothing.
	private static bool IsKnownStoppedOnOne(uint actionId)
	{
		var act = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(actionId) ?? default;
		if (act.RowId == 0 || act.EffectRange != 0 || (CastType)act.CastType is not (CastType.Targeted or CastType.Unknown))
		{
			return false;
		}

		lock (OtherConfiguration.SleepStoreGate)
		{
			return OtherConfiguration.SleepOutcomes.TryGetValue(actionId, out var reading) && reading.Stopped > 0;
		}
	}

	/// <summary>
	/// Whether a sleep is worth trying on <paramref name="enemy"/>'s cast: it is worth stopping
	/// (<see cref="IsCastWorthStopping"/>), and the stored outcomes do not show that a sleep cannot stop the action
	/// (<see cref="IsUnstoppable"/>).
	/// </summary>
	/// <param name="enemy">The enemy.</param>
	/// <returns>True for such a cast.</returns>
	public static bool IsCastToStop(IBattleChara enemy)
	{
		return IsCastWorthStopping(enemy) && !IsUnstoppable(enemy.CastActionId);
	}

	/// <summary>
	/// Whether a sleep cannot stop <paramref name="actionId"/>: it went off although a sleep had landed at least as
	/// long before its end as the longest a cut-off cast took to stop, and no cast of it was ever stopped.
	/// </summary>
	/// <remarks>
	/// The conclusion over every game version. The rule reads it as a lock through <see cref="LocksOn"/>, which holds it
	/// only in the version that saw it and gives one taken over a try (concept 13, "Stufe 4", 4b).
	/// </remarks>
	/// <param name="actionId">The enemy's action.</param>
	/// <returns>True when the stored outcomes say so.</returns>
	public static bool IsUnstoppable(uint actionId)
	{
		Derive();
		lock (OtherConfiguration.SleepStoreGate)
		{
			return _holdTime >= 0f && OtherConfiguration.SleepOutcomes.TryGetValue(actionId, out var reading)
				&& reading.Stopped == 0 && reading.WentOff > 0 && reading.LatestWentOff >= _holdTime;
		}
	}

	/// <summary>
	/// The rule chose the job's own stopper for this enemy's cast. Written to the trace once per cast.
	/// </summary>
	/// <param name="enemy">The enemy.</param>
	/// <param name="action">The action chosen.</param>
	/// <param name="remaining">Seconds left on its cast.</param>
	/// <param name="needed">Seconds the action needs to land, as the rule reckoned them.</param>
	internal static void JobStop(IBattleChara enemy, IBaseAction action, float remaining, float needed)
	{
		var cast = (enemy.GameObjectId, enemy.CastActionId);
		if (cast == _jobStopTraced)
		{
			return;
		}

		_jobStopTraced = cast;
		var name = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(enemy.CastActionId)?.Name.ExtractText() ?? "?";
		DefenseTrace.Line($"stop a cast -> {action.Name} #{action.ID} on {enemy.Name.TextValue} (kind {enemy.BaseId}), casting {name} #{enemy.CastActionId}"
			+ $", {remaining:F1} s left, interruptible {enemy.IsCastInterruptible}, needs {needed:F1} s");
	}

	/// <summary>
	/// The rule chose a sleep for this enemy's cast. Written to the trace once per cast; the effect of the sleep is
	/// matched against it.
	/// </summary>
	/// <param name="enemy">The enemy.</param>
	/// <param name="sleep">The sleep chosen.</param>
	/// <param name="remaining">Seconds left on its cast.</param>
	/// <param name="needed">Seconds the sleep needs to land, as the rule reckoned them.</param>
	/// <param name="tries">The locks taken over from an earlier game version this sleep tries; empty for none.</param>
	internal static void AimAt(IBattleChara enemy, IBaseAction sleep, float remaining, float needed, IReadOnlyList<SleepLockKey> tries)
	{
		var actionId = enemy.CastActionId;
		if (_aim is { } aim && aim.EnemyId == enemy.GameObjectId && aim.ActionId == actionId)
		{
			_lapse = null;
			_lastLapse = null;
			if (!aim.Cast)
			{
				_aim = aim with { Tries = [.. tries] };
			}

			return;
		}

		// A sleep already cast for the current aim keeps it until its effect arrives, which is matched against it.
		if (_aim is { Cast: true } && DateTime.Now <= _castEnd.AddSeconds(DataCenter.DefaultGCDTotal))
		{
			return;
		}

		if (_aim is { Cast: false })
		{
			_lapse ??= $"a cast ending sooner was aimed at instead ({enemy.Name.TextValue})";
		}

		TraceNotCast(_aim);
		_lapse = null;
		_lastLapse = null;
		var name = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(actionId)?.Name.ExtractText() ?? "?";
		_aim = new Aim(enemy.GameObjectId, enemy.Name.TextValue, enemy.BaseId, actionId, name, DateTime.Now.AddSeconds(remaining),
			enemy.IsCastInterruptible) { Tries = [.. tries] };
		DefenseTrace.Line($"sleep to stop a cast -> {sleep.Name} #{sleep.ID} on {enemy.Name.TextValue} (kind {enemy.BaseId}), casting {name} #{actionId}"
			+ $", {remaining:F1} s left, interruptible {enemy.IsCastInterruptible}, needs {needed:F1} s"
			+ (tries.Count > 0 ? $"; tries locks from an earlier game version: {Describe(tries)}" : string.Empty));
	}

	/// <summary>The player pressed a sleep: the start of the time to its effect. Called from <c>BaseAction.Use</c>.</summary>
	/// <param name="actionId">The action pressed.</param>
	/// <param name="castTime">Its cast time at the press, zero when instant.</param>
	internal static void RecordPress(uint actionId, float castTime)
	{
		if (!IsSleepAction(actionId))
		{
			return;
		}

		_pressedAction = actionId;
		_pressedAt = DateTime.Now;
		_pressedCastTime = castTime;

		// The GCD is spent now: the one try in this game version on each lock taken over that the aim tries (concept 13,
		// "Stufe 4", 4b), whatever answer comes.
		if (_aim is { Tries.Count: > 0 } aim)
		{
			var game = OtherConfiguration.GameVersion;
			bool marked;
			lock (OtherConfiguration.SleepStoreGate)
			{
				marked = SleepLockRules.Try(OtherConfiguration.SleepLocks, aim.Tries, game);
			}

			_aim = aim with { Tries = [] };
			if (marked)
			{
				_ = OtherConfiguration.SaveSleepLocks();
				DefenseTrace.Line($"sleep pressed for {aim.EnemyName}: the one try in game version {game} on {Describe(aim.Tries)}");
			}
		}
	}

	/// <summary>
	/// The effect of the player's Sleep or Repose arrived: measures its lag, learns a resistance from every target that
	/// refused it - whether the rule chose the sleep or the player pressed it himself - and for the enemy it was aimed at
	/// starts watching the cast. Called by the effect handler on the game thread.
	/// </summary>
	/// <param name="actionId">The sleep action.</param>
	/// <param name="targets">Its targets.</param>
	internal static void ObserveOwnSleep(uint actionId, IReadOnlyList<Observation> targets)
	{
		// From the end of the cast the client showed, within one GCD of it, as everywhere a cast's effect is timed. A
		// sleep without a cast bar (Swiftcast, Dualcast) is timed from its press. The press alone gave no reading in
		// four days of his traces (A334): the cast is what can be seen to have happened.
		var now = DateTime.Now;
		DateTime? castEnded = _castAction == actionId && Math.Abs((now - _castEnd).TotalSeconds) <= DataCenter.DefaultGCDTotal
			? _castEnd
			: _pressedAction == actionId && (now - _pressedAt).TotalSeconds <= _pressedCastTime + DataCenter.DefaultGCDTotal
				? _pressedAt.AddSeconds(_pressedCastTime)
				: null;
		if (castEnded is { } end)
		{
			Lag = Math.Max(0f, (float)(now - end).TotalSeconds);
			_castAction = 0;
			_pressedAction = 0;
		}

		var aim = _aim;
		_aim = null;
		Observation? found = null;
		for (var i = 0; i < targets.Count; i++)
		{
			if (aim != null && targets[i].Target == aim.EnemyId)
			{
				found = targets[i];
				continue;
			}

			// What the game answered for any other target is knowledge of its kind too, whether the rule chose the sleep
			// or he pressed it himself: his own sleep on a Mimic came back fully resisted on 26.10.2026 and was lost,
			// because only the rule's sleeps were read, and a second one followed 23 minutes later (A427).
			if (!targets[i].Applied && (targets[i].StatusResisted || targets[i].Unaffected)
				&& ECommons.DalamudServices.Svc.Objects.SearchById(targets[i].Target) is IBattleChara other)
			{
				LearnRefusal(other.GameObjectId, other.Name.TextValue, other.BaseId, targets[i]);
			}
		}

		if (aim == null)
		{
			return;
		}

		if (found is not { } hit || (!hit.Applied && !hit.StatusResisted && !hit.Unaffected))
		{
			DefenseTrace.Line($"sleep reached no effect on {aim.EnemyName} (casting {aim.ActionName} #{aim.ActionId})");
			return;
		}

		if (!hit.Applied)
		{
			LearnRefusal(aim.EnemyId, aim.EnemyName, aim.BaseId, hit);
			return;
		}

		var enemy = ECommons.DalamudServices.Svc.Objects.SearchById(aim.EnemyId) as IBattleChara;
		if (enemy == null || !enemy.IsCasting || enemy.CastActionId != aim.ActionId)
		{
			DefenseTrace.Line($"sleep landed on {aim.EnemyName} after {aim.ActionName} #{aim.ActionId} had ended - gone off, interrupted by someone else or the enemy fell"
				+ $" (lag {Lag:F2} s)");
			return;
		}

		var left = enemy.TotalCastTime - enemy.CurrentCastTime;
		_watch = new Watch { Aim = aim, AppliedAt = now, ExpectedEnd = now.AddSeconds(left), LeftAtLanding = left };
		DefenseTrace.Line($"sleep landed on {aim.EnemyName} while it cast {aim.ActionName} #{aim.ActionId}, {left:F1} s left (lag {Lag:F2} s)");
	}

	// A sleep that did not take on an enemy: resisted after earlier sleeps in this fight is the halving running out, which
	// says how many it takes, not that the kind cannot be slept; resisted at the first is an immunity of the kind, stored;
	// invulnerable says nothing about the kind outside that moment and holds for the session.
	private static void LearnRefusal(ulong enemyId, string enemyName, uint baseId, Observation hit)
	{
		var slept = _sleptThisFight.GetValueOrDefault(enemyId);
		if (hit.StatusResisted && slept > 0)
		{
			_diminishedThisFight[enemyId] = 0;
			lock (OtherConfiguration.SleepStoreGate)
			{
				OtherConfiguration.SleepDiminishing[baseId] = OtherConfiguration.SleepDiminishing.TryGetValue(baseId, out var known)
					? Math.Min(known, slept)
					: slept;
			}

			_ = OtherConfiguration.SaveSleepDiminishing();
			ConfirmLock(lockMarks => SleepLockRules.ConfirmLimit(lockMarks, baseId, slept, OtherConfiguration.GameVersion));
			DefenseTrace.Line($"sleep resisted by {enemyName} (kind {baseId}) after {slept} sleep(s) in this fight: worn down, "
				+ $"not tried on it again this fight; the limit is stored and holds in game version {OtherConfiguration.GameVersion ?? "NOT READABLE"}");
			return;
		}

		if (hit.StatusResisted)
		{
			lock (OtherConfiguration.SleepStoreGate)
			{
				OtherConfiguration.SleepResistantKinds[baseId] = OtherConfiguration.SleepResistantKinds.GetValueOrDefault(baseId) + 1;
			}

			_ = OtherConfiguration.SaveSleepResistantKinds();
			ConfirmLock(lockMarks => SleepLockRules.ConfirmKind(OtherConfiguration.SleepResistantKinds, lockMarks, baseId, OtherConfiguration.GameVersion));
			DefenseTrace.Line($"sleep resisted by {enemyName} (kind {baseId}): immune, its kind is stored and not tried again in game version "
				+ (OtherConfiguration.GameVersion ?? "NOT READABLE"));
			return;
		}

		_unaffectedThisSession[baseId] = 0;
		DefenseTrace.Line($"sleep did not reach {enemyName} (kind {baseId}): resisted or invulnerable at that moment, its kind is not tried again this session");
	}

	// Records an observation that confirms a lock in the running game version, and saves the marks.
	private static void ConfirmLock(Action<SleepLockMarks> confirm)
	{
		lock (OtherConfiguration.SleepStoreGate)
		{
			confirm(OtherConfiguration.SleepLocks);
		}

		_ = OtherConfiguration.SaveSleepLocks();
	}

	/// <summary>
	/// An enemy's action resolved: if it is the watched cast, the sleep did not stop it. Called by the effect
	/// handler for every set from the enemy side, on the game thread.
	/// </summary>
	/// <param name="sourceId">The enemy.</param>
	/// <param name="actionId">Its action.</param>
	internal static void ObserveEnemyAction(ulong sourceId, uint actionId)
	{
		if (_watch is not { } watch || watch.Aim.EnemyId != sourceId || watch.Aim.ActionId != actionId)
		{
			return;
		}

		_watch = null;
		Record(actionId, reading =>
		{
			reading.WentOff++;
			reading.LatestWentOff = Math.Max(reading.LatestWentOff, watch.LeftAtLanding);
		});

		var line = $"sleep did not stop {watch.Aim.ActionName} #{actionId} of {watch.Aim.EnemyName}: it went off"
			+ $" {(DateTime.Now - watch.AppliedAt).TotalSeconds:F1} s after the sleep landed, {watch.LeftAtLanding:F1} s before its end"
			+ (watch.CastGone && watch.EndedEarlyBy > 0f ? $", its cast bar gone {watch.EndedEarlyBy:F1} s early" : string.Empty);

		// A sleep in time that did not stop it holds the lock in the running game version (concept 13, "Stufe 4", 4b); one
		// that came too late says nothing of the action.
		var unstoppable = IsUnstoppable(actionId);
		if (unstoppable && watch.LeftAtLanding >= _holdTime)
		{
			ConfirmLock(lockMarks => SleepLockRules.ConfirmAction(lockMarks, actionId, OtherConfiguration.GameVersion));
		}

		DefenseTrace.Line(unstoppable
			? $"{line}; cut-off casts stopped within {_holdTime:F1} s, so not tried on it again in game version {OtherConfiguration.GameVersion ?? "NOT READABLE"}"
			: $"{line}; too late - a sleep now has to land {LandingMargin:F1} s before the end");
	}

	/// <summary>Follows the player's own sleep cast and settles the watched cast. Called once per framework cycle.</summary>
	internal static void Update()
	{
		if (!DataCenter.InCombat && (!_sleptThisFight.IsEmpty || !_diminishedThisFight.IsEmpty))
		{
			_sleptThisFight.Clear();
			_diminishedThisFight.Clear();
		}

		if (ECommons.GameHelpers.Player.Object is { IsCasting: true } player && IsSleepAction(player.CastActionId))
		{
			_castAction = player.CastActionId;
			_castEnd = DateTime.Now.AddSeconds(Math.Max(0f, player.TotalCastTime - player.CurrentCastTime));
			if (_aim is { Cast: false } cast && player.CastTargetObjectId == cast.EnemyId)
			{
				_aim = cast with { Cast = true };
			}
		}

		if (_watch is not { } watch)
		{
			// An aim no sleep answered: past the cast's end and one GCD for an effect to arrive, or past the cast of
			// a sleep pressed after it.
			if (_aim is { } aim)
			{
				var until = aim.CastEnd;
				if (_pressedAction != 0 && _pressedAt.AddSeconds(_pressedCastTime) > until)
				{
					until = _pressedAt.AddSeconds(_pressedCastTime);
				}

				if (DateTime.Now > until.AddSeconds(DataCenter.DefaultGCDTotal))
				{
					TraceNotCast(aim);
					_aim = null;
					_lapse = null;
					_lastLapse = null;
				}
			}

			return;
		}

		var now = DateTime.Now;
		var enemy = ECommons.DalamudServices.Svc.Objects.SearchById(watch.Aim.EnemyId) as IBattleChara;
		if (enemy == null || enemy.IsDead || enemy.CurrentHp == 0)
		{
			_watch = null;
			DefenseTrace.Line($"sleep on {watch.Aim.EnemyName}: it fell before {watch.Aim.ActionName} #{watch.Aim.ActionId} could resolve - no answer");
			return;
		}

		if (!watch.CastGone)
		{
			if (enemy.IsCasting && enemy.CastActionId == watch.Aim.ActionId)
			{
				return;
			}

			watch.CastGone = true;
			watch.CastGoneAt = now;
			watch.EndedEarlyBy = (float)(watch.ExpectedEnd - now).TotalSeconds;
			watch.StunnedAtEnd = StunWatch.StunLeft(enemy) > 0f;
			return;
		}

		// The answer is the cast's effect, which arrives after the cast bar: ObserveEnemyAction settles a cast that went
		// off. Until A364 a bar that ended before its time by more than the round trip and a frame counted as stopped at
		// once, and the effect was no longer watched for - but a cast bar the client shows can end that early by itself:
		// one went off 0.5 s before its time, and three "stops" in his traces of 22.-25.10.2026 ended 0.1-0.3 s early,
		// 0.5-1.2 s after the sleep landed, as a cast that ran its course does.
		if ((now - watch.CastGoneAt).TotalSeconds <= DataCenter.DefaultGCDTotal)
		{
			return;
		}

		_watch = null;
		var early = Math.Max(0f, TankbusterForecast.RoundTrip) + ECommons.DalamudServices.Svc.Framework.UpdateDelta.TotalSeconds;
		if (watch.EndedEarlyBy <= early)
		{
			DefenseTrace.Line($"sleep on {watch.Aim.EnemyName}: {watch.Aim.ActionName} #{watch.Aim.ActionId} ended at its time with no effect seen - no answer");
			return;
		}

		if (watch.StunnedAtEnd)
		{
			DefenseTrace.Line($"{watch.Aim.ActionName} #{watch.Aim.ActionId} of {watch.Aim.EnemyName} stopped "
				+ $"{watch.EndedEarlyBy:F1} s early with no effect, but a stun landed on it too - not counted");
			return;
		}

		var took = (float)(watch.CastGoneAt - watch.AppliedAt).TotalSeconds;
		Record(watch.Aim.ActionId, reading =>
		{
			reading.Stopped++;
			reading.LongestStop = Math.Max(reading.LongestStop, took);
		});
		DefenseTrace.Line($"sleep stopped {watch.Aim.ActionName} #{watch.Aim.ActionId} of {watch.Aim.EnemyName}: "
			+ $"the cast ended {watch.EndedEarlyBy:F1} s early, {took:F1} s after the sleep landed, and no effect followed"
			+ (watch.Aim.Interruptible ? "; the cast was interruptible, so another player's interrupt may have done it" : string.Empty));
	}

	// Adds one outcome to the stored table and saves it, as the area table does after each rated hit.
	private static void Record(uint actionId, Action<SleepReading> change)
	{
		lock (OtherConfiguration.SleepStoreGate)
		{
			if (!OtherConfiguration.SleepOutcomes.TryGetValue(actionId, out var reading))
			{
				reading = new SleepReading();
				OtherConfiguration.SleepOutcomes[actionId] = reading;
			}

			change(reading);
			_version++;
		}

		_ = OtherConfiguration.SaveSleepOutcomes();
	}

	// The hold time and the landing margin from the stored outcomes; again whenever an outcome was added or the table
	// was loaded.
	private static void Derive()
	{
		lock (OtherConfiguration.SleepStoreGate)
		{
			var table = OtherConfiguration.SleepOutcomes;
			if (ReferenceEquals(table, _derivedFrom) && _derivedVersion == _version)
			{
				return;
			}

			_derivedFrom = table;
			_derivedVersion = _version;

			var hold = -1f;
			foreach (var reading in table.Values)
			{
				if (reading.Stopped > 0)
				{
					hold = Math.Max(hold, reading.LongestStop);
				}
			}

			var margin = 0f;
			foreach (var reading in table.Values)
			{
				if (reading.WentOff > 0 && (hold < 0f || reading.LatestWentOff < hold))
				{
					margin = Math.Max(margin, reading.LatestWentOff + (float)ECommons.DalamudServices.Svc.Framework.UpdateDelta.TotalSeconds);
				}
			}

			_holdTime = hold;
			_landingMargin = margin;
		}
	}

	/// <summary>
	/// The rule did not choose the sleep this time, for <paramref name="reason"/>: kept for the aim it had chosen it for,
	/// the first since the choice and the last, so a choice that lapses says why (concept 08, "Protokoll"). <paramref name="enemy"/> null for a reason that holds
	/// for every cast.
	/// </summary>
	internal static void NoteLapse(IBattleChara? enemy, string reason)
	{
		if (_aim is { Cast: false } aim && (enemy == null || (aim.EnemyId == enemy.GameObjectId && aim.ActionId == enemy.CastActionId)))
		{
			_lapse ??= reason;
			_lastLapse = reason;
		}
	}

	// A choice the GCD never carried out, with the reason the rule last gave for not choosing it again. Without a reason
	// the rule was not asked: a GCD ahead of it in the path was chosen. The choice holds only for the moment it is made -
	// moving or a member in danger takes the GCD after it, as intended; the reason tells those apart from a window that
	// closed (A457).
	private static void TraceNotCast(Aim? aim)
	{
		if (aim is { Cast: false })
		{
			DefenseTrace.Line($"sleep not cast on {aim.EnemyName}: chosen for {aim.ActionName} #{aim.ActionId}, but no sleep went out for it - "
				+ (_lapse ?? "the rule was not asked again: a GCD ahead of it in the path")
				+ (_lastLapse != null && _lastLapse != _lapse ? $"; last: {_lastLapse}" : string.Empty));
		}
	}
}
