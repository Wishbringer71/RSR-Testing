using Dalamud.Hooking;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Hooks;
using ECommons.Hooks.ActionEffectTypes;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using RotationSolver.ActionTimeline;
using RotationSolver.Basic.Configuration;
using System.Text.RegularExpressions;

namespace RotationSolver;

public static partial class Watcher
{
	// Stage 4a (concept 13): the marker locks that hold in the running game version; the rest are taken over.
	private static int MarkerLocksRunning()
	{
		var running = 0;
		foreach (var key in OtherConfiguration.TankbusterMarkerWithoutHit)
		{
			running += DataCenter.MarkerLockHolds(key, trace: false) ? 1 : 0;
		}

		return running;
	}

	// The commit this build was made from, so an uploaded trace can be matched to the code that wrote it:
	// the version is the same for every build on the branch, and a day carries several commits.
	// Embedded by Directory.Build.props; missing when the build had no git.
	private static string SourceCommit()
	{
		foreach (var attribute in typeof(Watcher).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
		{
			if (attribute.Key == "SourceCommit")
			{
				return attribute.Value ?? "unknown";
			}
		}
		return "unknown";
	}

	// Amounts are read in full, never from .value alone (ActionEffectReading.FullAmount).
	private static uint FullAmount(EffectEntry entry)
		=> ActionEffectReading.FullAmount(entry);

	// What a listed area hit measures on one member: its amount as a share of his maximum HP - or nothing,
	// when the amount reaches his whole health and the barrier he carries (A298). Such a hit kills from
	// full health whatever anyone does: the hit of a kill mechanic on the one who missed it (Megadeath on a
	// member without the zombie debuff, 941 % of max HP, while the same cast took 10 % from one who had
	// it; Necrohazard near its centre, 588 %), or a raidwide nothing in reach answers. Stored, it stood for
	// good, because the table only rises, and every reader took the cast for a raidwide that kills: the heal
	// ahead went out whenever anyone missed health, the holds gave way, the mitigation was spent, and the
	// tankbuster plan counted the party dead. A cast nobody survives is left unrated, and unrated is
	// answered as before the table: the mitigation goes out.
	//
	// A hit he survived only behind his barrier counts as his whole health - it takes all of it, and the
	// table never holds more than that, so a load can tell an old reading from a measurement. The barrier
	// is read when the effect arrives: that is when the action locks in, before its damage applies
	// (cactbot LogGuide, line 37), and a barrier counts if it stands at that moment. The amount includes the part
	// a barrier absorbs - hits swallowed whole by one carry their full figure while the member's HP stays (his ACT
	// logs of 26.10.2026, A449) -, so the bound is exact.
	//
	// The share is stored unmitigated (A441), as the tankbuster table stores its hits: divided by what got through the
	// mitigation that stood on the member and on the caster when the effect arrived, by the figures of the effect texts
	// (TankbusterTable.MitigationFactor), and scaled to his maximum without a raise. Read raw, a raidwide whose first
	// landing the party took under three mitigations stood too low until a hit under less raised it (Seelenüberfluss,
	// 26.10.2026: 25 % measured, about 34 % unmitigated, A419). A mitigation no text rates and a barrier are not divided
	// out, so the figure errs low, as before, and a later hit can raise it; it never exceeds his whole health.
	private static bool TryAreaShare(uint amount, IBattleChara member, uint actionId, IBattleChara? caster, out float share)
	{
		share = 0f;
		var maxHp = member.MaxHp;
		if (maxHp == 0 || amount >= maxHp + member.GetObjectShield())
		{
			return false;
		}

		share = Math.Min(amount, maxHp) / (float)maxHp;
		var factor = TankbusterTable.MitigationFactor(actionId, member, caster);
		if (factor > 0f)
		{
			share = Math.Min(1f, share * (1f + TankbusterTable.MaxHpRaise(member)) / factor);
		}

		return true;
	}

	// An action measured since the readings are stored unmitigated (A441). Until then its stored reading is of the earlier
	// form - the hit as it arrived, often under mitigation - and DataCenter.AreaShareNow reads it as it was read then,
	// without taking the mitigation standing now off it a second time (A446). A reading of the new form that does not
	// raise the stored one still marks it: the stored figure is then at least an unmitigated measurement.
	private static void MarkMeasuredUnmitigated(uint actionId)
	{
		if (OtherConfiguration.AreaPotentialUnmitigated.TryAdd(actionId, 1))
		{
			_ = OtherConfiguration.SaveAreaPotentialUnmitigated();
		}
	}

	// How long a listed area series hits after its first landing, the longest seen: DataCenter.AreaShareNow counts only the
	// mitigation that stands until its last hit (A460). Stored alongside the share, under the same conditions.
	private static void RecordAreaSeriesSpan(uint actionId, float span)
	{
		if (span <= 0f || (OtherConfiguration.AreaSeriesSpan.TryGetValue(actionId, out var known) && known >= span))
		{
			return;
		}

		OtherConfiguration.AreaSeriesSpan[actionId] = span;
		_ = OtherConfiguration.SaveAreaSeriesSpan();
		var name = Service.GetSheet<Lumina.Excel.Sheets.Action>().GetRowOrDefault(actionId)?.Name.ExtractText() ?? "?";
		DefenseTrace.Line($"area series {name} #{actionId}: hits {span:F1} s after its first landing - stored");
	}

	// A clean area reading: no damage up on the caster and no vulnerability on a member measured - the mitigation
	// TryAreaShare divides out knows neither, and both raise the figure (concept 13, rule 2). A vulnerability the hit
	// itself lays counts where it already stands when the effect arrives: one clean reading too few, which only holds a
	// later lowering back.
	private static bool IsCleanAreaReading(IBattleChara? caster, bool vulnerableMeasured)
		=> !vulnerableMeasured && caster?.CarriesDamageUp() != true;

	// A listed cast that lands several times - a series of hits after one cast, Bodyweight Exorcism's four in three-second
	// steps - is rated by what the whole series takes from a member, not by its largest hit: one hit of it leaves
	// everybody above the area heal line, the series does not, and "Skip mitigation for small area casts" mitigates
	// in full a hit after which healing is called for (A349). A landing belongs to the series while it follows the
	// previous one sooner than the action's own cast time: the caster could not have cast it again in between, so it
	// is the same use, or another caster's - a mechanic's helpers, a pack -, and they reach the party together. A
	// member's sum stops at his whole health, the most the table holds (IsWithinWholeHealth).
	//
	// Whoever cast it, because the readers rate one announced cast at a time and would otherwise see a single hit of
	// the pack or the mechanic. Except a targetable enemy casting anew - it already landed in the series, and its
	// own previous landing lies at least a cast time back: the pack's round is over, and the landing starts a new
	// series. Without that, a big pull of like enemies kept one series running while any of them filled the gap, and
	// its sum grew with the pull, not the attack - three enemies recasting an 8 % area cast every 8 s stood at 64 %
	// instead of 24 % (area_series_model.py). Untargetable casters, the mechanic objects, keep the gap rule alone: a
	// helper casting twice in one mechanic would split it, and part of the towers would be rated (concept 13, A480).
	private sealed class AreaSeries
	{
		public DateTime First;
		public DateTime Last;
		public readonly Dictionary<ulong, float> Sums = [];
		public readonly Dictionary<ulong, DateTime> LandedBy = [];
	}

	// The span is the time from the series' first landing to this one: how long after its first hit the series still hits.
	private static float SeriesShare(uint listed, Dictionary<ulong, float> shares, IGameObject? caster, out float span)
	{
		var now = DateTime.Now;
		var castSeconds = Service.GetSheet<Lumina.Excel.Sheets.Action>().TryGetRow(listed, out var row)
			? row.CastSeconds() : 0f;
		var fresh = !_areaSeries.TryGetValue(listed, out var series) || (now - series.Last).TotalSeconds > castSeconds;
		if (!fresh && caster is { IsTargetable: true } && series!.LandedBy.TryGetValue(caster.GameObjectId, out var own)
			&& (now - own).TotalSeconds >= castSeconds)
		{
			fresh = true;
			DefenseTrace.Line($"area series #{listed}: {caster.Name.TextValue} cast it anew, {(now - own).TotalSeconds:F1} s"
				+ $" after its landing in the series {(now - series.First).TotalSeconds:F1} s long - a new series");
		}

		if (fresh)
		{
			series = new AreaSeries { First = now };
			_areaSeries[listed] = series;
		}

		series!.Last = now;
		if (caster != null)
		{
			series.LandedBy[caster.GameObjectId] = now;
		}

		span = (float)(series.Last - series.First).TotalSeconds;
		var highest = 0f;
		foreach (var (member, share) in shares)
		{
			var sum = Math.Min(1f, series.Sums.GetValueOrDefault(member) + share);
			series.Sums[member] = sum;
			highest = Math.Max(highest, sum);
		}

		return highest;
	}

	private static readonly List<(ulong Target, uint Amount)> _ownHeals = [];

	/// <summary>
	/// Reads a same-named hit after an empty landing as that landing: whether it reached the player, for
	/// "Skip area defence for casts that missed you", and how hard it hit, for the damage table.
	/// </summary>
	/// <remarks>
	/// Without it the empty landing stood as "missed you": the next cast opened no area defence for the hit
	/// that does come, and the table never rated the cast, so every other rule treated it as unknown. A
	/// wind-up nothing follows keeps its "missed you", which is right - it hurt nobody.
	/// </remarks>
	private static void RecordFollowUpOfEmptyLanding(ActionEffectSet set, Lumina.Excel.Sheets.Action action,
		IBattleChara player, bool hitPlayer)
	{
		uint listed;
		lock (_emptyLandingByName)
		{
			if (!DataCenter.InCombat)
			{
				_emptyLandingByName.Clear();
				return;
			}

			// A listed action records its own landing.
			if (OtherConfiguration.HostileCastingArea.Contains(action.RowId)
				|| !_emptyLandingByName.TryGetValue(action.Name.ExtractText(), out listed))
			{
				return;
			}
		}

		var damaged = DamagedPartyMembers(set);
		if (damaged == 0 && !hitPlayer)
		{
			return;
		}

		// Reached once is reached: a mechanic resolved in several sets need not hit him in the first.
		if (hitPlayer && !player.IsDead)
		{
			DataCenter.AreaCastReachedPlayer[listed] = true;
		}

		var shares = new Dictionary<ulong, float>();
		var beyondHealth = 0;
		var vulnerableMeasured = false;
		foreach (var member in DataCenter.PartyMembers)
		{
			if (member == null || member.MaxHp == 0)
			{
				continue;
			}

			foreach (var effect in set.TargetEffects)
			{
				if (effect.TargetID == member.GameObjectId && TryGetDamageEntry(effect, out var entry)
					&& FullAmount(entry) > 0)
				{
					if (TryAreaShare(FullAmount(entry), member, action.RowId, set.Source as IBattleChara, out var share))
					{
						shares[member.GameObjectId] = Math.Max(shares.GetValueOrDefault(member.GameObjectId), share);
						vulnerableMeasured |= member.CarriesVulnerabilityUp();
					}
					else
					{
						beyondHealth++;
					}
				}
			}
		}

		var highestShare = SeriesShare(listed, shares, set.Source, out var seriesSpan);

		// Above the level the content syncs to or in an unrestricted party (A466), the reading is smaller than in sync and would rate the action too small for
		// every synced run after it (A452).
		if (highestShare > 0f && DataCenter.OutOfContentSync)
		{
			DataCenter.RecordAreaMeasurementOutcome(listed, $"not measured - {DataCenter.OutOfContentSyncReason}",
				$" ({highestShare:P0} of max HP unmitigated), from #{action.RowId}");
			DefenseTrace.Line($"follow-up of #{listed}: {action.Name.ExtractText()} #{action.RowId}, highest {highestShare:P0}"
				+ $" of max HP unmitigated - not stored, {DataCenter.OutOfContentSyncReason}");
			return;
		}

		// Same rule as a listed landing: only a real amount a member can survive measures. Every reading goes to the
		// revisions, which keep the highest for the readers as before (concept 13, "Stufe 1").
		if (highestShare > 0f)
		{
			OtherConfiguration.MeasureAreaShare(listed, highestShare, IsCleanAreaReading(set.Source as IBattleChara, vulnerableMeasured));
			RecordAreaSeriesSpan(listed, seriesSpan);
			MarkMeasuredUnmitigated(listed);
		}

		if (highestShare > 0f)
		{
			DataCenter.RecordAreaMeasurementOutcome(listed, "measured", $" at {highestShare:P0} of max HP unmitigated, from #{action.RowId}");
		}
		else if (beyondHealth > 0)
		{
			DataCenter.RecordAreaMeasurementOutcome(listed, "not measured - every hit beyond max HP and barrier",
				$", from #{action.RowId}");
		}

		DefenseTrace.Line($"follow-up of #{listed}: {action.Name.ExtractText()} #{action.RowId}, reached you {hitPlayer},"
			+ $" {damaged} party members damaged, highest {highestShare:P0} of max HP unmitigated"
			+ (beyondHealth > 0 ? $", {beyondHealth} hit beyond max HP and barrier not measured" : string.Empty));
	}

	private static float DamageShareOn(ActionEffectSet set, ulong targetId, uint denom)
	{
		float share = 0;
		foreach (var effect in set.TargetEffects)
		{
			if (effect.TargetID == targetId)
			{
				effect.ForEach(entry =>
				{
					if (IsDamageEntry(entry.type))
					{
						share += (float)FullAmount(entry) / denom;
					}
				});
			}
		}

		return share;
	}

	// The statuses an effect set puts on one target: the entry's value is the status id.
	private static List<uint> StatusesAppliedTo(ActionEffectSet set, ulong targetId)
	{
		List<uint> statuses = [];
		foreach (var effect in set.TargetEffects)
		{
			if (effect.TargetID != targetId)
			{
				continue;
			}

			effect.ForEach(entry =>
			{
				if (entry.type == ActionEffectType.ApplyStatusEffectTarget && entry.value != 0)
				{
					statuses.Add(entry.value);
				}
			});
		}

		return statuses;
	}

	// What an own action did, for OwnHitWatch: the damage on its main target, against the potency its text states; and for
	// the Blue Mage, whether a spell of the fixed-or-lethal class did anything and which states the targets resisted (A507).
	private static void ObserveOwnHits(ActionEffectSet set, Lumina.Excel.Sheets.Action action)
	{
		var mainId = set.Target?.GameObjectId ?? 0;
		foreach (var effect in set.TargetEffects)
		{
			if (effect.TargetID == mainId && TryGetDamageEntry(effect, out var entry))
			{
				OwnHitWatch.Hit(action.RowId, FullAmount(entry), set.Target as IBattleChara);
				break;
			}
		}

		if (DataCenter.Job != ECommons.ExcelServices.Job.BLU || action.ClassJob.RowId != (uint)ECommons.ExcelServices.Job.BLU)
		{
			return;
		}

		// The class's immunity is learned from the spells that take health (Missile, Launcher, Dimensional Shift, Tail Screw):
		// they answer a failed chance with a miss. Doom, Level 5 Death and Petrify, Ultravibration put on a status or kill,
		// and how the game answers their failed chance is not known, so they teach nothing.
		var lowerLevelOnly = ActionTooltip.IgnoresHigherLevel(action.RowId)
			&& ActionTooltip.Text(action.RowId).Contains(" HP", StringComparison.Ordinal);
		// A status that wears off with repeated use says nothing about the enemy's kind when it does not land.
		var diminishes = ActionTooltip.StatusDiminishes(action.RowId);
		var promised = diminishes ? new List<uint>() : PromisedTargetStatuses(action.RowId);
		var againstUndead = ActionTooltip.TryUndeadPotency(action.RowId, out var undeadPotency)
			& ActionTooltip.TryPotencyOn(action.RowId, 1, out var basePotency);
		foreach (var effect in set.TargetEffects)
		{
			IBattleChara? target = null;
			foreach (var hostile in DataCenter.AllHostileTargets)
			{
				if (hostile.GameObjectId == effect.TargetID)
				{
					target = hostile;
					break;
				}
			}

			if (target == null)
			{
				continue;
			}

			bool damaged = false, applied = false, missed = false, invulnerable = false;
			List<uint>? resisted = null;
			effect.ForEach(e =>
			{
				if (IsDamageEntry(e.type))
				{
					damaged |= FullAmount(e) > 0;
				}
				else if (e.type == ActionEffectType.ApplyStatusEffectTarget)
				{
					applied = true;
				}
				else if (e.type == ActionEffectType.Miss)
				{
					missed = true;
				}
				// "Invulnerable" is a phase only while the target carries an invincibility status; without one it is how the
				// game answers a spell its kind is immune to, and that is learned (OwnHitWatch).
				else if (e.type == ActionEffectType.Invulnerable)
				{
					invulnerable = HasInvincibility(target);
				}
				// A fully resisted status (52, 55), as the sleep and stun watches read it.
				else if (e.type is ActionEffectType.FullResistStatus or ActionEffectType.FullResistStatus2)
				{
					(resisted ??= []).Add(e.value);
				}
			});

			OwnHitWatch.Landed(target.GameObjectId, action.RowId, applied);
			if (againstUndead && TryGetDamageEntry(effect, out var undeadEntry))
			{
				OwnHitWatch.UndeadHit(action.RowId, target, FullAmount(undeadEntry), basePotency, undeadPotency);
			}

			if (lowerLevelOnly || (resisted != null && !diminishes) || promised.Count > 0)
			{
				OwnHitWatch.Outcome(action.RowId, target, lowerLevelOnly, damaged, applied, missed, invulnerable,
					diminishes || resisted == null ? new List<uint>() : resisted, promised);
			}
		}
	}

	private static bool HasInvincibility(IBattleChara target)
	{
		if (target.StatusList == null)
		{
			return false;
		}

		foreach (var status in target.StatusList)
		{
			if (status != null && status.IsInvincible())
			{
				return true;
			}
		}

		return false;
	}

	// The statuses an own action puts on its target by the rotation's setting, where its effect text names no chance for
	// them: a hit without them did not land them (OwnHitWatch).
	private static List<uint> PromisedTargetStatuses(uint actionId)
	{
		List<uint> promised = [];
		var actions = DataCenter.CurrentRotation?.AllBaseActions;
		if (actions == null || ActionTooltip.Text(actionId).Contains("chance", StringComparison.OrdinalIgnoreCase))
		{
			return promised;
		}

		foreach (var candidate in actions)
		{
			if (candidate?.ID != actionId || candidate.Setting.TargetStatusProvide is not { } provides)
			{
				continue;
			}

			foreach (var status in provides)
			{
				promised.Add((uint)status);
			}

			break;
		}

		return promised;
	}
}
