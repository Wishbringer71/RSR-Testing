using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Hooks;
using ECommons.Hooks.ActionEffectTypes;
using RotationSolver.Basic.Configuration;
using RotationSolver.Helpers;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// When a status that damages every action - Pyretic, Causality - will land on the player: from the enemy casts whose
/// landing has laid one on a party member before, learned in play, and from BossModReborn's imminent special mode
/// (concept 03, "Status, unter denen nicht gehandelt werden soll", A439).
/// </summary>
/// <remarks>
/// The status is no warning: it comes with the hit, and an action still under way at that moment takes its damage at
/// once. "Heißes Herz" (#43322) killed the player that way while a Slipstream begun 0.8 s before the landing was still
/// casting (26.10.2026), and six minutes earlier a party member - whose landing would have taught it. The rules that hold
/// while the status lies read the status; this one answers the time before it. Recognition only: the press gate, the
/// cast check, the cast cancel and the auto-attack hold decide with <see cref="WouldMeetLanding"/>.
/// </remarks>
internal static class ActionPenaltyWatch
{
	// A cast of a learned action: when its bar ends, when it was last seen, and whether its landing reaches the player.
	private readonly record struct Cast(uint Action, DateTime EndsAt, DateTime SeenAt, bool ReachesPlayer);

	private static readonly Dictionary<ulong, Cast> _casting = [];
	// Casts whose bar has ended and whose landing has not been seen yet: the effect comes after the bar, and in between
	// nothing on the enemy shows it. Kept until the landing is seen, or for a GCD after the bar if it never comes.
	private static readonly Dictionary<ulong, Cast> _landing = [];
	private static readonly HashSet<ulong> _seen = [];
	private static readonly List<ulong> _gone = [];
	private static DateTime _tracedLanding = DateTime.MinValue;

	/// <summary>The enemy actions learned to lay such a status, as of this update.</summary>
	internal static int LearnedCount
	{
		get
		{
			lock (OtherConfiguration.ActionPenaltyGate)
			{
				return OtherConfiguration.ActionPenaltyCasts.Count;
			}
		}
	}

	/// <summary>
	/// Follows the casts of learned actions whose status is still listed, every enemy-side figure included - the helpers
	/// that resolve many of them.
	/// </summary>
	internal static void Update()
	{
		var player = Player.Object;
		if (player == null || !DataCenter.InCombat)
		{
			Clear();
			return;
		}

		var now = DateTime.Now;
		_seen.Clear();
		lock (OtherConfiguration.ActionPenaltyGate)
		{
			if (OtherConfiguration.ActionPenaltyCasts.Count > 0)
			{
				foreach (var obj in Svc.Objects)
				{
					if (obj is not IBattleChara caster || !caster.IsCasting || !caster.IsEnemySideFigure()
						|| !OtherConfiguration.ActionPenaltyCasts.TryGetValue(caster.CastActionId, out var status)
						|| !OtherConfiguration.NoCastingStatus.Contains(status))
					{
						continue;
					}

					var reaches = caster.CastTargetObjectId == player.GameObjectId || DataCenter.CastReachesPlayer(caster);
					_casting[caster.GameObjectId] = new Cast(caster.CastActionId,
						now.AddSeconds(Math.Max(0f, caster.TotalCastTime - caster.CurrentCastTime)), now, reaches);
					_ = _seen.Add(caster.GameObjectId);
				}
			}
		}

		// A bar gone at least one update before its end was stopped - interrupted, or its caster died - and lands nothing.
		// One gone at its end waits for the landing, which comes after the bar; a cast stopped there cannot be told from
		// one whose effect is late, and the wait is a GCD at most.
		_gone.Clear();
		foreach (var (caster, cast) in _casting)
		{
			if (!_seen.Contains(caster))
			{
				_gone.Add(caster);
				if (now + (now - cast.SeenAt) >= cast.EndsAt)
				{
					_landing[caster] = cast;
				}
			}
		}

		foreach (var caster in _gone)
		{
			_ = _casting.Remove(caster);
		}

		_gone.Clear();
		foreach (var (caster, cast) in _landing)
		{
			if (now > cast.EndsAt.AddSeconds(DataCenter.DefaultGCDTotal))
			{
				_gone.Add(caster);
			}
		}

		foreach (var caster in _gone)
		{
			_ = _landing.Remove(caster);
		}
	}

	/// <summary>
	/// An enemy action's landing: it resolves the wait for it, and if it laid a status that damages every action on a party
	/// member, the action is learned.
	/// </summary>
	internal static void ObserveLanding(ActionEffectSet set)
	{
		if (set.Action is not { } action || action.GetActionCate() == ActionCate.Autoattack)
		{
			return;
		}

		var sourceId = set.Source?.GameObjectId ?? 0;
		_gone.Clear();
		foreach (var (caster, cast) in _landing)
		{
			if (cast.Action == action.RowId && (sourceId == 0 || caster == sourceId))
			{
				_gone.Add(caster);
			}
		}

		foreach (var caster in _gone)
		{
			_ = _landing.Remove(caster);
		}

		var player = Player.Object;
		if (player == null)
		{
			return;
		}

		foreach (var effect in set.TargetEffects)
		{
			var onParty = effect.TargetID == player.GameObjectId;
			foreach (var member in DataCenter.PartyMembers)
			{
				onParty |= member != null && member.GameObjectId == effect.TargetID;
			}

			if (!onParty)
			{
				continue;
			}

			var laid = 0u;
			effect.ForEach(entry =>
			{
				if (entry.type == ActionEffectType.ApplyStatusEffectTarget && entry.value != 0
					&& OtherConfiguration.NoCastingStatus.Contains(entry.value) && NoCastingStatusHelper.DamagesEveryAction(entry.value))
				{
					laid = entry.value;
				}
			});

			if (laid != 0)
			{
				Learn(action, laid);
				return;
			}
		}
	}

	private static void Learn(Lumina.Excel.Sheets.Action action, uint status)
	{
		lock (OtherConfiguration.ActionPenaltyGate)
		{
			if (OtherConfiguration.ActionPenaltyCasts.TryGetValue(action.RowId, out var known) && known == status)
			{
				return;
			}

			OtherConfiguration.ActionPenaltyCasts[action.RowId] = status;
		}

		_ = OtherConfiguration.SaveActionPenaltyCasts();
		DefenseTrace.Line($"learned: {action.Name.ExtractText()} #{action.RowId} lays status #{status}, which damages every action"
			+ " - no action under way when it lands from now on");
	}

	/// <summary>
	/// Seconds until the earliest landing on the player of a status that damages every action; 0 while a cast has ended and
	/// its landing has not been seen; <see cref="float.MaxValue"/> when none is coming.
	/// </summary>
	internal static float LandsOnPlayerIn()
	{
		var now = DateTime.Now;
		var best = float.MaxValue;
		foreach (var cast in _casting.Values)
		{
			if (cast.ReachesPlayer)
			{
				best = Math.Min(best, Math.Max(0f, (float)(cast.EndsAt - now).TotalSeconds));
			}
		}

		foreach (var cast in _landing.Values)
		{
			if (cast.ReachesPlayer)
			{
				best = 0f;
			}
		}

		if (DataCenter.BMRSpecialModeType == SpecialMode.Pyretic && DataCenter.BMRSpecialModeIn < float.MaxValue)
		{
			best = Math.Min(best, Math.Max(0f, DataCenter.BMRSpecialModeIn));
		}

		return best;
	}

	/// <summary>
	/// Whether an action that takes <paramref name="castTime"/> seconds, pressed now, would still be under way when such a
	/// status lands on the player - with the measured time a press needs to reach the server before its snapshot.
	/// </summary>
	internal static bool WouldMeetLanding(float castTime)
	{
		var landsIn = LandsOnPlayerIn();
		return landsIn < float.MaxValue && castTime + Math.Max(0f, TankbusterForecast.RoundTrip) >= landsIn;
	}

	/// <summary>One trace line per coming landing, when a press, a cast or an auto-attack is held for it.</summary>
	internal static void NoteHold(string what)
	{
		var landsIn = LandsOnPlayerIn();
		var landing = DateTime.Now.AddSeconds(landsIn);
		if (landsIn == float.MaxValue || Math.Abs((landing - _tracedLanding).TotalSeconds) < DataCenter.DefaultGCDTotal)
		{
			return;
		}

		_tracedLanding = landing;
		DefenseTrace.Line($"held for a status that damages every action: {what}, it lands in {landsIn:F1} s"
			+ $" (round trip {Math.Max(0f, TankbusterForecast.RoundTrip):F2} s)");
	}

	internal static void Clear()
	{
		_casting.Clear();
		_landing.Clear();
	}
}
