using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using RotationSolver.Basic.Configuration;
using RotationSolver.IPC;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// When "Lock movement while casting" holds the character and when it yields (concept 03, "Bewegungssperre beim Wirken und
/// die KI von BossMod", A649-A652). The lock keeps movement from breaking a spell or a channel, never from taking the player
/// out of danger. During a spell a release costs nothing while nobody moves - BossMod's AI waits as long as its leeway allows,
/// the player moves only by hand - so it yields to every sign of danger. During a channel BossMod's AI does not know the
/// channel and walks off at once, so there the lock holds until walking becomes necessary.
/// </summary>
internal static class MovementLock
{
	/// <summary>
	/// A channel that ends upon moving: the status it keeps on the player; the party mitigation that lasts only while it runs
	/// (Passage of Arms' Arms Up - Collective Unconscious' reduction is given once by the cast and outlasts a break, its ring
	/// and Improvisation keep only a regen); the party mitigations of the same job that replace it for an announced hit;
	/// whether it only fills a pause (Meditate); and whether its main effect outlasts a break (Collective Unconscious).
	/// </summary>
	internal readonly record struct Channel(ActionID Action, StatusID Self, StatusID? Party, ActionID[] Replacements,
		bool PauseFiller, bool OutlastsBreak, Func<bool> Ticked);

	/// <summary>
	/// The channels of the jobs in play whose effect text says "Effect ends upon using another action or moving"
	/// (ActionTransient, xivapi v2, 07.11.2026); <c>check_channel_locks.py</c> holds the list against the game data.
	/// </summary>
	internal static readonly Channel[] Channels =
	[
		new(ActionID.PassageOfArmsPvE, StatusID.PassageOfArms, StatusID.ArmsUp, [ActionID.DivineVeilPvE, ActionID.ReprisalPvE],
			false, false, () => Service.Config.PosPassageOfArms),
		new(ActionID.CollectiveUnconsciousPvE, StatusID.CollectiveUnconscious_848, null, [], false, true,
			() => Service.Config.PosCollectiveUnconscious),
		new(ActionID.ImprovisationPvE, StatusID.Improvisation, null, [], false, false, () => Service.Config.PosImprovisation),
		new(ActionID.FlamethrowerPvE, StatusID.Flamethrower, null, [], false, false, () => Service.Config.PosFlameThrower),
		new(ActionID.MeditatePvE, StatusID.Meditate, null, [], true, false, () => Service.Config.PosMeditate),
		new(ActionID.PhantomFlurryPvE, StatusID.PhantomFlurry, null, [], false, false, () => Service.Config.PosPhantomFlurry),
		new(ActionID.ApokalypsisPvE, StatusID.Apokalypsis, null, [], false, false, () => Service.Config.PosApokalypsis),
		new(ActionID.ChelonianGatePvE, StatusID.ChelonianGate, null, [], false, false, () => Service.Config.PosChelonianGate),
	];

	/// <summary>
	/// Measured in this session: the character moved while the lock held it and BossMod's AI wanted to move. Then the game's
	/// lock does not stop the AI, and a channel started while the AI walks ends at once.
	/// </summary>
	internal static bool LockFailsAgainstAi { get; private set; }

	/// <summary>Whether BossMod leaves the player's spot outside its forbidden zones; true without BossMod.</summary>
	internal static bool BossModSpotSafe()
	{
		var player = Player.Object;
		if (player == null)
		{
			return true;
		}

		// BossMod's own zone list says it exactly; its IPC answers through the dash check, which also refuses every spot
		// while a module forbids dashes.
		return SpotZoneActivation() is { } activation ? activation == float.MaxValue : DataCenter.IsMovementDestinationSafe(player.Position);
	}

	/// <summary>
	/// The player's spot is in danger: BossMod forbids it, or RSR sees a hit only a step avoids landing within
	/// <paramref name="window"/> seconds (<see cref="RaiseProtection.FirstDodgeableWithin"/>).
	/// </summary>
	internal static bool SpotInDanger(float window)
		=> !BossModSpotSafe() || RaiseProtection.FirstDodgeableWithin(window) is not null;

	/// <summary>
	/// The earliest enemy cast landing within <paramref name="window"/> seconds that certainly reaches the player and that a
	/// step avoids (<see cref="DataCenter.CastCertainlyDodgeable"/>): a spell ending after it is broken by the step or stands
	/// in the hit. Null when none.
	/// </summary>
	internal static AnnouncedHitOnPlayer? CertainDodgeableWithin(float window)
		=> EarliestCertainDodgeable() is { } hit && hit.LandsIn <= window ? hit : null;

	/// <summary>
	/// Whether a channel must not start now: a step would end it before it pays off - a BossMod zone on the spot or a cast
	/// that certainly reaches it lands within the channel's purpose (<see cref="PurposeLeft"/>), or BossMod's AI walks and
	/// must move at once (its leeway spent) or was measured not to be stopped by the game's lock. Only certain signs bar:
	/// RSR's broader sight counts an unlearned raidwide as one to step out of, and Passage of Arms would be barred right
	/// before the raidwide it is for. A channel whose main effect outlasts a break starts anyway. With and without the lock.
	/// </summary>
	internal static bool ChannelStartBarred(uint actionId)
	{
		foreach (var channel in Channels)
		{
			if ((uint)channel.Action != actionId)
			{
				continue;
			}

			if (channel.OutlastsBreak)
			{
				return false;
			}

			var duration = DefensiveValues.DurationByActionId.TryGetValue(actionId, out var seconds) ? seconds : float.MaxValue;
			var window = PurposeLeft(channel, duration);
			return SpotDangerIn() <= window || CertainDodgeableWithin(window) is not null
				|| (DataCenter.BMRIsNavigating && (LockFailsAgainstAi || (BossModLeewayKnown() && DataCenter.BMRMaxCastTime <= 0f)));
		}

		return false;
	}

	/// <summary>
	/// The ticked channel this action starts, if any (the adjusted id, as the press goes out); a pause filler is pressed only
	/// standing and never holds the player, so it is none here.
	/// </summary>
	internal static bool IsTickedChannelPress(ActionID action)
	{
		foreach (var channel in Channels)
		{
			if (!channel.PauseFiller && channel.Ticked() && Service.GetAdjustedActionId(channel.Action) == action)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>The ticked channel running on the player, and its seconds left.</summary>
	internal static bool ActiveTickedChannel(out Channel active, out float left)
	{
		var player = Player.Object;
		foreach (var channel in Channels)
		{
			if (player != null && channel.Ticked() && player.HasStatus(true, channel.Self))
			{
				active = channel;
				left = player.StatusTime(true, channel.Self);
				return true;
			}
		}

		active = default;
		left = 0f;
		return false;
	}

	/// <summary>
	/// Whether the lock lets go during a spell: the game has its answer (the slidecast window - moving no longer breaks it,
	/// <c>CastInfo.ResponseSourceSequence</c>), BossMod moves the player (it moves mid-spell only when its goal would
	/// otherwise be out of reach), or the spot is in danger before the spell ends.
	/// </summary>
	internal static unsafe bool CastYields(out string reason)
	{
		reason = string.Empty;
		var player = Player.Object;
		if (player == null)
		{
			return false;
		}

		var info = ((FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)player.Address)->GetCastInfo();
		if (info != null && info->SourceSequence != 0 && info->ResponseSourceSequence == info->SourceSequence)
		{
			reason = "the game has the spell";
			return true;
		}

		if (DataCenter.BMRIsMoving)
		{
			reason = "BossMod moves you";
			return true;
		}

		var left = info != null ? Math.Max(0f, info->TotalCastTime - info->CurrentCastTime) : 0f;
		return Danger(left, out reason);
	}

	/// <summary>Whether the lock lets go before a spell RSR is about to press: BossMod moves, or the spot is in danger.</summary>
	internal static bool PreCastYields(float castTime, out string reason)
	{
		if (DataCenter.BMRIsMoving)
		{
			reason = "BossMod moves you";
			return true;
		}

		return Danger(castTime, out reason);
	}

	/// <summary>
	/// Whether the lock lets go during a channel. The player's own key in danger always does. Without BossMod's AI walking,
	/// any danger before the channel ends does - then nobody moves but by hand. With it, only once walking becomes
	/// necessary (<see cref="AiMustMove"/>); and Passage of Arms holds on while its Arms Up covers a member in the critical
	/// class before an announced party hit, the player is not in it, and no other party mitigation of the job is ready -
	/// the party's safety first, weighed by what is at stake (CLAUDE.md, "Spielweise"). Meditate holds only against the AI.
	/// </summary>
	internal static bool ChannelYields(in Channel channel, float left, out string reason)
	{
		if (DataCenter.BMRMoveRequested && Danger(left, out reason))
		{
			reason = "you leave: " + reason;
			return true;
		}

		if (channel.PauseFiller)
		{
			return PauseFillerYields(left, out reason);
		}

		if (!DataCenter.BMRIsNavigating)
		{
			return Danger(left, out reason);
		}

		return AiMustMove(PurposeLeft(channel, left), out reason) && !HoldsForParty(channel);
	}

	// How long a channel still has to stand to pay off: a party mitigation up to the announced party hit it is for, the
	// rest for what remains of them.
	private static float PurposeLeft(in Channel channel, float left)
		=> channel.Party is not null ? Math.Min(left, AnnouncedPartyHitIn()) : left;

	// Seconds until the next announced hit on the party: BossMod's raidwide, or a listed area cast under way.
	private static float AnnouncedPartyHitIn()
	{
		var earliest = Service.Config.UseBmrTimeline && DataCenter.BMRNextRaidwideIn > 0f ? DataCenter.BMRNextRaidwideIn : float.MaxValue;
		var hostiles = DataCenter.AllHostileTargets;
		for (var i = 0; hostiles != null && i < hostiles.Count; i++)
		{
			var h = hostiles[i];
			if (h != null && h.IsCasting && OtherConfiguration.HostileCastingArea.Contains(h.CastActionId))
			{
				earliest = Math.Min(earliest, Math.Max(0f, h.TotalCastTime - h.CurrentCastTime));
			}
		}

		return earliest;
	}

	// Meditate fills a pause in which the Samurai walks to where the fight goes on: it holds only against BossMod's AI, and
	// only while BossMod's timeline shows the pause lasting longer than the walk - never against the player's own movement.
	// Not holding is no release worth a line, so the reason stays empty then.
	private static bool PauseFillerYields(float left, out string reason)
	{
		reason = string.Empty;
		var pause = DataCenter.BMRNextDowntimeEndIn;
		if (!DataCenter.BMRIsNavigating || DataCenter.BMRMoveRequested || pause == float.MaxValue || pause <= WalkToAiDestination())
		{
			return true;
		}

		return AiMustMove(left, out reason);
	}

	// When BossMod's AI must walk. Its movement module knows its leeway on its own path (Hints.MaxCastTime, from the path's
	// LeewaySeconds): the player may stand while it is above zero, as BossMod lets its own spell finish. The classic AI gives
	// none (unlimited); then a zone on the spot, or a hit RSR sees, before the channel ends with the walk counted in.
	private static bool AiMustMove(float left, out string reason)
	{
		if (BossModLeewayKnown())
		{
			reason = DataCenter.BMRMaxCastTime <= 0f ? "BossMod's leeway on its path is spent" : string.Empty;
			return DataCenter.BMRMaxCastTime <= 0f;
		}

		var horizon = left + WalkToAiDestination();
		var spot = SpotDangerIn();
		if (spot <= horizon)
		{
			reason = $"a BossMod zone on your spot turns active in {spot:F1} s";
			return true;
		}

		if (RaiseProtection.FirstDodgeableWithin(horizon) is { } hit)
		{
			reason = $"{hit.Name} lands in {hit.LandsIn:F1} s";
			return true;
		}

		reason = string.Empty;
		return false;
	}

	// BossMod's movement module reports its leeway as the cast limit; the classic AI leaves it unlimited, and the player's
	// own movement keys set it to zero.
	private static bool BossModLeewayKnown() => DataCenter.BMRMaxCastTime < float.MaxValue && !DataCenter.BMRMoveRequested;

	// Seconds until a BossMod zone on the player's spot turns active - from BossMod's zone list, else the earliest zone
	// anywhere while the spot is forbidden; float.MaxValue when the spot is free.
	private static float SpotDangerIn()
		=> SpotZoneActivation() ?? (BossModSpotSafe() ? float.MaxValue : DataCenter.BMRForbiddenZonesNextActivation);

	private static bool Danger(float window, out string reason)
	{
		if (!BossModSpotSafe())
		{
			reason = "BossMod forbids your spot";
			return true;
		}

		if (RaiseProtection.FirstDodgeableWithin(window) is { } hit)
		{
			reason = $"{hit.Name} lands in {hit.LandsIn:F1} s";
			return true;
		}

		reason = string.Empty;
		return false;
	}

	// Seconds the AI needs to its destination at the speed it reads; zero when either is unknown.
	private static float WalkToAiDestination()
	{
		var player = Player.Object;
		if (player == null || DataCenter.BMRNaviTargetPos is not { } target || DataCenter.BMRPlayerSpeed <= 0f)
		{
			return 0f;
		}

		var dx = target.X - player.Position.X;
		var dz = target.Z - player.Position.Z;
		return MathF.Sqrt((dx * dx) + (dz * dz)) / DataCenter.BMRPlayerSpeed;
	}

	private static bool HoldsForParty(in Channel channel)
	{
		var player = Player.Object;
		if (channel.Party is not { } party || player == null || !DataCenter.AreaHitPending || player.IsInCriticalClass())
		{
			return false;
		}

		foreach (var replacement in channel.Replacements)
		{
			if (ReadyNow(replacement))
			{
				return false;
			}
		}

		foreach (var member in DataCenter.PartyMembers)
		{
			if (member != null && member.GameObjectId != player.GameObjectId && !member.IsDead
				&& member.HasStatus(true, party) && member.IsInCriticalClass())
			{
				return true;
			}
		}

		return false;
	}

	// Whether the game would take the action now on the player: learned, off cooldown, its cost paid.
	private static unsafe bool ReadyNow(ActionID action)
	{
		var player = Player.Object;
		var manager = ActionManager.Instance();
		return player != null && manager != null
			&& manager->GetActionStatus(ActionType.Action, (uint)Service.GetAdjustedActionId(action), player.GameObjectId) == 0;
	}

	// Per game frame: the zone activation on the player's spot (several readers ask each frame) and the earliest certain hit.
	private static DateTime _spotFrame;
	private static float? _spotActivation;
	private static DateTime _certainFrame;
	private static AnnouncedHitOnPlayer? _certain;

	// Seconds until a BossMod zone on the player's spot turns active, float.MaxValue when none; null when the zones cannot
	// be read (no BossMod, or its list unreadable - then BossMod's IPC answers).
	private static float? SpotZoneActivation()
	{
		var frame = Svc.Framework.LastUpdate;
		if (frame != _spotFrame)
		{
			_spotFrame = frame;
			var player = Player.Object;
			_spotActivation = player == null || DataCenter.BMRIsPositionSafe == null ? null : BMRZones_Reflection.ActivationAt(player.Position);
		}

		return _spotActivation;
	}

	private static AnnouncedHitOnPlayer? EarliestCertainDodgeable()
	{
		var frame = Svc.Framework.LastUpdate;
		if (frame == _certainFrame)
		{
			return _certain;
		}

		_certainFrame = frame;
		_certain = null;
		var hostiles = DataCenter.AllHostileTargets;
		if (hostiles == null)
		{
			return null;
		}

		for (var i = 0; i < hostiles.Count; i++)
		{
			var h = hostiles[i];
			if (h == null || !DataCenter.CastCertainlyDodgeable(h))
			{
				continue;
			}

			var left = Math.Max(0f, h.TotalCastTime - h.CurrentCastTime);
			if (_certain is not { } earlier || left < earlier.LandsIn)
			{
				_certain = new($"#{h.CastActionId} from {h.Name.TextValue}", left);
			}
		}

		return _certain;
	}

	// The trace: one line per release, and once per hold whether the character moved while BossMod wanted to move.
	private static string _lastRelease = string.Empty;
	private static bool _heldWhileBossModMoved;
	private static bool _movedWhileHeld;

	/// <summary>
	/// Notes what the lock did this frame for the trace, and measures whether the game's lock stops BossMod's AI.
	/// <paramref name="lockWanted"/>: the lock would hold without its releases; <paramref name="held"/>: it holds.
	/// </summary>
	internal static void Note(bool lockWanted, bool held, string reason, string what)
	{
		if (held && DataCenter.BMRIsMoving)
		{
			_heldWhileBossModMoved = true;
			_movedWhileHeld |= Player.IsMoving && !DataCenter.BMRMoveRequested;
		}
		else if (_heldWhileBossModMoved && !held)
		{
			DefenseTrace.Line($"movement lock held while BossMod wanted to move; the character moved: {(_movedWhileHeld ? "yes" : "no")}");
			if (_movedWhileHeld && !LockFailsAgainstAi)
			{
				LockFailsAgainstAi = true;
				DefenseTrace.Line("the game's movement lock does not stop BossMod's AI: ticked channels wait while it walks");
			}

			_heldWhileBossModMoved = false;
			_movedWhileHeld = false;
		}

		var release = lockWanted && !held && reason.Length > 0 ? $"{what}: {reason}" : string.Empty;
		if (release.Length > 0 && release != _lastRelease)
		{
			DefenseTrace.Line($"movement lock yields during {release}; spot {(BossModSpotSafe() ? "safe" : "forbidden")} by BossMod");
		}

		_lastRelease = release;
	}
}
