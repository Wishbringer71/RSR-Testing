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

	private sealed class TakeOver
	{
		public required ulong Source { get; init; }
		public required string SourceName { get; init; }
		public required ulong Holder { get; init; }
		public required string HolderName { get; init; }
		public required string Reason { get; init; }
		public required bool ByGesture { get; init; }
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
	private static DateTime _heldWritten = DateTime.MinValue;

	// The other side of a swap (concept 09, "Tankwechsel von der anderen Seite"): the tankbuster each enemy last hit
	// somebody with, a co-tank's Shirk on the player while an enemy still attacks him, and the take-over chosen,
	// carried out and waiting for the enemy's next tankbuster.
	private static readonly Dictionary<ulong, uint> _lastActionBySource = [];
	private static ulong _shirkFrom;
	private static TakeOver? _takeOverNow;
	private static TakeOver? _takeOverPlanned;
	private static TakeOver? _takeOverOpen;
	// Enemies whose next tankbuster still hit the holder after a take-over: something other than the enmity fixed it.
	private static readonly HashSet<ulong> _takeOverIgnored = [];
	private static string _takeOverHeldWritten = string.Empty;

	/// <summary>
	/// The enemy to take from a co-tank in danger, or who handed it over with Shirk; 0 when none. Facts only - the
	/// setting is the consumer's (<c>ObjectHelper.CanProvoke</c>).
	/// </summary>
	public static ulong TakeOverSourceId { get; private set; }

	/// <summary>Why <see cref="TakeOverSourceId"/> is to be taken, for the trace.</summary>
	public static string TakeOverReason { get; private set; } = string.Empty;

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

			_lastActionBySource[source] = actionId;
			SettleTakeOver(source, Player.Object?.GameObjectId ?? 0, name);

			List<uint> vulnerabilities = [];
			foreach (var status in appliedStatuses)
			{
				if (StatusHelper.IsVulnerabilityUp(status))
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
	public static void RecordBusterOnOther(ulong source, ulong target, string name, uint actionId)
	{
		lock (_gate)
		{
			_lastActionBySource[source] = actionId;
			SettleTakeOver(source, target, name);

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
	/// A party member's Shirk reached the player. Handing the enemy back to him is decided by his measured
	/// danger, which covers the case without this signal. While an enemy still attacks him it is his request to
	/// take it (the owner's note of 17.10.2026: acting himself, he does not want the next tankbuster).
	/// </summary>
	public static void RecordShirkOnPlayer(ulong from, string name)
	{
		lock (_gate)
		{
			_shirkFrom = from;
		}

		DefenseTrace.Line($"{name} shirked to you");
	}

	/// <summary>The consumer pressed Provoke on <see cref="TakeOverSourceId"/>.</summary>
	public static void TakeOverChosen()
	{
		lock (_gate)
		{
			_takeOverPlanned = _takeOverNow;
		}
	}

	// The enemy's next tankbuster after a take-over shows whether it followed the enmity. One that still hit the
	// holder had its target fixed some other way; on an announcement that enemy is not taken again this fight. His
	// own Shirk still counts - it is his request, not a reading of the fight.
	private static void SettleTakeOver(ulong source, ulong struck, string name)
	{
		_shirkFrom = 0;
		if (_takeOverOpen is not { } open || open.Source != source)
		{
			return;
		}

		_takeOverOpen = null;
		if (struck == Player.Object?.GameObjectId)
		{
			DefenseTrace.Line($"take-over held: you took {name} instead of {open.HolderName}");
		}
		else if (struck == open.Holder)
		{
			if (!open.ByGesture)
			{
				_ = _takeOverIgnored.Add(source);
			}

			DefenseTrace.Line($"take-over did not move {name}: it still hit {open.HolderName}, its target was fixed some other way"
				+ (open.ByGesture ? string.Empty : $"; no further take-over from {open.SourceName} on an announcement this fight"));
		}
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
			TakeOverSourceId = 0;
			TakeOverReason = string.Empty;

			if (!DataCenter.InCombat)
			{
				_last = null;
				_planned = null;
				_pending = null;
				_swappedAway = null;
				_hardestShare.Clear();
				_failedRatio = 1f;
				_lastActionBySource.Clear();
				_shirkFrom = 0;
				_takeOverNow = _takeOverPlanned = _takeOverOpen = null;
				_takeOverIgnored.Clear();
				_takeOverHeldWritten = string.Empty;
				return;
			}

			var player = Player.Object;
			if (player == null || player.IsDead)
			{
				return;
			}

			// The same reading as below, needed before it for the take-over: nothing goes to a player in danger.
			if (_last != null && Svc.Objects.SearchById(_last.Source) is IBattleChara lastSource && !lastSource.IsDead
				&& lastSource.IsTargetable && !player.HasStatus(false, StatusHelper.NoNeedHealingStatus) && !player.HasStatus(false, StatusID.WalkingDead))
			{
				PlayerInDanger = DangerOf(player).Length > 0;
			}

			if (DataCenter.Role == JobRole.Tank)
			{
				UpdateTakeOver(player);
			}

			if (_last == null)
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

				if (member.CarriesVulnerabilityUp())
				{
					why = $"{member.Name.TextValue} carries a vulnerability too";
					continue;
				}

				// The owner's case of 04.10.2026: the other tank may be the one in danger - he may even
				// have shirked to you for it. Handing him the enemy back would trade one death for another.
				if (member.CurrentHp + member.GetObjectShield() <= ExpectedShare(member, source) * Math.Max(1u, member.MaxHp))
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

	/// <summary>
	/// The other side of a swap (concept 09, "Tankwechsel von der anderen Seite", the owner's decision of 17.10.2026):
	/// an enemy attacking another tank of the party is to be taken from him when he shirked to the player (D), or when
	/// BossModReborn announces the next tankbuster and he would take it still vulnerable or not survive it by his
	/// health trend (B). Only before the hit's target is fixed: a cast at him or a marker on him can no longer be
	/// turned away, so B holds then - his own request does not.
	/// </summary>
	private static void UpdateTakeOver(IBattleChara player)
	{
		_takeOverNow = null;

		// A request lasts while an enemy still attacks him; once it moved - to the player or anyone - it is done.
		if (_shirkFrom != 0 && (Svc.Objects.SearchById(_shirkFrom) is not IBattleChara shirker || shirker.IsDead
			|| !AnyAttacking(shirker)))
		{
			_shirkFrom = 0;
		}

		// A take-over carried out: open once the enemy attacks the player, gone with the enemy.
		if (_takeOverPlanned is { } planned)
		{
			if (Svc.Objects.SearchById(planned.Source) is not IBattleChara taken || taken.IsDead)
			{
				_takeOverPlanned = null;
			}
			else if (taken.TargetObjectId == player.GameObjectId)
			{
				_takeOverOpen = planned;
				_takeOverPlanned = null;
				DefenseTrace.Line($"took {planned.SourceName} over from {planned.HolderName} ({planned.Reason})");
			}
		}

		// Back on the holder before its next tankbuster - his stance outweighed the provoke, or he took it back: no
		// reading about its target, so it is not counted against the enemy; a danger left opens a new take-over.
		if (_takeOverOpen is { } open)
		{
			if (Svc.Objects.SearchById(open.Source) is not IBattleChara kept || kept.IsDead)
			{
				_takeOverOpen = null;
			}
			else if (kept.TargetObjectId == open.Holder)
			{
				_takeOverOpen = null;
				DefenseTrace.Line($"take-over undone: {open.SourceName} went back to {open.HolderName} before its next tankbuster");
			}
		}

		var hitIn = Service.Config.UseBmrTimeline && DataCenter.BMRNextTankbusterIn is > 0f and < float.MaxValue
			? DataCenter.BMRNextTankbusterIn
			: float.MaxValue;

		var anyHeld = false;
		foreach (var member in DataCenter.PartyMembers)
		{
			if (member == null || member.GameObjectId == player.GameObjectId || member.IsDead || !member.IsJobCategory(JobRole.Tank))
			{
				continue;
			}

			var source = AttackerOf(member, player, out var several);
			if (source == null && !several)
			{
				continue;
			}

			var asked = member.GameObjectId == _shirkFrom;
			var reason = asked ? $"{member.Name.TextValue} shirked to you" : DangerAt(member, source, hitIn);
			if (reason.Length == 0)
			{
				continue;
			}

			var held = TakeOverHeld(member, source, player, asked, hitIn, several);
			if (held.Length > 0)
			{
				anyHeld = true;
				// Once per holder and reason: the danger line carries times that change every frame.
				var key = $"{member.GameObjectId}:{held}";
				if (_takeOverHeldWritten != key)
				{
					_takeOverHeldWritten = key;
					DefenseTrace.Line($"take-over from {member.Name.TextValue} held: {reason}; {held}");
				}

				continue;
			}

			_takeOverNow = new TakeOver
			{
				Source = source!.GameObjectId,
				SourceName = source.Name.TextValue,
				Holder = member.GameObjectId,
				HolderName = member.Name.TextValue,
				Reason = reason,
				ByGesture = asked,
			};
			TakeOverSourceId = source.GameObjectId;
			TakeOverReason = reason;
			_takeOverHeldWritten = string.Empty;
			return;
		}

		// Nothing held this cycle: the next hold is written again.
		if (!anyHeld)
		{
			_takeOverHeldWritten = string.Empty;
		}
	}

	// The enemy attacking the holder: the player's own target when it is one of them, otherwise the only one. Several
	// without the player's target among them leave it open - BossModReborn names no caster for its announcement.
	private static IBattleChara? AttackerOf(IBattleChara holder, IBattleChara player, out bool several)
	{
		several = false;
		IBattleChara? only = null;
		var count = 0;
		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile == null || hostile.IsDead || hostile.TargetObjectId != holder.GameObjectId)
			{
				continue;
			}

			if (hostile.GameObjectId == player.TargetObjectId)
			{
				return hostile;
			}

			only = hostile;
			count++;
		}

		several = count > 1;
		return count == 1 ? only : null;
	}

	private static bool AnyAttacking(IBattleChara holder)
	{
		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile != null && !hostile.IsDead && hostile.TargetObjectId == holder.GameObjectId)
			{
				return true;
			}
		}

		return false;
	}

	// Why the holder is in danger at the announced tankbuster, or empty. Vulnerable when it lands; or, for the
	// tankbuster this enemy last hit with, his health at that moment by its measured trend (GetHealthRatioIn) with
	// his shield not above what the table expects it to take from him. A vulnerability with nothing announced is
	// returned too, so the trace can say why nothing happens.
	private static string DangerAt(IBattleChara holder, IBattleChara? source, float hitIn)
	{
		var vulnerable = holder.VulnerabilityUpRemaining();
		if (hitIn == float.MaxValue)
		{
			return vulnerable > 0f ? $"{holder.Name.TextValue} is vulnerable" : string.Empty;
		}

		if (vulnerable > hitIn)
		{
			return $"{holder.Name.TextValue} would take the tankbuster announced in {hitIn:F1} s still vulnerable";
		}

		if (source != null && _lastActionBySource.TryGetValue(source.GameObjectId, out var action))
		{
			var share = TankbusterTable.PredictedShare(action, holder, source);
			var maxHp = Math.Max(1u, holder.MaxHp);
			var standing = (holder.GetHealthRatioIn(hitIn) * maxHp) + holder.GetObjectShield();
			if (share > 0f && standing <= share * maxHp)
			{
				return $"{holder.Name.TextValue} would not survive a repeat of #{action} ({share:P0} of max HP) in {hitIn:F1} s"
					+ $" at {standing / maxHp:P0}";
			}
		}

		return string.Empty;
	}

	// Why the take-over does not go, or empty.
	private static string TakeOverHeld(IBattleChara holder, IBattleChara? source, IBattleChara player, bool asked, float hitIn, bool several)
	{
		if (source == null)
		{
			return several ? $"several enemies attack {holder.Name.TextValue}, and BossModReborn names no caster" : "no enemy attacks him";
		}

		if (asked ? !holder.CanBeHealed() : holder.ImperviousThrough(hitIn))
		{
			return $"{holder.Name.TextValue} is impervious through it";
		}

		// Both in danger: neither hands over, neither provokes - as on the player's side.
		if (PlayerInDanger)
		{
			return "you are in danger yourself";
		}

		if (asked ? player.CarriesVulnerabilityUp() : player.VulnerabilityUpRemaining() > hitIn)
		{
			return "you are vulnerable yourself";
		}

		if (asked)
		{
			return string.Empty;
		}

		if (hitIn == float.MaxValue)
		{
			return "no next tankbuster announced";
		}

		if (_lastActionBySource.TryGetValue(source.GameObjectId, out var action))
		{
			var share = TankbusterTable.PredictedShare(action, player, source);
			var maxHp = Math.Max(1u, player.MaxHp);
			if (share > 0f && (player.GetHealthRatioIn(hitIn) * maxHp) + player.GetObjectShield() <= share * maxHp)
			{
				return "you would not survive it yourself";
			}
		}

		if (DataCenter.BMRNextDamageType == PredictedDamageType.Tankbuster && DataCenter.BMRNextDamageIn is > 0f and < float.MaxValue
			&& DataCenter.BMRNextDamageHitsPlayer)
		{
			return "the announced tankbuster hits you anyway";
		}

		// Any cast aimed at him, from its start: its target is fixed until it resolves. The tankbuster recognition
		// (DataCenter.IsHostileCastingTank) answers only in the last GCD before the hit, too late to hold here - a
		// provoke during the cast would change nothing about it and its hit would count against the enemy below.
		if (source.IsCasting && source.CastTargetObjectId == holder.GameObjectId)
		{
			return $"{source.Name.TextValue} is already casting at him, and that cast's target is fixed";
		}

		if (DataCenter.IsTankbusterVfxOn(holder.GameObjectId))
		{
			return $"a tankbuster marker on {holder.Name.TextValue} fixes its target";
		}

		if (_takeOverIgnored.Contains(source.GameObjectId))
		{
			return $"{source.Name.TextValue}'s tankbuster still hit him after the last take-over";
		}

		return string.Empty;
	}

	private static float HardestShare()
	{
		var last = _last!;
		return _hardestShare.TryGetValue(last.ActionId, out var share) ? share : last.Share;
	}

	/// <summary>
	/// What a repeat of the last tankbuster is expected to take from <paramref name="target"/>: the higher
	/// of the hardest share measured on the player this fight and the learned table's prediction for this
	/// target under the mitigation standing now (<see cref="TankbusterTable"/>, carried across sessions
	/// and learned from every player hit).
	/// </summary>
	private static float ExpectedShare(IBattleChara target, IBattleChara? source)
	{
		return Math.Max(HardestShare(), TankbusterTable.PredictedShare(_last!.ActionId, target, source));
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

		var hardest = ExpectedShare(player, Svc.Objects.SearchById(last.Source) as IBattleChara);
		var maxHp = Math.Max(1u, player.MaxHp);
		var standing = player.CurrentHp + player.GetObjectShield();
		if (standing <= hardest * maxHp)
		{
			return $"a repeat of {last.Name} ({hardest:P0} of max HP) would kill you at {(float)standing / maxHp:P0}";
		}

		return string.Empty;
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
