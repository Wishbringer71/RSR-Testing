using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using RotationSolver.IPC;

namespace RotationSolver.Basic.Tweaks;

/// <summary>
/// How long the player's actions really hold the next one back, measured on every own answer, and what follows from it
/// without a set value: how much of the lock comes off, the weave window, the framerate cap, the other tools, and the
/// trace line per fight (concept 03, "Weben, Animationssperre und Latenz", A753).
/// </summary>
/// <remarks>
/// <para>The delay from a request to its answer is the way up, the wait for the server's tick, the way down and the frame
/// the answer is read in. The server's lock for the action ends its lock time after it processed the request, so the next
/// request may leave the moment that lock ends less its own way up: what may come off the client's lock is the network
/// round trip, not the tick wait. Its lower envelope in the zone - the least delay of the earlier answers, less their mean
/// deviation while the samples are few - is the safe part; the routes change with the zone, so the statistics do.</para>
/// <para>What the server does with a request that arrives before its lock ends is unknown (it may hold it, ignore the
/// lock or refuse it). A controller finds out: each answer adds r measured frames to the cut, each request without an
/// answer takes (1 - r) frames off, r being the frame time over the client's lock on a press. The share of refused
/// requests settles at r, where what a refused request costs - a new lock from the press - weighs what a frame of cut
/// gains. A late answer is no loss - the server held the request and still processed it - and does not count.</para>
/// </remarks>
public sealed unsafe partial class AnimationLockTweak
{
	// The zone the statistics belong to.
	private uint _zone = uint.MaxValue;

	// Delay from request to answer in the zone: count, mean, mean deviation, least.
	private int _delayCount;
	private float _delayMean, _delayDeviation, _delayLeast, _delayMost;

	// The controller: what it cuts beyond the measured envelope; whether a request since the last answer went without one.
	private float _extra;
	private bool _missing;

	// The zone's delays, for the rest the current cut leaves of each, and that rest's mean plus mean deviation.
	private readonly List<float> _delays = [];
	private float _keptByCut;

	// What actions kept after their lock, as measured after every tool: for the weave window while RSR does not cut.
	private int _keptCount;
	private float _keptMean, _keptDeviation;

	// The server's lock per action, and the last one seen.
	private readonly Dictionary<uint, float> _lockByAction = [];
	private float _lastLock = -1f;

	// Mean frame time in the zone: the controller's step and the framerate tweak's cap.
	private int _frames;
	private float _frameMean;

	private bool _heldInZone;
	private string? _conflictingTool;

	/// <summary>The mean frame time measured in this zone; 0 before the first frame.</summary>
	internal static float MeanFrameTime { get; private set; }

	/// <summary>Whether the answer <see cref="Apply"/> saw last belonged to the request recorded before it.</summary>
	internal bool LastMatched { get; private set; }

	/// <summary>
	/// Whether the lock may be shortened now: the setting is on, no other tool shortens it, and the packet check has not
	/// found one in this zone. PvP as PvE (concept 03, A755).
	/// </summary>
	internal bool ReductionAllowed => Service.Config.RemoveAnimationLockDelay && !_heldInZone
		&& ConflictingTool == null;

	/// <summary>
	/// A tool that shortens the lock itself and is not switched off by RSR: XivAlexander in the game's process, or NoClippy
	/// while RSR does not take it over. Null when none.
	/// </summary>
	internal string? ConflictingTool
	{
		get
		{
			ResetOnZoneChange();
			return _conflictingTool;
		}
	}

	private void ResetOnZoneChange()
	{
		var zone = Svc.ClientState.TerritoryType;
		if (zone == _zone)
		{
			return;
		}

		_zone = zone;
		_delayCount = _keptCount = _frames = 0;
		_delayMean = _delayDeviation = _delayLeast = _delayMost = _keptMean = _keptDeviation = _frameMean = 0f;
		_extra = _keptByCut = 0f;
		_delays.Clear();
		_castSequences.Clear();
		_missing = _heldInZone = false;

		TakeOverOtherTools();
		var found = XivAlexanderLoaded() ? "XivAlexander"
			: _noClippyTakenOver ? null
			: DataCenter.IsPluginLoaded("NoClippy", "NoClippy") ? "NoClippy" : null;
		if (found != null && found != _conflictingTool)
		{
			DefenseTrace.Line($"animation lock reduction held: {found} shortens the lock itself; RSR weaves on what it leaves");
		}

		_conflictingTool = found;
	}

	private static bool XivAlexanderLoaded()
	{
		try
		{
			using var process = System.Diagnostics.Process.GetCurrentProcess();
			foreach (System.Diagnostics.ProcessModule module in process.Modules)
			{
				if (module.ModuleName.Contains("XivAlexander", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}
		catch (Exception)
		{
			// The module list could not be read; the packet check still finds a tool that changes the lock.
		}

		return false;
	}

	// ---- Measuring and cutting.

	/// <summary>
	/// Learns the delay of an answer to the last request and returns what may come off its lock: the zone's measured
	/// envelope of the network round trip plus the controller's extra, both from the earlier answers.
	/// </summary>
	private float Learn(float delay)
	{
		ResetOnZoneChange();
		var cut = 0f;
		if (_delayCount >= 2 && _lastReqInitialAnimLock > 0f)
		{
			var step = MeanFrameTime;
			var share = step / _lastReqInitialAnimLock;
			var envelope = Math.Max(0f, _delayLeast - _delayDeviation);
			// No more than the longest delay of the zone can come off: a server that refuses nothing would let the extra grow
			// without end, and a later refusal would need as many steps down (A753).
			_extra = _missing ? Math.Max(0f, _extra - step * (1 - share))
				: Math.Min(_extra + step * share, Math.Max(0f, _delayMost - envelope));
			cut = envelope + _extra;
		}

		_missing = false;
		var n = _delayCount;
		_delayDeviation = n == 0 ? 0f : (_delayDeviation * n + Math.Abs(delay - _delayMean)) / (n + 1);
		_delayMean = (_delayMean * n + delay) / (n + 1);
		_delayLeast = n == 0 ? delay : Math.Min(_delayLeast, delay);
		_delayMost = Math.Max(_delayMost, delay);
		_delayCount = n + 1;
		_delays.Add(delay);
		_lastCut = cut;
		_keptByCut = RestLeftBy(cut);
		return cut;
	}

	private float _lastCut;

	// What the cut leaves of each delay the zone saw, mean plus mean deviation: once per answer, the list one entry per
	// answer in the zone.
	private float RestLeftBy(float cut)
	{
		var count = _delays.Count;
		if (count == 0)
		{
			return 0f;
		}

		var sum = 0f;
		for (var i = 0; i < count; i++)
		{
			sum += Math.Max(0f, _delays[i] - cut);
		}

		var mean = sum / count;
		var deviation = 0f;
		for (var i = 0; i < count; i++)
		{
			deviation += Math.Abs(Math.Max(0f, _delays[i] - cut) - mean);
		}

		return mean + deviation / count;
	}

	/// <summary>
	/// The rest an action is expected to keep after its lock: with RSR cutting, what the current cut leaves of the zone's
	/// delays, mean plus mean deviation; else what the zone's answers kept after every tool. 0 before the first
	/// measurement.
	/// </summary>
	internal float KeptEstimate => ReductionAllowed && _delayCount >= 2
		? _keptByCut
		: _keptCount > 0 ? _keptMean + _keptDeviation : 0f;

	/// <summary>
	/// An answer to the player's own instant action: the delay from the request, the lock the packet gave, and the game's
	/// lock right after every tool handled it.
	/// </summary>
	internal void Observe(uint actionId, float packetLock, float delay, float lockAfter)
	{
		if (packetLock <= 0f || delay < 0f)
		{
			return;
		}

		_lockByAction[actionId] = packetLock;
		_lastLock = packetLock;
		var kept = Math.Max(0f, delay + lockAfter - packetLock);
		var n = _keptCount;
		_keptDeviation = n == 0 ? 0f : (_keptDeviation * n + Math.Abs(kept - _keptMean)) / (n + 1);
		_keptMean = (_keptMean * n + kept) / (n + 1);
		_keptCount = n + 1;

		if (DataCenter.InCombat)
		{
			_fight.Answers++;
			_fight.Delay += delay;
			_fight.Kept += kept;
			_fight.MaxKept = Math.Max(_fight.MaxKept, kept);
		}
	}

	/// <summary>
	/// Seconds an ability blocks before the GCD: its lock as the server last gave it (else the last lock seen) plus the rest
	/// expected after it. Null before the first measurement.
	/// </summary>
	internal float? AbilityWindow(uint actionId)
	{
		if (_lastLock < 0f || _delayCount == 0)
		{
			return null;
		}

		var lockTime = actionId != 0 && _lockByAction.TryGetValue(actionId, out var known) ? known : _lastLock;
		return lockTime + KeptEstimate;
	}

	/// <summary>A reduction RSR applied, for the fight's line.</summary>
	internal void NoteReduction(float reduction)
	{
		if (DataCenter.InCombat)
		{
			_fight.Reduced++;
			_fight.ReducedTotal += reduction;
		}
	}

	// ---- Taking over BossModReborn's and NoClippy's lock functions.

	private readonly List<(string Field, string Original)> _bossModTakenOver = [];
	private bool _noClippyTakenOver;

	private static bool TakeOverWanted => Service.Config.TakeOverOtherLockTools && Service.Config.RemoveAnimationLockDelay;

	/// <summary>
	/// Switches BossModReborn's two lock tweaks off for the session without saving (its transient setting) and NoClippy's
	/// compensation off by its command, while RSR's own are on; gives them back when not.
	/// </summary>
	internal void TakeOverOtherTools()
	{
		if (!TakeOverWanted)
		{
			GiveBackOtherTools();
			return;
		}

		try
		{
			// Read as it is now, every time: a BossMod loaded again carries its saved value again (A754).
			if (BMRInfo_IPCSubscriber.IsEnabled && BMRInfo_IPCSubscriber.Configuration != null && BMRInfo_IPCSubscriber.SetTransient != null)
			{
				foreach (var field in (ReadOnlySpan<string>)["RemoveAnimationLockDelay", "RemoveCooldownDelay"])
				{
					var current = BMRInfo_IPCSubscriber.Configuration(["ActionTweaksConfig", field], false);
					if (current is [var value] && bool.TryParse(value, out var on) && on)
					{
						_ = BMRInfo_IPCSubscriber.SetTransient(["ActionTweaksConfig", field, "false"]);
						if (!HasTakenOver(field))
						{
							_bossModTakenOver.Add((field, value));
						}

						DefenseTrace.Line($"took over BossModReborn's {field}: off for the session, its setting kept");
					}
				}
			}

			// Only what NoClippy has on: "/noclippy on" afterwards must not switch on what the player had off. Read again
			// every time, so a compensation switched on by hand is taken over again.
			if (DataCenter.IsPluginLoaded("NoClippy", "NoClippy") && NoClippyCompensationOn() && Svc.Commands.ProcessCommand("/noclippy off"))
			{
				_noClippyTakenOver = true;
				DefenseTrace.Line("took over NoClippy's animation lock compensation: off until RSR's is off or the plugin unloads");
			}
		}
		catch (Exception ex)
		{
			DefenseTrace.Line($"taking over the other lock tools failed: {ex.Message}");
		}
	}

	private bool HasTakenOver(string field)
	{
		foreach (var (taken, _) in _bossModTakenOver)
		{
			if (taken == field)
			{
				return true;
			}
		}

		return false;
	}

	// NoClippy's saved switch for its compensation (EnableAnimLockComp in its configuration file beside RSR's); on when the
	// file has no such entry, its default.
	private static bool NoClippyCompensationOn()
	{
		try
		{
			var folder = Svc.PluginInterface.ConfigFile.Directory;
			var file = folder == null ? null : new System.IO.FileInfo(System.IO.Path.Combine(folder.FullName, "NoClippy.json"));
			if (file is not { Exists: true })
			{
				return false;
			}

			using var document = System.Text.Json.JsonDocument.Parse(System.IO.File.ReadAllText(file.FullName));
			return !document.RootElement.TryGetProperty("EnableAnimLockComp", out var value)
				|| value.ValueKind != System.Text.Json.JsonValueKind.False;
		}
		catch (Exception)
		{
			// Unreadable: left alone; the packet check still holds RSR's cut if NoClippy changes the lock.
			return false;
		}
	}

	/// <summary>Gives the other tools their settings back, as far as RSR took them.</summary>
	internal void GiveBackOtherTools()
	{
		try
		{
			if (_bossModTakenOver.Count > 0 && BMRInfo_IPCSubscriber.Configuration != null)
			{
				foreach (var (field, original) in _bossModTakenOver)
				{
					// An explicit set, unsaved, replaces the transient override of this one field only.
					_ = BMRInfo_IPCSubscriber.Configuration(["ActionTweaksConfig", field, original], false);
				}
			}

			if (_noClippyTakenOver)
			{
				_ = Svc.Commands.ProcessCommand("/noclippy on");
			}
		}
		catch (Exception ex)
		{
			DefenseTrace.Line($"giving the other lock tools back failed: {ex.Message}");
		}
		finally
		{
			_bossModTakenOver.Clear();
			_noClippyTakenOver = false;
		}
	}

	// ---- The fight's line: GCDs, weaves, GCDs a lock held back (NoClippy's measure), GCD time with nothing queued, and
	// requests the server did not answer.

	private struct FightTiming
	{
		public int Answers, Reduced, Gcds, Abilities, Clips, Unanswered;
		public float Delay, Kept, MaxKept, ReducedTotal, ClipTotal, Idle;
	}

	private FightTiming _fight;
	private bool _wasInCombat;
	private uint _clipCounted;
	private bool _lastRequestAbility;
	private ushort _lastAnswered;
	private bool _tookOverFor;

	/// <summary>A request of the player left: a GCD or an ability without cast (a cast is neither).</summary>
	internal void NoteRequest(ActionManager* manager, ActionType actionType, uint actionId)
	{
		var isCast = manager->CastSpellId != 0;
		var isGcd = manager->GetRecastGroup((int)actionType, actionId) == ActionHelper.GCDCooldownGroup - 1;
		_lastRequestAbility = !isCast && !isGcd;
		if (isCast)
		{
			// A cast that movement breaks gets no answer with an effect either; it says nothing about the lock (A754).
			_castSequences.Add((ushort)manager->LastUsedActionSequence);
		}
		else
		{
			// A765: what went, and how long after the last answer, for the line of a request without an answer.
			_instantRequests.Add(((ushort)manager->LastUsedActionSequence, actionId, isGcd,
				(float)(DateTime.Now - _lastAnswerAt).TotalSeconds));
		}
		if (!DataCenter.InCombat || isCast && !isGcd)
		{
			return;
		}

		if (isGcd)
		{
			_fight.Gcds++;
		}
		else
		{
			_fight.Abilities++;
		}
	}

	/// <summary>An answer of the server to the player's request <paramref name="sequence"/>; a gap is a request without one.</summary>
	internal void NoteAnswer(ushort sequence)
	{
		if (_lastAnswered != 0 && sequence > _lastAnswered + 1)
		{
			// Only an instant without an answer counts; a cast's sequence in the gap was broken, not refused (A754).
			var unanswered = 0;
			for (var skipped = _lastAnswered + 1; skipped < sequence; skipped++)
			{
				if (!_castSequences.Remove((ushort)skipped))
				{
					unanswered++;
					TraceUnanswered((ushort)skipped);
				}
			}

			if (unanswered > 0)
			{
				_missing = true;
				if (DataCenter.InCombat)
				{
					_fight.Unanswered += unanswered;
				}
			}
		}

		// The answered cast and any older ones are done with.
		for (var i = _castSequences.Count - 1; i >= 0; i--)
		{
			if (_castSequences[i] <= sequence)
			{
				_castSequences.RemoveAt(i);
			}
		}

		for (var i = _instantRequests.Count - 1; i >= 0; i--)
		{
			if (_instantRequests[i].Sequence <= sequence)
			{
				_instantRequests.RemoveAt(i);
			}
		}

		_lastAnswered = sequence;
		_lastAnswerAt = DateTime.Now;
	}

	private readonly List<ushort> _castSequences = [];
	private readonly List<(ushort Sequence, uint Action, bool Gcd, float SinceAnswer)> _instantRequests = [];
	private DateTime _lastAnswerAt = DateTime.MinValue;

	// A765 (concept 03, "Weben, Animationssperre und Latenz"): an instant request without an answer, with what it was and
	// how long after the previous answer it left, against that answer's lock - whether the server refused it as early.
	private void TraceUnanswered(ushort sequence)
	{
		if (!DataCenter.InCombat)
		{
			return;
		}

		foreach (var request in _instantRequests)
		{
			if (request.Sequence == sequence)
			{
				DefenseTrace.Line($"request without an answer: {(request.Gcd ? "GCD" : "ability")} #{request.Action}, "
					+ $"{request.SinceAnswer * 1000:F0} ms after the previous answer (its lock {_lastLock * 1000:F0} ms), cut {_lastCut * 1000:F0} ms");
				return;
			}
		}

		DefenseTrace.Line($"request without an answer: sequence {sequence} not seen as a request");
	}

	/// <summary>Once per frame from the action manager's update: frame time, clipping, GCD time with nothing queued, the fight's end.</summary>
	internal void Frame(ActionManager* manager, float delta)
	{
		ResetOnZoneChange();
		if (TakeOverWanted != _tookOverFor)
		{
			// The setting changed: take the other tools over, or give them back, now rather than at the next zone.
			_tookOverFor = TakeOverWanted;
			TakeOverOtherTools();
		}

		_frameMean = (_frameMean * _frames + delta) / (_frames + 1);
		_frames++;
		MeanFrameTime = _frameMean;

		var inCombat = DataCenter.InCombat;
		if (inCombat)
		{
			var gcdFree = DataCenter.DefaultGCDRemain <= 0f;
			var sequence = manager->LastUsedActionSequence;
			if (gcdFree && manager->AnimationLock > 0f && _lastRequestAbility && sequence != _clipCounted)
			{
				// The GCD came free while an ability's lock still stood: the rest of that lock is how late the GCD comes.
				_clipCounted = sequence;
				_fight.Clips++;
				_fight.ClipTotal += manager->AnimationLock;
			}
			else if (gcdFree && manager->AnimationLock <= 0f && manager->CastSpellId == 0 && !manager->ActionQueued && DataCenter.State)
			{
				_fight.Idle += delta;
			}
		}
		else if (_wasInCombat)
		{
			TraceFight();
		}

		_wasInCombat = inCombat;
	}

	private void TraceFight()
	{
		var f = _fight;
		_fight = default;
		if (f.Gcds == 0 && f.Answers == 0)
		{
			return;
		}

		var window = AbilityWindow(0);
		var state = ConflictingTool is { } tool ? $"held, {tool} shortens it"
			: !Service.Config.RemoveAnimationLockDelay ? "off" : _heldInZone ? "held in this zone" : "on";
		DefenseTrace.Line($"action timing over the fight: {f.Answers} answers, delay to the answer mean"
			+ $" {(f.Answers > 0 ? f.Delay / f.Answers * 1000 : 0):F0} ms (zone: least {_delayLeast * 1000:F0}, deviation"
			+ $" {_delayDeviation * 1000:F0} ms), rest kept after the lock mean {(f.Answers > 0 ? f.Kept / f.Answers * 1000 : 0):F0} ms"
			+ $" (max {f.MaxKept * 1000:F0} ms); lock reduction {state}, cut {_lastCut * 1000:F0} ms (controller {_extra * 1000:F0} ms),"
			+ $" {f.Reduced} shortened by"
			+ $" {f.ReducedTotal * 1000:F0} ms in all; weave window {(window is { } w ? $"{w:F3} s" : "not measured")}; frame"
			+ $" {MeanFrameTime * 1000:F1} ms; {f.Gcds} GCDs, {f.Abilities} abilities ({(f.Gcds > 0 ? (float)f.Abilities / f.Gcds : 0):F2}"
			+ $" per GCD); GCD held back by a lock {f.Clips} times, {f.ClipTotal:F2} s in all; GCD free with nothing queued"
			+ $" {f.Idle:F2} s; {f.Unanswered} request(s) without an answer");
	}
}
