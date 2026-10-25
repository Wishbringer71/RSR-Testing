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
	/// Whether a sleep could still land on <paramref name="enemy"/>: not a boss (BNpcBase rank), no Sleep Resistance
	/// (row 1354, "Immune to sleep effects"), not of a kind that resisted one (stored), not worn down by repeated sleeps
	/// in this fight, and no sleep has just landed on it.
	/// </summary>
	/// <remarks>
	/// Repeated sleeps on one enemy halve until it resists for a while, counted apart from stuns (community sources:
	/// Square Enix forum, 2013 and 2016, "30s, 15s, 8s, then immune"; no game text, and the limit is not set here but
	/// learned: <see cref="OtherConfiguration.SleepDiminishing"/>). The game shows neither the count nor the immunity as
	/// a status, so the sleeps that landed in this fight are counted here, as StunWatch counts stuns.
	/// </remarks>
	/// <param name="enemy">The enemy.</param>
	/// <returns>True while a sleep can still land.</returns>
	public static bool CanStillBeSlept(IBattleChara enemy)
	{
		if (enemy == null || enemy.IsBossFromIcon() || enemy.HasStatus(false, StatusID.SleepResistance)
			|| _unaffectedThisSession.ContainsKey(enemy.BaseId)
			|| _diminishedThisFight.ContainsKey(enemy.GameObjectId)
			// An enemy a sleep has just landed on is asleep before its status shows; a second sleep on the same cast
			// is a GCD for nothing (trace of 21.10.2026, A334).
			|| _watch?.Aim.EnemyId == enemy.GameObjectId)
		{
			return false;
		}

		var slept = _sleptThisFight.GetValueOrDefault(enemy.GameObjectId);
		lock (OtherConfiguration.SleepStoreGate)
		{
			if (OtherConfiguration.SleepResistantKinds.ContainsKey(enemy.BaseId))
			{
				return false;
			}

			// The limit learned for this kind, otherwise the lowest learned for any: the halving is the game's, not
			// the kind's.
			if (slept == 0)
			{
				return true;
			}

			if (OtherConfiguration.SleepDiminishing.TryGetValue(enemy.BaseId, out var limit))
			{
				return slept < limit;
			}

			var lowest = int.MaxValue;
			foreach (var learned in OtherConfiguration.SleepDiminishing.Values)
			{
				lowest = Math.Min(lowest, learned);
			}

			return slept < lowest;
		}
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
			if (targets[i].Applied)
			{
				_sleptThisFight.AddOrUpdate(targets[i].Target, 1, (_, count) => count + 1);
			}
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
			&& ReachesParty(enemy);
	}

	/// <summary>
	/// Whether <paramref name="enemy"/>'s cast is worth stopping by any means: an area attack that reaches the party
	/// (<see cref="IsAreaCastToStop"/>), or a cast the melee roles would stop with their interrupt - one the game marks
	/// interruptible, by the same recognition (ObjectHelper.CanInterrupt). The stop is the healers' and casters'
	/// counterpart to it (his rule of 23.10.2026: "schlaf sollte doch als meleependant für unterbrechungen genutzt
	/// werden, also auch auf unterbrechbare casts, solange das zeitfenster reicht"). Whatever sleeps have shown about the
	/// action is not read here: a stun stops what a sleep may not.
	/// </summary>
	/// <param name="enemy">The enemy.</param>
	/// <returns>True for such a cast.</returns>
	public static bool IsCastWorthStopping(IBattleChara enemy)
	{
		return enemy != null && enemy.IsCasting && enemy.CastActionId != 0 && (ReachesParty(enemy) || enemy.CanInterrupt());
	}

	// An area attack that reaches the player or two living members, by the shape of the action.
	private static bool ReachesParty(IBattleChara enemy)
	{
		var act = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRow(enemy.CastActionId);
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
		var name = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRow(enemy.CastActionId).Name.ExtractText();
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
	internal static void AimAt(IBattleChara enemy, IBaseAction sleep, float remaining, float needed)
	{
		var actionId = enemy.CastActionId;
		if (_aim is { } aim && aim.EnemyId == enemy.GameObjectId && aim.ActionId == actionId)
		{
			return;
		}

		// A sleep already cast for the current aim keeps it until its effect arrives, which is matched against it.
		if (_aim is { Cast: true } && DateTime.Now <= _castEnd.AddSeconds(DataCenter.DefaultGCDTotal))
		{
			return;
		}

		TraceNotCast(_aim);
		var name = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRow(actionId).Name.ExtractText();
		_aim = new Aim(enemy.GameObjectId, enemy.Name.TextValue, enemy.BaseId, actionId, name, DateTime.Now.AddSeconds(remaining),
			enemy.IsCastInterruptible);
		DefenseTrace.Line($"sleep to stop a cast -> {sleep.Name} #{sleep.ID} on {enemy.Name.TextValue} (kind {enemy.BaseId}), casting {name} #{actionId}"
			+ $", {remaining:F1} s left, interruptible {enemy.IsCastInterruptible}, needs {needed:F1} s");
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
	}

	/// <summary>
	/// The effect of the player's Sleep or Repose arrived: measures its lag, and for the enemy it was aimed at
	/// learns a resistance or starts watching the cast. Called by the effect handler on the game thread.
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

		if (_aim is not { } aim)
		{
			return;
		}

		_aim = null;
		Observation? found = null;
		for (var i = 0; i < targets.Count; i++)
		{
			if (targets[i].Target == aim.EnemyId)
			{
				found = targets[i];
				break;
			}
		}

		if (found is not { } hit || (!hit.Applied && !hit.StatusResisted && !hit.Unaffected))
		{
			DefenseTrace.Line($"sleep reached no effect on {aim.EnemyName} (casting {aim.ActionName} #{aim.ActionId})");
			return;
		}

		if (!hit.Applied)
		{
			// Resisted after earlier sleeps in this fight: the halving has run out, which says how many it takes, not
			// that the kind cannot be slept.
			var slept = _sleptThisFight.GetValueOrDefault(aim.EnemyId);
			if (hit.StatusResisted && slept > 0)
			{
				_diminishedThisFight[aim.EnemyId] = 0;
				lock (OtherConfiguration.SleepStoreGate)
				{
					OtherConfiguration.SleepDiminishing[aim.BaseId] = OtherConfiguration.SleepDiminishing.TryGetValue(aim.BaseId, out var known)
						? Math.Min(known, slept)
						: slept;
				}

				_ = OtherConfiguration.SaveSleepDiminishing();
				DefenseTrace.Line($"sleep resisted by {aim.EnemyName} (kind {aim.BaseId}) after {slept} sleep(s) in this fight: worn down, "
					+ "not tried on it again this fight; the limit is stored");
				return;
			}

			if (hit.StatusResisted)
			{
				lock (OtherConfiguration.SleepStoreGate)
				{
					OtherConfiguration.SleepResistantKinds[aim.BaseId] =
						OtherConfiguration.SleepResistantKinds.GetValueOrDefault(aim.BaseId) + 1;
				}

				_ = OtherConfiguration.SaveSleepResistantKinds();
				DefenseTrace.Line($"sleep resisted by {aim.EnemyName} (kind {aim.BaseId}): immune, its kind is stored and not tried again");
				return;
			}

			_unaffectedThisSession[aim.BaseId] = 0;
			DefenseTrace.Line($"sleep did not reach {aim.EnemyName} (kind {aim.BaseId}): resisted or invulnerable at that moment, its kind is not tried again this session");
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
		DefenseTrace.Line(IsUnstoppable(actionId)
			? $"{line}; cut-off casts stopped within {_holdTime:F1} s, so not tried on it again"
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

	// A choice the GCD never carried out - another action came first, or the enemy's cast ran too short for the sleep
	// by the time the GCD rolled. Without this line such a choice and a sleep that went out unanswered looked alike.
	private static void TraceNotCast(Aim? aim)
	{
		if (aim is { Cast: false })
		{
			DefenseTrace.Line($"sleep not cast on {aim.EnemyName}: chosen for {aim.ActionName} #{aim.ActionId}, but no sleep went out for it");
		}
	}
}
