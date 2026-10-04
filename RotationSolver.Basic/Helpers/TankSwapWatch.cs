using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// What the fight shows about a tank swap after a tankbuster on the player: whether a repeat of that
/// buster would kill him, and which other tank in the party would hold its source after a Shirk.
/// The owner's proposal of 04.10.2026 (concept 09, "Tankwechsel nach einem Tankbuster").
/// </summary>
/// <remarks>
/// <para>Detection only: nothing here reads the option or casts. The consumer
/// (<c>CustomRotation.TankSwapAbility</c>) decides, and writes the decision into the trace.</para>
///
/// <para>Whether Shirk moves the enemy at all is measured, not assumed. Its effect text moves a share
/// s of the user's enmity to the target (<see cref="DefensiveValues.EnmityTransferOf"/>, 25%), so the
/// receiver becomes the target from this side alone only when it held more than 1 - 2s of the user's
/// enmity beforehand. The party's enmity on the player's current target is in the game's own list
/// (<c>UIState.Hate</c>, the party list's enmity figures, 0 to 100 relative to the leader). Below that
/// the Shirk would change nothing about who takes the next hit and only cost the action for the
/// planned swap, so no candidate is named.</para>
///
/// <para>Self-correcting: the first auto-attack of the source after a swap Shirk shows whom it now
/// attacks. A Shirk that did not move it raises the margin the next one needs, for the rest of the
/// fight, by exactly the margin that failed.</para>
/// </remarks>
internal static class TankSwapWatch
{
	private sealed class Buster
	{
		public required ulong Source { get; init; }
		public required uint ActionId { get; init; }
		public required string Name { get; init; }
		public required float Share { get; init; }
		public required DateTime At { get; init; }
		public required uint[] Vulnerabilities { get; init; }
	}

	private sealed class Pending
	{
		public required ulong Source { get; init; }
		public required ulong Receiver { get; init; }
		public required string ReceiverName { get; init; }
		public required float Ratio { get; init; }
	}

	// The effect handler records from the game thread, the update reads on the framework thread.
	private static readonly object _gate = new();
	private static Buster? _last;
	private static Pending? _planned;
	private static Pending? _pending;
	// A swap that moved the enemy, until the player has taken it back or it is gone.
	private static Pending? _swappedAway;
	// Whether the enemy's next tankbuster has gone to the tank holding it since the swap.
	private static bool _holderTookBuster;
	private static string _reclaimHeldWritten = string.Empty;
	private static readonly Dictionary<uint, float> _hardestShare = [];
	// The after-transfer ratio receiver/player that last failed to move the source; a swap needs more.
	private static float _failedRatio = 1f;
	private static HashSet<uint>? _vulnerabilityIds;
	private static DateTime _heldWritten = DateTime.MinValue;

	/// <summary>The tank a Shirk would hand the buster's source to, or null when none would hold it.</summary>
	public static IBattleChara? Target { get; private set; }

	/// <summary>Why the player is in danger after the buster, or empty when he is not.</summary>
	public static string Danger { get; private set; } = string.Empty;

	/// <summary>The receiver's figures for the trace line, or why there is none.</summary>
	public static string Detail { get; private set; } = string.Empty;

	/// <summary>
	/// Whether the player is in danger after a tankbuster right now - the state in which nothing may
	/// take the enemy back to him (the owner's rule of 04.10.2026: provoke back only once the debuff
	/// and the critical state are over).
	/// </summary>
	public static bool PlayerInDanger { get; private set; }

	/// <summary>
	/// The enemy a swap handed to the co-tank, once the player is out of danger again; null otherwise.
	/// Whether his invulnerability is back is the consumer's question.
	/// </summary>
	public static IBattleChara? ReclaimSource { get; private set; }

	/// <summary>The source of the last tankbuster on the player while the danger holds.</summary>
	public static ulong DangerSource => Danger.Length > 0 && _last != null ? _last.Source : 0;

	/// <summary>
	/// An enemy action that is not an auto-attack reached the player. Called from the effect handler,
	/// on the game thread. <paramref name="markedForPlayer"/>: a tankbuster marker on the player stood
	/// for this hit and is not on the learned list of markers without one.
	/// </summary>
	public static void RecordHitOnPlayer(uint actionId, string name, ulong source, float share, bool markedForPlayer, List<uint> appliedStatuses)
	{
		lock (_gate)
		{
			if (!markedForPlayer && !OtherConfiguration.HostileCastingTank.Contains(actionId))
			{
				return;
			}

			List<uint> vulnerabilities = [];
			foreach (var status in appliedStatuses)
			{
				if (IsVulnerability(status))
				{
					vulnerabilities.Add(status);
				}
			}

			_last = new Buster
			{
				Source = source,
				ActionId = actionId,
				Name = name,
				Share = share,
				At = DateTime.Now,
				Vulnerabilities = [.. vulnerabilities],
			};

			if (!_hardestShare.TryGetValue(actionId, out var hardest) || share > hardest)
			{
				_hardestShare[actionId] = share;
			}

			DefenseTrace.Line($"tankbuster on you: {name} #{actionId} for {share:P0} of max HP"
				+ (vulnerabilities.Count > 0 ? $", vulnerability {Describe(vulnerabilities)}" : string.Empty));
		}
	}

	/// <summary>
	/// An auto-attack from <paramref name="source"/> reached <paramref name="target"/>. Settles an open
	/// swap Shirk: the auto-attack goes to whoever holds the enemy now.
	/// </summary>
	public static void RecordAutoAttack(ulong source, ulong target)
	{
		lock (_gate)
		{
			if (_pending is not { } pending || pending.Source != source)
			{
				return;
			}

			_pending = null;
			if (target == pending.Receiver)
			{
				Moved(pending);
				return;
			}

			if (target == Player.Object?.GameObjectId)
			{
				if (pending.Ratio > _failedRatio)
				{
					_failedRatio = pending.Ratio;
				}

				DefenseTrace.Line($"tank swap did not move the enemy: it still attacks you; the next one needs more than"
					+ $" {_failedRatio:F2} of your enmity after the transfer");
			}
		}
	}

	private static void Moved(Pending pending)
	{
		_swappedAway = pending;
		_holderTookBuster = false;
		_reclaimHeldWritten = string.Empty;
		DefenseTrace.Line($"tank swap moved the enemy: it now attacks {pending.ReceiverName}");
	}

	/// <summary>
	/// A tankbuster from <paramref name="source"/> reached <paramref name="target"/>, someone other than
	/// the player. After a swap this is the moment the enemy may come back: the swap follows the
	/// rhythm of the busters, and taking it back before the holder's buster would hand the player the
	/// next one himself and leave the co-tank's cooldowns unspent (the owner's question of 04.10.2026).
	/// </summary>
	public static void RecordBusterOnOther(ulong source, ulong target, string name)
	{
		lock (_gate)
		{
			// The holder: the receiver, or whoever the enemy attacks now if it has moved on.
			if (_swappedAway is { } away && away.Source == source && !_holderTookBuster
				&& (target == away.Receiver || Svc.Objects.SearchById(source) is IBattleChara enemy && enemy.TargetObjectId == target))
			{
				_holderTookBuster = true;
				var holder = Svc.Objects.SearchById(target)?.Name.TextValue ?? "the co-tank";
				DefenseTrace.Line($"tankbuster on {holder}: {name} - the enemy may come back once you are safe");
			}
		}
	}

	/// <summary>
	/// A party member's Shirk reached the player. Written to the trace only: whether to hand the enemy
	/// back is decided by that member's measured danger, which covers the case without this signal.
	/// </summary>
	public static void RecordShirkOnPlayer(string from)
	{
		DefenseTrace.Line($"{from} shirked to you");
	}

	/// <summary>Writes why the enemy is not yet taken back, once per reason and swap.</summary>
	public static void TraceReclaimHeld(string why)
	{
		lock (_gate)
		{
			if (_swappedAway == null || _reclaimHeldWritten == why)
			{
				return;
			}

			_reclaimHeldWritten = why;
			DefenseTrace.Line($"tank swap back waits: {why}");
		}
	}

	/// <summary>The consumer chose Shirk on <paramref name="receiver"/> for a swap.</summary>
	public static void PlanSwapShirk(ulong source, IBattleChara receiver, float ratio)
	{
		lock (_gate)
		{
			_planned = new Pending { Source = source, Receiver = receiver.GameObjectId, ReceiverName = receiver.Name.TextValue, Ratio = ratio };
		}
	}

	/// <summary>
	/// The player's Shirk went out. Only now is the swap open to be settled: a choice the game never
	/// carried out must not count against the margin.
	/// </summary>
	public static void ShirkLanded()
	{
		lock (_gate)
		{
			if (_planned != null)
			{
				_pending = _planned;
				_planned = null;
			}
		}
	}

	/// <summary>Writes why a danger has no receiver, once per tankbuster.</summary>
	public static void TraceHeld(string why)
	{
		lock (_gate)
		{
			if (_last == null || _heldWritten == _last.At)
			{
				return;
			}

			_heldWritten = _last.At;
			DefenseTrace.Line($"tank swap held: {Danger}; {why}");
		}
	}

	/// <summary>Recomputes the danger and the receiver. Called once per framework cycle.</summary>
	public static unsafe void Update()
	{
		lock (_gate)
		{
			Target = null;
			Danger = string.Empty;
			Detail = string.Empty;
			PlayerInDanger = false;
			ReclaimSource = null;

			if (!DataCenter.InCombat)
			{
				_last = null;
				_planned = null;
				_pending = null;
				_swappedAway = null;
				_hardestShare.Clear();
				_failedRatio = 1f;
				return;
			}

			var player = Player.Object;
			if (player == null || _last == null || player.IsDead)
			{
				return;
			}

			// The enemy's own target shows a moved swap at once; the auto-attack check is the fallback.
			if (_pending is { } open && Svc.Objects.SearchById(open.Source) is IBattleChara swung
				&& swung.TargetObjectId == open.Receiver)
			{
				_pending = null;
				Moved(open);
			}

			if (Svc.Objects.SearchById(_last.Source) is not IBattleChara source || source.IsDead || !source.IsTargetable)
			{
				return;
			}

			// Nothing to save while the player cannot die.
			if (player.HasStatus(false, StatusHelper.NoNeedHealingStatus) || player.HasStatus(false, StatusID.WalkingDead))
			{
				return;
			}

			Danger = DangerOf(player);
			PlayerInDanger = Danger.Length > 0;
			if (!PlayerInDanger)
			{
				UpdateReclaim(player);
				return;
			}

			var transfer = DefensiveValues.EnmityTransferOf((uint)ActionID.ShirkPvE);
			if (transfer <= 0f)
			{
				Detail = "Shirk's effect text states no enmity share";
				return;
			}

			var hate = &UIState.Instance()->Hate;
			if (hate->HateTargetId != source.EntityId)
			{
				Detail = $"your target is not {source.Name.TextValue}, so the party's enmity on it cannot be read";
				return;
			}

			float mine = 0f;
			Dictionary<uint, float> enmity = [];
			for (var i = 0; i < hate->HateArrayLength; i++)
			{
				var info = hate->HateInfo[i];
				if (info.EntityId == 0)
				{
					continue;
				}

				enmity[info.EntityId] = info.Enmity;
				if (info.EntityId == player.EntityId)
				{
					mine = info.Enmity;
				}
			}

			if (mine <= 0f)
			{
				Detail = "your own enmity on the enemy is not in the list";
				return;
			}

			IBattleChara? best = null;
			var bestLoad = int.MaxValue;
			var bestHealth = 0f;
			var bestRatio = 0f;
			var bestEnmity = 0f;
			var anyTank = false;
			var why = string.Empty;

			foreach (var member in DataCenter.PartyMembers)
			{
				if (member == null || member.GameObjectId == player.GameObjectId || !member.IsJobCategory(JobRole.Tank))
				{
					continue;
				}

				anyTank = true;
				if (member.IsDead || !member.IsTargetable)
				{
					why = $"{member.Name.TextValue} is dead or cannot be targeted";
					continue;
				}

				if (CarriesVulnerability(member))
				{
					why = $"{member.Name.TextValue} carries a vulnerability too";
					continue;
				}

				// The owner's case of 04.10.2026: the other tank may be the one in danger - he may even
				// have shirked to you for it. Handing him the enemy back would trade one death for another.
				if (member.CurrentHp + member.GetObjectShield() <= HardestShare() * Math.Max(1u, member.MaxHp))
				{
					why = $"{member.Name.TextValue} would not survive a repeat either";
					continue;
				}

				var theirs = enmity.TryGetValue(member.EntityId, out var value) ? value : 0f;
				var top = 0f;
				foreach (var pair in enmity)
				{
					if (pair.Key != player.EntityId && pair.Key != member.EntityId && pair.Value > top)
					{
						top = pair.Value;
					}
				}

				var after = theirs + (transfer * mine);
				var yoursAfter = mine * (1f - transfer);
				var ratio = yoursAfter > 0f ? after / yoursAfter : float.MaxValue;
				if (ratio <= _failedRatio || after < top)
				{
					why = $"{member.Name.TextValue} holds {theirs:F0}% against your {mine:F0}%, too little to keep the enemy after a Shirk";
					continue;
				}

				var load = 0;
				foreach (var hostile in DataCenter.AllHostileTargets)
				{
					if (hostile != null && hostile.TargetObjectId == member.GameObjectId)
					{
						load++;
					}
				}

				var health = member.GetHealthRatio();
				if (best == null || load < bestLoad || (load == bestLoad && health > bestHealth))
				{
					best = member;
					bestLoad = load;
					bestHealth = health;
					bestRatio = ratio;
					bestEnmity = theirs;
				}
			}

			if (best == null)
			{
				Detail = anyTank ? why : "no other tank in the party";
				return;
			}

			Target = best;
			Ratio = bestRatio;
			Detail = $"{best.Name.TextValue} holds {bestEnmity:F0}% against your {mine:F0}%, attacked by {bestLoad}, {bestHealth:P0} HP";
		}
	}

	/// <summary>The after-transfer ratio of the current <see cref="Target"/>.</summary>
	public static float Ratio { get; private set; }

	private static void UpdateReclaim(IBattleChara player)
	{
		if (_swappedAway is not { } away)
		{
			return;
		}

		if (Svc.Objects.SearchById(away.Source) is not IBattleChara source || source.IsDead || !source.IsTargetable)
		{
			_swappedAway = null;
			return;
		}

		if (source.TargetObjectId == player.GameObjectId)
		{
			_swappedAway = null;
			return;
		}

		if (!_holderTookBuster)
		{
			TraceReclaimHeld($"{away.ReceiverName} has not taken a tankbuster from {source.Name.TextValue} yet");
			return;
		}

		ReclaimSource = source;
	}

	private static float HardestShare()
	{
		var last = _last!;
		return _hardestShare.TryGetValue(last.ActionId, out var share) ? share : last.Share;
	}

	private static string DangerOf(IBattleChara player)
	{
		var last = _last!;

		foreach (var status in last.Vulnerabilities)
		{
			if (player.HasStatus(false, (StatusID)status))
			{
				return $"{last.Name} left you with {Describe([status])}";
			}
		}

		var hardest = HardestShare();
		var maxHp = Math.Max(1u, player.MaxHp);
		var standing = player.CurrentHp + player.GetObjectShield();
		if (standing <= hardest * maxHp)
		{
			return $"a repeat of {last.Name} ({hardest:P0} of max HP) would kill you at {(float)standing / maxHp:P0}";
		}

		return string.Empty;
	}

	private static bool CarriesVulnerability(IBattleChara member)
	{
		var statuses = member.StatusList;
		if (statuses == null)
		{
			return false;
		}

		foreach (var status in statuses)
		{
			if (status != null && status.StatusId != 0 && IsVulnerability(status.StatusId))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// The game's "damage taken is increased" debuffs, recognised by the names the generator gives
	/// them from the status sheet (Vulnerability Up, Physical and Magic Vulnerability Up) rather than
	/// by a hand-kept id list. The names are the English sheet names, so the client language does not
	/// matter.
	/// </summary>
	private static bool IsVulnerability(uint statusId)
	{
		if (_vulnerabilityIds == null)
		{
			HashSet<uint> ids = [];
			foreach (var value in Enum.GetValues<StatusID>())
			{
				var name = value.ToString();
				if (name.StartsWith("VulnerabilityUp", StringComparison.Ordinal)
					|| name.StartsWith("PhysicalVulnerabilityUp", StringComparison.Ordinal)
					|| name.StartsWith("MagicVulnerabilityUp", StringComparison.Ordinal))
				{
					_ = ids.Add((uint)value);
				}
			}

			_vulnerabilityIds = ids;
		}

		return _vulnerabilityIds.Contains(statusId);
	}

	private static string Describe(List<uint> statuses)
	{
		var sheet = Service.GetSheet<Lumina.Excel.Sheets.Status>();
		var names = new List<string>(statuses.Count);
		foreach (var id in statuses)
		{
			names.Add(sheet.TryGetRow(id, out var row) ? $"{row.Name.ExtractText()} #{id}" : $"#{id}");
		}

		return string.Join(", ", names);
	}
}
