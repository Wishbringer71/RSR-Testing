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
	// A cast of a learned action: when its bar ends, when it was last seen, whether its landing reaches the player, and its
	// target as the cast bar shows it - the one landing that can show the action no longer lays the status (concept 13,
	// "Stufe 4", 4c).
	private readonly record struct Cast(uint Action, DateTime EndsAt, DateTime SeenAt, bool ReachesPlayer, ulong Target);

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

	/// <summary>Of <see cref="LearnedCount"/>, the actions a game version refuted: they hold no spells (concept 13, "Stufe 4", 4c).</summary>
	internal static int RefutedCount
	{
		get
		{
			lock (OtherConfiguration.ActionPenaltyGate)
			{
				var refuted = 0;
				foreach (var action in OtherConfiguration.ActionPenaltyCasts.Keys)
				{
					refuted += PenaltySightingRules.IsRefuted(OtherConfiguration.ActionPenaltySightings, action) ? 1 : 0;
				}

				return refuted;
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
					// A learned action the running game version refuted holds nothing until it lays the status again (concept 13,
					// "Stufe 4", 4c).
					if (obj is not IBattleChara caster || !caster.IsCasting || !caster.IsEnemySideFigure()
						|| !OtherConfiguration.ActionPenaltyCasts.TryGetValue(caster.CastActionId, out var status)
						|| !OtherConfiguration.NoCastingStatus.Contains(status)
						|| PenaltySightingRules.IsRefuted(OtherConfiguration.ActionPenaltySightings, caster.CastActionId))
					{
						continue;
					}

					var reaches = caster.CastTargetObjectId == player.GameObjectId || DataCenter.CastReachesPlayer(caster);
					_casting[caster.GameObjectId] = new Cast(caster.CastActionId,
						now.AddSeconds(Math.Max(0f, caster.TotalCastTime - caster.CurrentCastTime)), now, reaches, caster.CastTargetObjectId);
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
	/// An enemy action's landing: it resolves the wait for it; if it laid a status that damages every action on a party
	/// member, the action is learned; and if a learned action hit its target, a party member, without laying it, that is
	/// counted towards refuting it (concept 13, "Stufe 4", 4c).
	/// </summary>
	internal static void ObserveLanding(ActionEffectSet set)
	{
		if (set.Action is not { } action || action.GetActionCate() == ActionCate.Autoattack)
		{
			return;
		}

		// The cast this landing ends, by the same caster: its target, as the cast bar showed it. The landing can come before
		// the bar is seen gone.
		var sourceId = set.Source?.GameObjectId ?? 0;
		var castTarget = 0ul;
		if (sourceId != 0 && _casting.TryGetValue(sourceId, out var running) && running.Action == action.RowId)
		{
			castTarget = running.Target;
		}

		_gone.Clear();
		foreach (var (caster, cast) in _landing)
		{
			if (cast.Action == action.RowId && (sourceId == 0 || caster == sourceId))
			{
				_gone.Add(caster);
				if (caster == sourceId)
				{
					castTarget = cast.Target;
				}
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

		var laid = 0u;
		var targetHit = false;
		var targetUnclear = false;
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

			var isTarget = castTarget != 0 && effect.TargetID == castTarget;
			effect.ForEach(entry =>
			{
				if (entry.type == ActionEffectType.ApplyStatusEffectTarget && entry.value != 0
					&& OtherConfiguration.NoCastingStatus.Contains(entry.value) && NoCastingStatusHelper.DamagesEveryAction(entry.value))
				{
					laid = entry.value;
				}
				else if (isTarget && entry.type is ActionEffectType.Damage or ActionEffectType.BlockedDamage or ActionEffectType.ParriedDamage)
				{
					targetHit = true;
				}
				// A status resisted or without effect, an invulnerable or missed target: the landing does not say whether
				// the action still lays the status.
				else if (isTarget && entry.type is ActionEffectType.FullResistStatus or ActionEffectType.FullResistStatus2
					or ActionEffectType.StatusNoEffect or ActionEffectType.Invulnerable or ActionEffectType.PartialInvulnerable
					or ActionEffectType.FullResist or ActionEffectType.Miss)
				{
					targetUnclear = true;
				}
			});
		}

		if (laid != 0)
		{
			Learn(action, laid);
			return;
		}

		if (castTarget != 0 && targetHit && !targetUnclear)
		{
			SeenWithout(action, castTarget);
		}
	}

	private static void Learn(Lumina.Excel.Sheets.Action action, uint status)
	{
		var game = OtherConfiguration.GameVersion;
		bool known, lifted;
		lock (OtherConfiguration.ActionPenaltyGate)
		{
			var learned = OtherConfiguration.ActionPenaltyCasts.TryGetValue(action.RowId, out var was);
			known = learned && was == status;
			if (!known)
			{
				OtherConfiguration.ActionPenaltyCasts[action.RowId] = status;
			}

			lifted = PenaltySightingRules.SeenWith(OtherConfiguration.ActionPenaltySightings, action.RowId, learned, game);
		}

		_ = OtherConfiguration.SaveActionPenaltySightings();
		if (lifted)
		{
			DefenseTrace.Line($"{action.Name.ExtractText()} #{action.RowId} laid status #{status} again: no longer refuted"
				+ " - no action under way when it lands from now on");
		}

		if (known)
		{
			return;
		}

		_ = OtherConfiguration.SaveActionPenaltyCasts();
		DefenseTrace.Line($"learned: {action.Name.ExtractText()} #{action.RowId} lays status #{status}, which damages every action"
			+ " - no action under way when it lands from now on");
	}

	// A learned action landed on its target, a party member, with damage and laid the status on nobody. A target that carried
	// it already says nothing; nor does a landing in a game version that cannot be read.
	private static void SeenWithout(Lumina.Excel.Sheets.Action action, ulong target)
	{
		var game = OtherConfiguration.GameVersion;
		uint status;
		lock (OtherConfiguration.ActionPenaltyGate)
		{
			if (game == null || !OtherConfiguration.ActionPenaltyCasts.TryGetValue(action.RowId, out status))
			{
				return;
			}
		}

		if (Svc.Objects.SearchById(target) is IBattleChara member && member.HasStatus(false, (StatusID)status))
		{
			return;
		}

		bool refuted;
		PenaltySighting? sighting;
		lock (OtherConfiguration.ActionPenaltyGate)
		{
			refuted = PenaltySightingRules.SeenWithout(OtherConfiguration.ActionPenaltySightings, action.RowId, game);
			sighting = OtherConfiguration.ActionPenaltySightings.GetValueOrDefault(action.RowId);
		}

		if (sighting == null || sighting.Game != game)
		{
			return;
		}

		_ = OtherConfiguration.SaveActionPenaltySightings();
		DefenseTrace.Line($"{action.Name.ExtractText()} #{action.RowId} hit its target without status #{status}: {sighting.Without} landing(s) without,"
			+ $" {sighting.With} with it in game version {game}, {sighting.Before} with it before"
			+ (refuted ? " - refuted: it holds no spells until it lays the status again" : string.Empty));
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
