using ECommons.GameHelpers;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// When "Lock movement while casting" holds the character and when it yields (concept 03, "Bewegungssperre beim Wirken und
/// die KI von BossMod", A651). The lock keeps movement from breaking a spell or a channel, never from taking the player out
/// of danger. During a spell a release costs nothing while nobody moves - BossMod's AI waits as long as its leeway allows,
/// the player moves only by hand - so it yields to every sign of danger. During a channel BossMod's AI does not know the
/// channel and walks off at once, so there the release waits for danger that lands before the channel ends.
/// </summary>
internal static class MovementLock
{
	/// <summary>
	/// A channel that ends upon moving, the status it keeps on the player, and the party mitigation that lasts only while it
	/// runs - Passage of Arms' Arms Up. Collective Unconscious' reduction (849) is given once by the cast and outlasts a
	/// break, and its ring and Improvisation keep only a regen: they carry none (concept 03, A651).
	/// </summary>
	internal readonly record struct Channel(ActionID Action, StatusID Self, StatusID? Party, Func<bool> Ticked);

	/// <summary>
	/// The channels of the jobs in play whose effect text says "Effect ends upon using another action or moving"
	/// (ActionTransient, xivapi v2, 07.11.2026); <c>check_channel_locks.py</c> holds the list against the game data.
	/// </summary>
	internal static readonly Channel[] Channels =
	[
		new(ActionID.PassageOfArmsPvE, StatusID.PassageOfArms, StatusID.ArmsUp, () => Service.Config.PosPassageOfArms),
		new(ActionID.CollectiveUnconsciousPvE, StatusID.CollectiveUnconscious_848, null, () => Service.Config.PosCollectiveUnconscious),
		new(ActionID.ImprovisationPvE, StatusID.Improvisation, null, () => Service.Config.PosImprovisation),
		new(ActionID.FlamethrowerPvE, StatusID.Flamethrower, null, () => Service.Config.PosFlameThrower),
		new(ActionID.MeditatePvE, StatusID.Meditate, null, () => Service.Config.PosMeditate),
		new(ActionID.PhantomFlurryPvE, StatusID.PhantomFlurry, null, () => Service.Config.PosPhantomFlurry),
		new(ActionID.ApokalypsisPvE, StatusID.Apokalypsis, null, () => Service.Config.PosApokalypsis),
		new(ActionID.ChelonianGatePvE, StatusID.ChelonianGate, null, () => Service.Config.PosChelonianGate),
	];

	/// <summary>Whether BossMod leaves the player's spot outside its forbidden zones; true without BossMod.</summary>
	internal static bool BossModSpotSafe()
	{
		var player = Player.Object;
		return player == null || DataCenter.IsMovementDestinationSafe(player.Position);
	}

	/// <summary>
	/// The player's spot is in danger: BossMod forbids it, or RSR sees a hit only a step avoids landing within
	/// <paramref name="window"/> seconds (<see cref="RaiseProtection.FirstDodgeableWithin"/>).
	/// </summary>
	internal static bool SpotInDanger(float window)
		=> !BossModSpotSafe() || RaiseProtection.FirstDodgeableWithin(window) is not null;

	/// <summary>
	/// Whether a channel must not start now: BossMod forbids the spot, so its AI or the player leaves it, and the first step
	/// ends the channel. Only BossMod's sign bars - RSR's own detection counts an unlearned raidwide as one to step out of,
	/// and Passage of Arms would be barred right before the raidwide it is for. With and without the lock.
	/// </summary>
	internal static bool ChannelStartBarred(uint actionId)
	{
		foreach (var channel in Channels)
		{
			if ((uint)channel.Action == actionId)
			{
				return !BossModSpotSafe();
			}
		}

		return false;
	}

	/// <summary>The ticked channel this action starts, if any (the adjusted id, as the press goes out).</summary>
	internal static bool IsTickedChannelPress(ActionID action)
	{
		foreach (var channel in Channels)
		{
			if (channel.Ticked() && Service.GetAdjustedActionId(channel.Action) == action)
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
	/// any danger does - then nobody moves but by hand. With it, danger that lands before the channel ends, the walk to the
	/// AI's destination counted in; but a party mitigation that ends with the channel holds against the AI while it covers a
	/// member in the critical class before an announced party hit and the player is not in it - the party's safety first, weighed by what
	/// is at stake (CLAUDE.md, "Spielweise").
	/// </summary>
	internal static bool ChannelYields(in Channel channel, float left, out string reason)
	{
		if (DataCenter.BMRMoveRequested && Danger(left, out reason))
		{
			reason = "you leave: " + reason;
			return true;
		}

		if (!DataCenter.BMRIsNavigating)
		{
			return Danger(left, out reason);
		}

		var horizon = left + WalkToAiDestination();
		reason = string.Empty;
		if (!BossModSpotSafe() && DataCenter.BMRForbiddenZonesNextActivation <= horizon)
		{
			reason = $"BossMod forbids your spot, a zone turns active in {DataCenter.BMRForbiddenZonesNextActivation:F1} s";
		}
		else if (RaiseProtection.FirstDodgeableWithin(horizon) is { } hit)
		{
			reason = $"{hit.Name} lands in {hit.LandsIn:F1} s";
		}
		else
		{
			return false;
		}

		return !CoversSomeoneInDanger(channel);
	}

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

	private static bool CoversSomeoneInDanger(in Channel channel)
	{
		var player = Player.Object;
		if (channel.Party is not { } party || player == null || !DataCenter.AreaHitPending || player.IsInCriticalClass())
		{
			return false;
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

	// The trace: one line per release, and once per hold whether the character moved while BossMod wanted to move.
	private static string _lastRelease = string.Empty;
	private static bool _heldWhileBossModMoved;
	private static bool _movedWhileHeld;

	/// <summary>
	/// Notes what the lock did this frame for the trace. <paramref name="lockWanted"/>: the lock would hold without its
	/// releases; <paramref name="held"/>: it holds.
	/// </summary>
	internal static void Note(bool lockWanted, bool held, string reason, string what)
	{
		if (held && DataCenter.BMRIsMoving)
		{
			_heldWhileBossModMoved = true;
			_movedWhileHeld |= Player.IsMoving;
		}
		else if (_heldWhileBossModMoved && !held)
		{
			DefenseTrace.Line($"movement lock held while BossMod wanted to move; the character moved: {(_movedWhileHeld ? "yes" : "no")}");
			_heldWhileBossModMoved = false;
			_movedWhileHeld = false;
		}

		var release = lockWanted && !held ? $"{what}: {reason}" : string.Empty;
		if (release.Length > 0 && release != _lastRelease)
		{
			DefenseTrace.Line($"movement lock yields during {release}; spot {(BossModSpotSafe() ? "safe" : "forbidden")} by BossMod");
		}

		_lastRelease = release;
	}
}
