using ECommons.ExcelServices;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Enemy damage over time on the party: measured tick by tick, learned per area and status, and how low it alone takes
/// each member before a heal over time laid now would run out - the owner's proposal of 27.10.2026 (concept 08,
/// "Schaden über Zeit: HoT gegen DoT", A432).
/// </summary>
/// <remarks>
/// The game reports one aggregated tick per target for all its damage over time and one for all its heals over time,
/// and a ground effect's tick apart with its status: ActorControl 0x605 and 0x604, the amount in the second parameter,
/// the ground effect's status in the first (DeathRecap reads them so; cactbot LogGuide, line 24). The amount stands after
/// a barrier - a tick a barrier swallowed whole reads 0 (his log of 25.10.2026). Before the first tick of a status its
/// strength comes from earlier hits in the same area; a status that states no end of its own counts with the lifetime it
/// was seen to have there, and without one it is left to the heal thresholds. Guessing the rest from the ticks so far
/// was tried in the model and refuted (A431). This class answers what is coming; whether that calls for a heal -
/// threshold, exclusions, Esuna - is the decision's, in <see cref="CustomRotation"/>.
/// </remarks>
internal static class DotOnParty
{
	private readonly record struct Tick(DateTime At, uint Amount);

	// One instance of a status: the same status from two enemies is two. Late: first seen on a member the update before
	// did not watch - he joined, or the record was reset -, so when it began is unknown and its end teaches no lifetime.
	// Last: the last update that still saw it.
	private readonly record struct Instance(uint Member, uint Status, uint Source);

	private readonly record struct Seen(DateTime Since, DateTime Last, bool StatesNoEnd, uint Area, bool Late);

	/// <summary>How one member's damage ahead was weighed, for the trace of a decision.</summary>
	internal readonly record struct Weighed(float Tick, bool Measured, int Ticks, float Window, uint HotTick, float HotTicks, float Low);

	private static readonly Dictionary<uint, Tick> _dot = [];
	private static readonly Dictionary<(uint Member, uint Status), Tick> _hot = [];
	private static readonly Dictionary<Instance, Seen> _seen = [];
	private static readonly HashSet<Instance> _present = [];
	private static readonly HashSet<uint> _party = [];
	private static readonly HashSet<uint> _watched = [];
	private static readonly Dictionary<uint, Weighed> _weighed = [];
	private static readonly List<Instance> _gone = [];
	private static readonly List<(float Left, float Strength)> _parts = [];
	private static bool _unsaved;

	/// <summary>Whether any party member carries an enemy damage over time.</summary>
	internal static bool AnyDot => _seen.Count > 0;

	/// <summary>
	/// A damage-over-time tick on <paramref name="entityId"/> (ActorControl 0x605): the aggregated one with
	/// <paramref name="statusId"/> 0. A ground effect's tick, with its status, is left out: it lasts as long as he stands
	/// in it, not as long as a status runs.
	/// </summary>
	internal static void RecordDotTick(uint entityId, uint statusId, uint amount)
	{
		if (statusId != 0 || !_party.Contains(entityId))
		{
			return;
		}

		_dot[entityId] = new Tick(DateTime.Now, amount);

		// Only a tick one instance of one status dealt alone can teach that status, and one a barrier swallowed whole
		// teaches nothing.
		if (amount == 0 || !TryOnlyInstance(entityId, out var instance) || !_seen.TryGetValue(instance, out var seen))
		{
			return;
		}

		lock (OtherConfiguration.DotStoreGate)
		{
			Reading(seen.Area, instance.Status).AddTick(amount);
		}

		// Written when a status goes, not on every tick of every member.
		_unsaved = true;
	}

	/// <summary>
	/// A heal-over-time tick on <paramref name="entityId"/> (ActorControl 0x604): the aggregated one with
	/// <paramref name="statusId"/> 0, a ground effect's with its status.
	/// </summary>
	internal static void RecordHotTick(uint entityId, uint statusId, uint amount)
	{
		if (_party.Contains(entityId))
		{
			_hot[(entityId, statusId)] = new Tick(DateTime.Now, amount);
		}
	}

	/// <summary>
	/// Follows the enemy damage over time on every party member, once per update after the party list: an instance seen
	/// for the first time is noted, and one that stated no end of its own teaches its lifetime when it goes - from its
	/// start to the last update that saw it - unless it was first seen late, its bearer died, the area changed or the
	/// fight ended, which cut it short or hid its start.
	/// </summary>
	internal static void Update()
	{
		var now = DateTime.Now;
		var area = DataCenter.TerritoryID;
		_party.Clear();
		_present.Clear();
		foreach (var member in DataCenter.PartyMembers)
		{
			_ = _party.Add(member.EntityId);
		}

		foreach (var member in DataCenter.PartyMembers)
		{
			if (!member.IsAlive() || member.StatusList == null)
			{
				continue;
			}

			foreach (var status in member.StatusList)
			{
				if (!IsEnemyDot(status))
				{
					continue;
				}

				var key = new Instance(member.EntityId, status.StatusId, status.SourceId);
				_ = _present.Add(key);
				_seen[key] = _seen.TryGetValue(key, out var seen)
					? seen with { Last = now }
					: new Seen(now, now, status.RemainingTime <= 0f, area, !_watched.Contains(member.EntityId));
			}
		}

		_watched.Clear();
		_watched.UnionWith(_party);

		_gone.Clear();
		foreach (var key in _seen.Keys)
		{
			if (!_present.Contains(key))
			{
				_gone.Add(key);
			}
		}

		foreach (var key in _gone)
		{
			var seen = _seen[key];
			_ = _seen.Remove(key);
			if (!seen.StatesNoEnd || seen.Late || seen.Area != area || !DataCenter.InCombat || !BearerAlive(key.Member))
			{
				continue;
			}

			lock (OtherConfiguration.DotStoreGate)
			{
				Reading(area, key.Status).AddEnd((float)(seen.Last - seen.Since).TotalSeconds);
			}

			_unsaved = true;
		}

		if (_unsaved && _gone.Count > 0)
		{
			_unsaved = false;
			_ = OtherConfiguration.SaveDotOnParty();
		}

		Forget(_dot);
		Forget(_weighed);
		List<(uint Member, uint Status)>? staleHot = null;
		foreach (var key in _hot.Keys)
		{
			if (!_party.Contains(key.Member))
			{
				(staleHot ??= []).Add(key);
			}
		}

		if (staleHot != null)
		{
			foreach (var key in staleHot)
			{
				_ = _hot.Remove(key);
			}
		}
	}

	/// <summary>
	/// Every living party member whose enemy damage over time is known ahead, with the lowest share of his maximum HP
	/// it alone takes him to within <paramref name="hotSeconds"/> - the time of a heal over time laid now -, the heals
	/// over time already on him counted in; lowest first. A member under a heal over time that has not ticked yet is
	/// left out until its first tick tells its size.
	/// </summary>
	internal static List<(IBattleChara Member, float Low)> Project(float hotSeconds)
	{
		List<(IBattleChara Member, float Low)> found = [];
		if (hotSeconds <= 0f || !AnyDot)
		{
			return found;
		}

		var area = DataCenter.TerritoryID;
		var now = DateTime.Now;
		foreach (var member in DataCenter.PartyMembers)
		{
			if (!member.IsAlive() || member.MaxHp == 0 || member.StatusList == null
				|| !TryDamageAhead(member, area, now, out var tick, out var left, out var measured, out var learnedAll))
			{
				continue;
			}

			var window = Math.Min(hotSeconds, left);
			var dotTicks = (int)Math.Ceiling(member.DotTicksBetween(0f, window));

			// The heals over time on him, from any source, at the size of what ticked within the last tick's time - for
			// as long as all of them run: once the first ends, that size is no longer what comes.
			var hotLeft = float.MaxValue;
			var anyHot = false;
			foreach (var status in member.StatusList)
			{
				if (status.StatusId != 0 && StatusHelper.IsHealOverTime((StatusID)status.StatusId))
				{
					anyHot = true;
					hotLeft = Math.Min(hotLeft, status.RemainingTime > 0f ? status.RemainingTime : window);
				}
			}

			var hotTick = 0u;
			if (anyHot)
			{
				// One that has not ticked within one tick's time was laid just now: its size is not known yet, so he
				// waits for its first tick instead of drawing a second one on top.
				hotTick = HotNow(member.EntityId, now);
				if (hotTick == 0)
				{
					continue;
				}
			}

			var hotTicks = anyHot ? member.DotTicksBetween(0f, Math.Min(window, hotLeft)) : 0f;

			// Each tick ahead loses what the instances still running at it deal. One that ends earlier stops counting
			// from then, at its share of the tick: its learned strength, in a measured tick scaled to the tick, since
			// the learned figures are the highest seen; where not every instance has one, nothing is taken off - the
			// safe side.
			var scale = learnedAll > 0f ? (measured ? tick / learnedAll : 1f) : 0f;
			float top = member.MaxHp;
			var health = (float)member.CurrentHp;
			var low = health;
			for (var k = 1; k <= dotTicks; k++)
			{
				var loss = tick;
				foreach (var (partLeft, strength) in _parts)
				{
					if (partLeft < window && member.DotTicksBetween(0f, partLeft) < k)
					{
						loss -= strength * scale;
					}
				}

				health -= Math.Max(0f, loss);
				if (k <= hotTicks)
				{
					health = Math.Min(top, health + hotTick);
				}

				low = Math.Min(low, health);
			}

			var lowRatio = low / top;
			found.Add((member, lowRatio));
			_weighed[member.EntityId] = new Weighed(tick, measured, dotTicks, window, hotTick, hotTicks, lowRatio);
		}

		found.Sort(static (a, b) => a.Low.CompareTo(b.Low));
		return found;
	}

	/// <summary>How <paramref name="member"/>'s damage ahead was weighed last, for the trace of a decision.</summary>
	internal static bool TryWeighed(IBattleChara member, out Weighed weighed)
	{
		return _weighed.TryGetValue(member.EntityId, out weighed);
	}

	/// <summary>The job of <paramref name="member"/>, for a trace line in place of his name.</summary>
	internal static string JobOf(IBattleChara member)
	{
		return ((Job)member.ClassJob.RowId).ToString();
	}

	/// <summary>Whether every enemy damage over time on <paramref name="member"/> is one Esuna clears.</summary>
	internal static bool OnlyDispellable(IBattleChara member)
	{
		if (member.StatusList == null)
		{
			return false;
		}

		foreach (var status in member.StatusList)
		{
			if (IsEnemyDot(status) && !status.CanDispel())
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>Forgets everything followed - on leaving an area or a fight, where nothing of it carries over.</summary>
	internal static void Clear()
	{
		_dot.Clear();
		_hot.Clear();
		_seen.Clear();
		_present.Clear();
		_party.Clear();
		_watched.Clear();
		_weighed.Clear();
	}

	// The tick he takes, how long it still runs, and each instance's time left with its learned strength (_parts). The
	// tick: the last one measured, once it came after his damage over time began, plus the strength learned in this
	// area for an instance put on since - one with none learned counts from its first tick -; the learned strengths of
	// all his instances before any measured tick, and in place of a tick a barrier swallowed whole. The end: the latest
	// of his instances, from their own time or, where they state none, from the lifetime learned here. learnedAll is the
	// sum of all learned strengths, or -1 where one has none. False where tick or end is unknown.
	private static bool TryDamageAhead(IBattleChara member, uint area, DateTime now, out float tick, out float left, out bool measured,
		out float learnedAll)
	{
		tick = 0f;
		left = 0f;
		measured = false;
		learnedAll = 0f;
		_parts.Clear();
		var hasLast = _dot.TryGetValue(member.EntityId, out var last);
		var earliest = DateTime.MaxValue;
		var learnedNewer = 0f;
		var any = false;
		foreach (var status in member.StatusList)
		{
			if (!_seen.TryGetValue(new Instance(member.EntityId, status.StatusId, status.SourceId), out var seen))
			{
				continue;
			}

			any = true;
			earliest = seen.Since < earliest ? seen.Since : earliest;
			var (ticks, strength, ends, lifetime) = Learned(area, status.StatusId);
			float statusLeft;
			if (status.RemainingTime > 0f)
			{
				statusLeft = status.RemainingTime;
			}
			else if (ends > 0)
			{
				statusLeft = Math.Max(0f, lifetime - (float)(now - seen.Since).TotalSeconds);
			}
			else
			{
				return false;
			}

			left = Math.Max(left, statusLeft);
			_parts.Add((statusLeft, ticks > 0 ? strength : 0f));
			learnedAll = ticks > 0 && learnedAll >= 0f ? learnedAll + strength : -1f;
			if (hasLast && seen.Since > last.At && ticks > 0)
			{
				learnedNewer += strength;
			}
		}

		if (!any || left <= 0f)
		{
			return false;
		}

		if (hasLast && last.At >= earliest && (last.Amount > 0 || learnedAll <= 0f))
		{
			tick = last.Amount + learnedNewer;
			measured = true;
		}
		else
		{
			tick = learnedAll;
		}

		return tick > 0f;
	}

	private static (int Ticks, float Strength, int Ends, float Lifetime) Learned(uint area, uint status)
	{
		lock (OtherConfiguration.DotStoreGate)
		{
			return OtherConfiguration.DotOnParty.TryGetValue(area, out var known)
				&& known.Statuses.TryGetValue(status, out var reading)
				? (reading.Ticks, reading.TickAmount, reading.Ends, reading.Lifetime)
				: (0, 0f, 0, 0f);
		}
	}

	// What ticked on him within the last tick's time: the aggregated tick and each ground effect's; 0 where none did.
	private static uint HotNow(uint entityId, DateTime now)
	{
		var sum = 0u;
		foreach (var (key, tick) in _hot)
		{
			if (key.Member == entityId && (now - tick.At).TotalMilliseconds <= DotTickTracker.TickMs)
			{
				sum += tick.Amount;
			}
		}

		return sum;
	}

	// An enemy's damage over time: the status says so in its own text, and nobody of the party put it on.
	private static bool IsEnemyDot(Dalamud.Game.ClientState.Statuses.IStatus status)
	{
		return status.StatusId != 0 && !_party.Contains(status.SourceId)
			&& StatusHelper.IsDamageOverTime([(StatusID)status.StatusId]);
	}

	// The one enemy damage over time instance on him, false where there are none or more.
	private static bool TryOnlyInstance(uint entityId, out Instance instance)
	{
		instance = default;
		var count = 0;
		foreach (var member in DataCenter.PartyMembers)
		{
			if (member.EntityId != entityId || member.StatusList == null)
			{
				continue;
			}

			foreach (var status in member.StatusList)
			{
				if (IsEnemyDot(status))
				{
					count++;
					instance = new Instance(entityId, status.StatusId, status.SourceId);
				}
			}
		}

		return count == 1;
	}

	private static bool BearerAlive(uint entityId)
	{
		foreach (var member in DataCenter.PartyMembers)
		{
			if (member.EntityId == entityId)
			{
				return member.IsAlive();
			}
		}

		return false;
	}

	// Called under the store's gate.
	private static DotReading Reading(uint area, uint status)
	{
		if (!OtherConfiguration.DotOnParty.TryGetValue(area, out var known))
		{
			OtherConfiguration.DotOnParty[area] = known = new DotArea();
		}

		if (!known.Statuses.TryGetValue(status, out var reading))
		{
			known.Statuses[status] = reading = new DotReading();
		}

		return reading;
	}

	private static void Forget<T>(Dictionary<uint, T> byMember)
	{
		List<uint>? stale = null;
		foreach (var id in byMember.Keys)
		{
			if (!_party.Contains(id))
			{
				(stale ??= []).Add(id);
			}
		}

		if (stale != null)
		{
			foreach (var id in stale)
			{
				_ = byMember.Remove(id);
			}
		}
	}
}
