using ECommons.DalamudServices;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// The learned tankbuster table (<see cref="OtherConfiguration.TankbusterPotential"/>): what a tankbuster
/// does unmitigated - all hits of one cast on one tank together, and over how long (A454) -, measured on
/// every tank it is seen to hit, and what it is expected to do to a given target now. The owner's request of 04.10.2026 (concept 13, "Die Tankbuster-Tabelle").
/// </summary>
/// <remarks>
/// <para>Whether a tankbuster kills cannot be known ahead of the hit from any public source (A258);
/// it can be learned. A hit is scaled back to unmitigated by the mitigation that stood at that moment
/// - on the one hit, and on the attacker by damage type - with the figures the actions' own effect
/// texts state, or a trait where the text leaves the figure blank (Troubadour), given only to a status
/// whose own text says it reduces damage (<see cref="DefensiveValues.TryStatus"/>). A well-mitigated hit
/// therefore does not rate the action low, which is the weakness of the area table's raw observation.</para>
///
/// <para>What is left out errs on the low side, never on the high one: a mitigation no text rates
/// (a status not tied to an action, a trait figure below the trait's level) is not divided out, so the
/// unmitigated figure is at most underestimated, and every later hit can only raise it; and a status
/// that only shares an action's name (its heal over time) carries no figure, so nothing is divided out
/// twice. A barrier needs no dividing out: the game's damage figure carries the part it absorbed (his ACT
/// logs of 26.10.2026, A449). An underestimate means a defensive that does not fire - the behaviour before
/// the table.</para>
///
/// <para>Detection only: nothing here decides. Readers are the tank swap
/// (<see cref="TankSwapWatch"/>) for "would a repeat kill", <see cref="TankbusterForecast"/> for
/// "what does the coming hit take", and <see cref="DataCenter.AimedTankbusterLeaves"/> for the same question about
/// another member, whom help from others is for (A373).</para>
/// </remarks>
internal static class TankbusterTable
{
	/// <summary>
	/// Whether <paramref name="actionId"/> is on the area list: it has hit every party member at least once, so it is a
	/// raidwide, whatever the table holds for it. Until A337 a raidwide landing on a marked tank while the marker stood
	/// was stored here as the marker's tankbuster (Feldgeschrei, Wehklagen); the file keeps such entries, since a save
	/// merges with it, and every reader passes them over through this.
	/// </summary>
	internal static bool IsPartyWide(uint actionId) => OtherConfiguration.HostileCastingArea.Contains(actionId);

	// A cast followed so its hits can be told as its landing (A454): who cast what at whom - a player, or none -, its name
	// and attack type, when its bar ends, the time a further hit may follow the previous one, and how the plan takes it
	// now. A new cast of the same action by the same caster - its elapsed time went back - is a new instance; the old one
	// keeps its hits until its window has passed.
	private sealed class FollowedCast
	{
		public required ulong Caster { get; init; }
		public required uint Action { get; init; }
		public required string Name { get; init; }
		public required uint AttackType { get; init; }
		public required ulong TargetId { get; init; }
		public required float Window { get; init; }
		public DateTime EndsAt { get; set; }
		public float Elapsed { get; set; }
		public DateTime LastHit { get; set; }

		// Listed or measured: the plan takes it for a tankbuster as it is.
		public required bool Tankbuster { get; init; }

		// Taken for one only by the fallback of DataCenter.IsHostileCastingTank: cast at its own target, a tank (A452).
		public required bool Aimed { get; init; }

		public bool Open(DateTime now) => now <= (LastHit > EndsAt ? LastHit : EndsAt).AddSeconds(Window);
	}

	// The hits one cast lands on one tank in a row (A454): the id the reading is kept under, the name a further hit carries,
	// the cast or the first hit's source it continues from, until when, their unmitigated sum, whether the first came under
	// a vulnerability the tank carried, whether every hit so far came only through a cast at its own target, and why it may
	// not be stored - a hit that made it unfit (Spoiled) or one that did no damage (Incomplete). Stored when it ends.
	private sealed class Series
	{
		public required ulong TargetId { get; init; }
		public required string TargetName { get; init; }
		public required uint Key { get; init; }
		public required string Name { get; init; }
		public required FollowedCast? Cast { get; init; }
		public required ulong SourceId { get; init; }
		public required float Window { get; init; }
		public required bool Vulnerable { get; init; }
		public required DateTime First { get; init; }
		public DateTime Last { get; set; }
		public float Sum { get; set; }
		public int Hits { get; set; }
		public bool AimedOnly { get; set; }
		public string? Spoiled { get; set; }
		public bool Incomplete { get; set; }

		public bool Open(DateTime now) => now <= Last.AddSeconds(Window);
	}

	// Which mitigation the forecast lays on against an attack type (MitigationFactor), and how well a followed cast
	// explains a hit: its own id before its name, the hitter's own cast before another's.
	private enum DamageKind
	{
		Other,
		Physical,
		Magical,
	}

	private enum Match
	{
		OwnId,
		OwnName,
		OtherId,
		OtherName,
		None,
	}

	private static readonly List<FollowedCast> _casts = [];
	private static readonly List<Series> _series = [];
	private static readonly Dictionary<uint, string> _names = [];

	/// <summary>
	/// Follows every cast of the enemies the cast recognition reads (<see cref="DataCenter.AreaCasters"/>) but a raidwide's,
	/// so a hit can be told as the landing of its cast - under the cast's own id, or under another row of its name, from
	/// the caster or from a helper (concept 13, "Ein Tankbuster ist der Cast samt seiner Treffer", A454). Notes how the
	/// plan takes each: listed or measured, or only by the fallback of <see cref="DataCenter.IsHostileCastingTank"/> - a
	/// targetable enemy, not a helper, casting at its own target, a tank, uninterruptibly and for longer than a GCD. Neither
	/// list nor marker names those, so their hits went unmeasured, and plan and invulnerability never took a lethal one: it
	/// killed at every repeat (his first test case of 29.10.2026, A452). Stores the series that have ended. With every
	/// target update, for every role - the table learns from every tank it sees hit; with RSR off and no auto-on, the
	/// targets are not updated, nothing is followed, and a series is stored when the next hit comes in.
	/// </summary>
	internal static void UpdateCasts()
	{
		var casters = DataCenter.AreaCasters;
		if (!DataCenter.InCombat || casters == null)
		{
			ClearCasts();
			return;
		}

		var now = DateTime.Now;
		var gcd = DataCenter.DefaultGCDTotal;
		for (var i = 0; i < casters.Count; i++)
		{
			var h = casters[i];
			if (h == null || !h.IsCasting || IsPartyWide(h.CastActionId))
			{
				continue;
			}

			var cast = LatestCast(h.GameObjectId, h.CastActionId);
			if (cast == null || h.CurrentCastTime < cast.Elapsed)
			{
				// How the plan takes it, read once for the instance: its aim is fixed when the cast begins.
				var row = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(h.CastActionId);
				var aimedAt = Svc.Objects.SearchById(h.CastTargetObjectId) as Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter;
				var tankbuster = OtherConfiguration.HostileCastingTank.Contains(h.CastActionId) || TankbusterForecast.IsLearned(h.CastActionId);
				cast = new FollowedCast
				{
					Caster = h.GameObjectId,
					Action = h.CastActionId,
					Name = row is { } known ? NameOf(known) : string.Empty,
					AttackType = row?.AttackType.RowId ?? 0,
					TargetId = aimedAt?.GameObjectId ?? 0,
					Window = Math.Max(row?.CastSeconds() ?? 0f, gcd),
					Tankbuster = tankbuster,
					Aimed = !tankbuster && aimedAt != null && aimedAt.IsJobCategory(JobRole.Tank)
						&& !DataCenter.AreaCastHelperIds.Contains(h.GameObjectId) && DataCenter.IsHostileCastingTank(h),
				};
				_casts.Add(cast);
			}

			cast.EndsAt = now.AddSeconds(Math.Max(0f, h.TotalCastTime - h.CurrentCastTime));
			cast.Elapsed = h.CurrentCastTime;
		}

		Expire(now);
	}

	/// <summary>
	/// Stores the series still open - out of combat they are complete - and forgets the casts followed by
	/// <see cref="UpdateCasts"/>, with the rest of the target lists.
	/// </summary>
	internal static void ClearCasts()
	{
		foreach (var series in _series)
		{
			Store(series);
		}

		_series.Clear();
		_casts.Clear();
	}

	// The newest instance of a caster's cast of an action.
	private static FollowedCast? LatestCast(ulong caster, uint action)
	{
		for (var i = _casts.Count - 1; i >= 0; i--)
		{
			if (_casts[i].Caster == caster && _casts[i].Action == action)
			{
				return _casts[i];
			}
		}

		return null;
	}

	// Casts whose window has passed explain no further hit; a series whose window has passed is complete and stored.
	private static void Expire(DateTime now)
	{
		for (var i = _series.Count - 1; i >= 0; i--)
		{
			if (!_series[i].Open(now))
			{
				Store(_series[i]);
				_series.RemoveAt(i);
			}
		}

		for (var i = _casts.Count - 1; i >= 0; i--)
		{
			if (!_casts[i].Open(now))
			{
				_casts.RemoveAt(i);
			}
		}
	}

	// An action's name, read once: every hit on a player compares it.
	private static string NameOf(Lumina.Excel.Sheets.Action action)
	{
		if (!_names.TryGetValue(action.RowId, out var name))
		{
			name = action.Name.ExtractText();
			_names[action.RowId] = name;
		}

		return name;
	}

	private static DamageKind KindOf(uint attackType)
	{
		return attackType is >= (uint)AttackType.Slashing and <= (uint)AttackType.Shot ? DamageKind.Physical
			: attackType == (uint)AttackType.Magic ? DamageKind.Magical
			: DamageKind.Other;
	}

	// The followed cast a hit belongs to (concept 13, A454): its own id, else its name - a boss's visual cast and its hits
	// share it -, aimed at the struck player or at none, whose window is open; the caster's own before another's, which
	// counts only for a hit of an untargetable helper. Of several, the one whose bar ended last before the hit, else a
	// running one. A cast of a definite damage kind other than the hit's does not count: the forecast lays the mitigation
	// on by the cast's kind, and one without a kind takes the smaller. For a hit list or marker names that no cast of its
	// id or name explains (Void Meteor's comets, before the meteor), the open cast of a tankbuster on that player, of the
	// same kind or none.
	private static FollowedCast? CastOf(IBattleChara source, ulong targetId, Lumina.Excel.Sheets.Action hit, bool listedOrMarked,
		DateTime now)
	{
		FollowedCast? best = null;
		var bestMatch = Match.None;
		FollowedCast? tankbuster = null;
		var hitKind = KindOf(hit.AttackType.RowId);
		foreach (var cast in _casts)
		{
			var own = cast.Caster == source.GameObjectId;
			if ((cast.TargetId != 0 && cast.TargetId != targetId) || (!own && source.IsTargetable) || !cast.Open(now))
			{
				continue;
			}

			var match = cast.Action == hit.RowId ? own ? Match.OwnId : Match.OtherId
				: cast.Name.Length > 0 && cast.Name == NameOf(hit)
					&& (KindOf(cast.AttackType) == DamageKind.Other || KindOf(cast.AttackType) == hitKind)
					? own ? Match.OwnName : Match.OtherName
				: Match.None;
			if (match != Match.None && (best == null || match < bestMatch || (match == bestMatch && Later(cast, best, now))))
			{
				best = cast;
				bestMatch = match;
			}

			if (listedOrMarked && cast.Tankbuster && (KindOf(cast.AttackType) == DamageKind.Other || KindOf(cast.AttackType) == hitKind)
				&& (tankbuster == null || Later(cast, tankbuster, now)))
			{
				tankbuster = cast;
			}
		}

		return best ?? tankbuster;
	}

	// Whether cast a ended its bar later before the hit than b; one whose bar has ended before one still running.
	private static bool Later(FollowedCast a, FollowedCast b, DateTime now)
	{
		var aEnded = a.EndsAt <= now;
		var bEnded = b.EndsAt <= now;
		return aEnded != bEnded ? aEnded : aEnded ? a.EndsAt > b.EndsAt : a.EndsAt < b.EndsAt;
	}

	// The series a hit continues on the struck tank: its cast's, while open; without a cast, an open one of the hit's id
	// or name from the same source or a helper, or for a hit list or marker names, the open series of a tankbuster cast.
	private static Series? OpenSeries(ulong targetId, FollowedCast? cast, Lumina.Excel.Sheets.Action hit, IBattleChara source,
		bool listedOrMarked, DateTime now)
	{
		Series? ofTankbuster = null;
		foreach (var series in _series)
		{
			if (series.TargetId != targetId || !series.Open(now))
			{
				continue;
			}

			if (cast != null)
			{
				if (series.Cast == cast)
				{
					return series;
				}

				continue;
			}

			if ((series.Key == hit.RowId || series.Name == NameOf(hit))
				&& (series.SourceId == source.GameObjectId || !source.IsTargetable))
			{
				return series;
			}

			if (listedOrMarked && series.Cast is { Tankbuster: true })
			{
				ofTankbuster ??= series;
			}
		}

		return ofTankbuster;
	}

	/// <summary>
	/// Whether a hit from <paramref name="source"/> on <paramref name="targetId"/> can count as a tankbuster: list or marker
	/// name it, a followed cast explains it, or a series is open on that target. For the effect handler, before it looks
	/// the target up: every hit of the enemy side on a player passes here.
	/// </summary>
	internal static bool MayCount(IBattleChara source, ulong targetId, Lumina.Excel.Sheets.Action hit, bool listedOrMarked)
	{
		if (listedOrMarked)
		{
			return true;
		}

		var now = DateTime.Now;
		var cast = CastOf(source, targetId, hit, false, now);
		_checked = (source.GameObjectId, targetId, hit.RowId, now, cast);
		if (cast != null)
		{
			return true;
		}

		foreach (var series in _series)
		{
			if (series.TargetId == targetId && series.Open(now))
			{
				return true;
			}
		}

		return false;
	}

	// What MayCount found for the hit it was last asked about, so Record does not search again.
	private static (ulong Source, ulong Target, uint Hit, DateTime At, FollowedCast? Cast) _checked;

	/// <summary>
	/// A hit from <paramref name="source"/> reached <paramref name="target"/>, a player, for <paramref name="share"/> of
	/// the target's maximum HP; on the game thread, from the effect handler, for a hit of the enemy side but a raidwide's
	/// that <see cref="MayCount"/> let through, once per target of an effect set. It counts as a tankbuster when list or
	/// marker name it (<paramref name="listedOrMarked"/>), when it lands from a cast the plan takes for one - listed,
	/// measured, or cast at its own target, a tank, with <paramref name="reached"/> one target -, or when it continues an
	/// open series on the target. What one cast's hits take on one tank is added up and stored when the series ends, under
	/// the id the plan reads, the cast's, with the time from the first hit to the last (concept 13, "Ein Tankbuster ist der
	/// Cast samt seiner Treffer", A454). <paramref name="appliedByHit"/>: the statuses this very hit put on the target - a
	/// vulnerability among them, or one an earlier hit of the series laid, came with the tankbuster and did not raise it,
	/// so it does not count as one the target carried (whether the game has already applied it when the effect arrives is
	/// not established).
	/// </summary>
	/// <returns>The id the hit counts under, zero when it is no tankbuster.</returns>
	public static uint Record(Lumina.Excel.Sheets.Action hit, IBattleChara target, IBattleChara source, float share,
		List<uint> appliedByHit, bool listedOrMarked, int reached)
	{
		// MayCount was asked about this very hit just before, unless list or marker name it.
		var known = !listedOrMarked && _checked.Source == source.GameObjectId && _checked.Target == target.GameObjectId
			&& _checked.Hit == hit.RowId;
		var now = known ? _checked.At : DateTime.Now;
		var cast = known ? _checked.Cast : CastOf(source, target.GameObjectId, hit, listedOrMarked, now);
		_checked = default;
		Expire(now);
		var name = NameOf(hit);
		var series = OpenSeries(target.GameObjectId, cast, hit, source, listedOrMarked, now);

		// A tankbuster reaches one target; one aimed only at its own target that reaches more is a cleave or a raidwide aimed
		// at the tank - counted over every target, other alliance groups' too - which the area list does not hold where its
		// intake is off or the party is small (A452). It spoils the series it would continue.
		var aimed = !listedOrMarked && cast is { Tankbuster: false, Aimed: true };
		if (aimed && reached > 1)
		{
			if (series != null)
			{
				series.Spoiled ??= "a hit reached more than one target";
			}

			return 0u;
		}

		if (!listedOrMarked && cast is not { Tankbuster: true } && !aimed && series == null)
		{
			return 0u;
		}

		var key = series?.Key ?? cast?.Action ?? hit.RowId;

		// Every reader rates the hit for a tank - the player's plan and the tank swap. A member who is not a tank lacks the
		// tank trait's reduction, which no status carries, and has a smaller pool, so his share would raise the figure for
		// every tank, and a failed mechanic on him could take it past his whole health (A312, as A298 for the area table).
		// Not stored; the line stays for the trace.
		if (!target.IsJobCategory(JobRole.Tank))
		{
			if (share > 0f)
			{
				DefenseTrace.Line($"tankbuster measured: {name} #{key} on {target.Name.TextValue} (not a tank) for {share:P0}"
					+ " of max HP - not stored, the table rates hits on tanks");
			}

			return key;
		}

		if (series == null)
		{
			series = new Series
			{
				TargetId = target.GameObjectId,
				TargetName = target.Name.TextValue,
				Key = key,
				Name = cast is { Name.Length: > 0 } ? cast.Name : name,
				Cast = cast,
				SourceId = cast?.Caster ?? source.GameObjectId,
				Window = cast?.Window ?? Math.Max(hit.CastSeconds(), DataCenter.DefaultGCDTotal),
				Vulnerable = CarriedVulnerability(target, appliedByHit),
				First = now,
				AimedOnly = true,
			};
			_series.Add(series);
		}

		// Aimed only while every hit came through a cast at its own target; one under list, marker or a known cast ends it.
		series.AimedOnly &= !listedOrMarked && cast is not { Tankbuster: true } && (aimed || series.Cast is { Aimed: true });
		series.Last = now;
		series.Hits++;
		if (cast != null)
		{
			cast.LastHit = now;
		}

		var via = series.AimedOnly ? ", known only as a cast at its own target" : string.Empty;
		var asKey = key != hit.RowId ? $" as #{key}" : string.Empty;

		// Nothing arrived: a hit turned away by invulnerability or evasion. Zero says that something stopped it, not that the
		// action is harmless; the series' sum would only be a lower bound, under which the plan holds back the rest of the
		// defence, so it is not stored. A barrier makes no zero: the figure carries what it absorbed (A449).
		if (share <= 0f)
		{
			series.Incomplete = true;
			return key;
		}

		// A strengthened attacker hits harder than the action does: stored, the figure would rate the action too high, and
		// the tankbuster plan would draw the invulnerability, or stack mitigation, where less would have done. The whole
		// series is unfit - the one error the table must not make (concept 09, "Gemessener Tankbuster: das geringste Mittel").
		if (source.CarriesDamageUp())
		{
			series.Spoiled ??= "the attacker carried a damage-up status";
			DefenseTrace.Line($"tankbuster measured: {name} #{hit.RowId}{asKey} on {target.Name.TextValue} for {share:P0} of max HP{via}"
				+ " - not stored, the attacker carried a damage-up status");
			return key;
		}

		// Above the level the content syncs to, the hit is a smaller share than in sync: stored, it would rate the action
		// too small for the next synced run, where the plan reads it to spend less (A452).
		if (DataCenter.AboveContentLevel)
		{
			series.Spoiled ??= "above the level the content syncs to";
			DefenseTrace.Line($"tankbuster measured: {name} #{hit.RowId}{asKey} on {target.Name.TextValue} for {share:P0} of max HP{via}"
				+ " - not stored, above the level the content syncs to");
			return key;
		}

		var factor = MitigationFactor(hit.RowId, target, source);
		if (factor <= 0f)
		{
			series.Incomplete = true;
			return key;
		}

		// A share of a raised maximum (Thrill of Battle, Great Nebula, Protraction) is a share of the larger pool; scaled
		// back to the unraised maximum, as every plan reads it (concept 13). Each hit is scaled back by what stood at it.
		var unmitigated = share * (1f + MaxHpRaise(target)) / factor;
		series.Sum += unmitigated;
		DefenseTrace.Line($"tankbuster measured: {name} #{hit.RowId}{asKey} on {target.Name.TextValue} for {share:P0} of max HP{via},"
			+ $" mitigation factor {factor:F2}, unmitigated {unmitigated:P0}"
			+ (series.Hits > 1 ? $", hit {series.Hits} of its series, together {series.Sum:P0} over {(series.Last - series.First).TotalSeconds:F1} s" : string.Empty)
			+ (series.Vulnerable ? " under a vulnerability" : string.Empty));
		return key;
	}

	// A series that has ended: its sum and span into the table, as highest values, when nothing made it unfit (concept 13,
	// A454). Stored once, when it ends: until then a later hit can still spoil it, and the table only rises.
	private static void Store(Series series)
	{
		var span = (float)(series.Last - series.First).TotalSeconds;
		var what = $"tankbuster series: {series.Name} #{series.Key} on {series.TargetName}, {series.Hits} hit(s) over {span:F1} s,"
			+ $" together {series.Sum:P0} unmitigated{(series.Vulnerable ? " under a vulnerability" : string.Empty)}";
		if (series.Spoiled != null || series.Incomplete || series.Sum <= 0f)
		{
			if (series.Sum > 0f || series.Spoiled != null)
			{
				DefenseTrace.Line(what + " - not stored, " + (series.Spoiled ?? "a hit was turned away"));
			}

			return;
		}

		bool raised;
		bool longer;
		bool changed;
		lock (OtherConfiguration.TankbusterPotentialGate)
		{
			var known = OtherConfiguration.TankbusterPotential.TryGetValue(series.Key, out var reading);
			reading ??= new TankbusterReading();

			var aimedOnly = series.AimedOnly && (!known || reading.AimedOnly);
			var before = series.Vulnerable ? reading.UnderVulnerability : reading.Unmitigated;
			raised = series.Sum > before;
			longer = span > reading.Span;
			changed = raised || longer || (known && reading.AimedOnly != aimedOnly);
		}

		// Every stored series, not only one that raises the value: the revisions count it in its game version, and set the
		// table to the higher of what they hold - the value written here before (concept 13, "Stufe 1"). Outside the
		// readers' lock: the store takes it only to set the entry, and its save is prepared after that.
		OtherConfiguration.MeasureTankbuster(series.Key, new TankbusterReading
		{
			Unmitigated = series.Vulnerable ? 0f : series.Sum,
			UnderVulnerability = series.Vulnerable ? series.Sum : 0f,
			AimedOnly = series.AimedOnly,
			Span = span,
		}, !series.Vulnerable);

		DefenseTrace.Line(what + (series.AimedOnly ? ", known only as a cast at its own target" : string.Empty)
			+ (raised ? " - stored" : longer ? " - not above the stored value, its span stored"
				: changed ? " - not above the stored value, known as a tankbuster from now on" : " - not above the stored value"));
	}

	/// <summary>
	/// The longest time from the first to the last hit of <paramref name="actionId"/>'s series on a tank, in seconds; zero
	/// for one hit or an action not measured. What mitigates it has to stand this much longer than its first hit (concept
	/// 09, "H_S, der Horizont der Serie", A454).
	/// </summary>
	public static float SeriesSpan(uint actionId)
	{
		lock (OtherConfiguration.TankbusterPotentialGate)
		{
			return OtherConfiguration.TankbusterPotential.TryGetValue(actionId, out var reading) ? Math.Max(0f, reading.Span) : 0f;
		}
	}

	/// <summary>
	/// The share of <paramref name="target"/>'s maximum HP the tankbuster is expected to take: its
	/// stored unmitigated figure (the one under vulnerability when the target carries one and it is
	/// known) times the mitigation standing now and still standing in <paramref name="horizon"/>
	/// seconds - whoever put it there, the player, a co-tank's Reprisal, a healer - as a share of the
	/// maximum HP the target has now: a raise of the maximum that still stands then makes the same hit a
	/// smaller share. Zero when the action has not been measured.
	/// </summary>
	public static float PredictedShare(uint actionId, IBattleChara target, IBattleChara? attacker, float horizon = 0f)
	{
		if (IsPartyWide(actionId))
		{
			return 0f;
		}

		TankbusterReading? reading;
		lock (OtherConfiguration.TankbusterPotentialGate)
		{
			_ = OtherConfiguration.TankbusterPotential.TryGetValue(actionId, out reading);
		}

		if (reading == null)
		{
			return 0f;
		}

		var unmitigated = target.CarriesVulnerabilityUp() && reading.UnderVulnerability > 0f
			? reading.UnderVulnerability
			: reading.Unmitigated;
		return unmitigated * MitigationFactor(actionId, target, attacker, horizon) / (1f + MaxHpRaise(target, horizon));
	}

	/// <summary>
	/// By how much the target's maximum HP is raised (0.2 for +20 %) by statuses standing now and, with a
	/// <paramref name="horizon"/>, still standing then - read from the effect texts of the actions that
	/// put them there (<see cref="DefensiveValue.MaxHp"/>). Raises multiply.
	/// </summary>
	public static float MaxHpRaise(IBattleChara target, float horizon = 0f)
	{
		var raised = 1f;
		foreach (var status in StatusesOf(target, horizon))
		{
			if (DefensiveValues.TryStatus(status, out var value) && value.MaxHp > 0f)
			{
				raised *= 1f + value.MaxHp;
			}
		}

		return raised - 1f;
	}

	/// <summary>
	/// The share of a hit that gets through the mitigation standing now: on the target, and on the
	/// attacker by the action's damage type. Where the type is neither physical nor magical, the smaller
	/// of the attacker's two reductions counts - the reading errs low rather than high. With a
	/// <paramref name="horizon"/>, a status that runs out before it does not count.
	/// </summary>
	public static float MitigationFactor(uint actionId, IBattleChara target, IBattleChara? attacker, float horizon = 0f)
	{
		var attackType = Service.GetSheet<Lumina.Excel.Sheets.Action>().TryGetRow(actionId, out var row)
			? row.AttackType.RowId
			: 0u;

		// On the target: its own mitigation, together with a split one for this damage type (Dark Mind).
		var factor = 1f;
		foreach (var status in StatusesOf(target, horizon))
		{
			if (DefensiveValues.TryStatus(status, out var value))
			{
				var reduction = value.SelfAgainst(attackType);
				if (reduction > 0f)
				{
					factor *= 1f - reduction;
				}
			}
		}

		if (attacker != null)
		{
			foreach (var status in StatusesOf(attacker, horizon))
			{
				if (!DefensiveValues.TryStatus(status, out var value))
				{
					continue;
				}

				var reduction = attackType is >= (uint)AttackType.Slashing and <= (uint)AttackType.Shot
					? value.EnemyPhysical
					: attackType == (uint)AttackType.Magic
						? value.EnemyMagical
						: Math.Min(value.EnemyPhysical, value.EnemyMagical);
				if (reduction > 0f)
				{
					factor *= 1f - reduction;
				}
			}
		}

		return factor;
	}

	private static bool CarriedVulnerability(IBattleChara target, List<uint> appliedByHit)
	{
		foreach (var status in StatusesOf(target))
		{
			if (StatusHelper.IsVulnerabilityUp(status) && !appliedByHit.Contains(status))
			{
				return true;
			}
		}

		return false;
	}

	// A remaining time of zero or less is a status without an end; it stands at any horizon.
	private static List<uint> StatusesOf(IBattleChara battleChara, float horizon = 0f)
	{
		List<uint> ids = [];
		var statuses = battleChara.StatusList;
		if (statuses == null)
		{
			return ids;
		}

		foreach (var status in statuses)
		{
			if (status != null && status.StatusId != 0
				&& (horizon <= 0f || status.RemainingTime <= 0f || status.RemainingTime >= horizon))
			{
				ids.Add(status.StatusId);
			}
		}

		return ids;
	}
}
