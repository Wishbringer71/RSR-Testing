using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace RotationSolver.Engine.Helpers;

/// <summary>
/// Concept 03, "Zauberbeginn vor dem eigenen Gehen: gelernter Vorlauf und Pause" (A880): when the player walks, measured on
/// him, so that RSR begins no spell his walking will break - how many seconds before a hit that reaches him he steps off,
/// and how long he stands, after a walk broke a spell, before he walks again. Learned per session; without a measurement
/// the rule is the one before it.
/// </summary>
internal static class MoveForecast
{
	private static DateTime _frame;

	// Seconds until a hit that reaches the player lands, read once per frame (every spell of the rotation asks).
	private static float _mustLeaveIn = float.MaxValue;
	private static bool _wasMoving;
	private static bool _wasCasting;
	private static bool _castAnswered;
	private static uint _castAction;
	private static float _castElapsed;

	private static DateTime _castStart = DateTime.MinValue;

	// The hit whose dodge was measured last: a lead is taken once per hit, and only once the hit stops reaching the player
	// before it lands - a step taken for another reason teaches nothing (code review A883).
	private static DateTime _sampledLanding = DateTime.MinValue;
	private static float _candidateLead = float.NaN;
	private static DateTime _candidateLanding = DateTime.MinValue;
	private static readonly List<float> _leads = [];
	private static readonly List<float> _pauses = [];

	// After a walk broke a spell, until a spell ends whole: a phase in which the player stops and walks again.
	private static bool _inPhase;
	private static DateTime _stoppedAt = DateTime.MinValue;

	/// <summary>The median of the measured leads, seconds before the landing; 0 without a measurement.</summary>
	internal static float Lead { get; private set; }

	/// <summary>The median of the measured pauses in a walking phase; 0 without a measurement.</summary>
	internal static float Pause { get; private set; }

	/// <summary>
	/// Whether a spell of <paramref name="castTime"/> seconds must not begin: it would still run when the player steps off
	/// a hit by his measured lead, or he stands in a pause after a broken spell shorter than his measured one.
	/// </summary>
	internal static bool Refuses(float castTime)
	{
		Observe();
		if (castTime <= 0f)
		{
			return false;
		}

		if (_inPhase && !_wasMoving && (DateTime.Now - _stoppedAt).TotalSeconds < Pause)
		{
			return true;
		}

		return _mustLeaveIn < float.MaxValue && castTime > _mustLeaveIn - Lead;
	}

	// Seconds until a hit that reaches the player lands: RSR's certain one, or BossMod's zone on his spot.
	private static float MustLeaveIn()
	{
		var certain = MovementLock.CertainDodgeableWithin(float.MaxValue)?.LandsIn ?? float.MaxValue;
		return Math.Min(certain, MovementLock.SpotDangerIn());
	}

	/// <summary>
	/// Once per game frame, from the movement update (every frame, the spell's own frames included - the rotation does not
	/// ask for spells while one runs): the walking, the spell, and what to learn from them.
	/// </summary>
	internal static unsafe void Observe()
	{
		var frame = Svc.Framework.LastUpdate;
		if (frame == _frame)
		{
			return;
		}

		_frame = frame;
		var player = Player.Object;
		if (player == null)
		{
			_mustLeaveIn = float.MaxValue;
			return;
		}

		var now = DateTime.Now;
		var moving = Player.IsMoving || DataCenter.BMRMoveRequested;
		// Read only where the spell start uses it: in combat and without the lock (the reads walk BossMod's zones).
		_mustLeaveIn = DataCenter.InCombat && DataCenter.NoPoslock ? MustLeaveIn() : float.MaxValue;

		if (!float.IsNaN(_candidateLead))
		{
			if (now >= _candidateLanding)
			{
				_candidateLead = float.NaN;
			}
			else if (_mustLeaveIn == float.MaxValue)
			{
				Learn(_leads, _candidateLead, value => Lead = value, "lead before a hit");
				_candidateLead = float.NaN;
			}
		}

		if (moving && !_wasMoving)
		{
			if (_mustLeaveIn < float.MaxValue && now > _sampledLanding)
			{
				_sampledLanding = now.AddSeconds(_mustLeaveIn);
				_candidateLead = _mustLeaveIn;
				_candidateLanding = _sampledLanding;
			}

			if (_inPhase)
			{
				var stood = (float)(now - _stoppedAt).TotalSeconds;
				if (stood <= DataCenter.DefaultGCDTotal)
				{
					Learn(_pauses, stood, value => Pause = value, "pause in a walking phase");
				}
			}
		}
		else if (!moving && _wasMoving)
		{
			_stoppedAt = now;
		}

		if (_inPhase && !moving && (now - _stoppedAt).TotalSeconds > DataCenter.DefaultGCDTotal)
		{
			_inPhase = false;
		}

		var casting = player.IsCasting;
		if (casting && !_wasCasting)
		{
			_castStart = now;
		}

		if (casting)
		{
			var info = ((FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)player.Address)->GetCastInfo();
			_castAnswered = info != null && info->SourceSequence != 0 && info->ResponseSourceSequence == info->SourceSequence;
			_castAction = player.CastActionId;
			_castElapsed = player.CurrentCastTime;
		}
		else if (_wasCasting)
		{
			if (!_castAnswered && moving && RotationSolver.Updaters.CancelCastUpdater.OwnCancelAt < _castStart)
			{
				_inPhase = true;
				DefenseTrace.Line($"cast broken by moving: #{_castAction} {_castElapsed:F2} s in | lead {Lead:F2} s ({_leads.Count})"
					+ $" | pause {Pause:F2} s ({_pauses.Count})");
			}
			else if (_castAnswered)
			{
				_inPhase = false;
			}
		}

		_wasCasting = casting;
		_wasMoving = moving;
	}

	private static void Learn(List<float> samples, float value, Action<float> set, string what)
	{
		samples.Add(value);
		var median = Median(samples);
		set(median);
		DefenseTrace.Line($"move forecast: {what} {value:F2} s, median {median:F2} s from {samples.Count}");
	}

	private static float Median(List<float> samples)
	{
		var sorted = samples.ToArray();
		Array.Sort(sorted);
		var mid = sorted.Length / 2;
		return sorted.Length % 2 == 1 ? sorted[mid] : (sorted[mid - 1] + sorted[mid]) / 2f;
	}
}
