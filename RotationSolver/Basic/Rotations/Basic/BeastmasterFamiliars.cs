using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace RotationSolver.Basic.Rotations.Basic;

/// <summary>
/// What a familiar brings, read from the game's sheets at the player's synced level: its Trick (the instinctual skill Trick
/// orders) and its Tempered Release, their shape and potency, and whether it heals or protects you (concept 01, "Begleiter
/// nach Lage", A485).
/// </summary>
/// <param name="Pet">The familiar, as the battlehorn slots hold it.</param>
/// <param name="TrickId">The action its Trick executes.</param>
/// <param name="ReleaseId">The action its Tempered Release executes.</param>
/// <param name="TrickRadius">0 for a single-target Trick, else the radius its area reaches.</param>
/// <param name="TrickPotency">The Trick's potency as its tooltip first states it, 0 when it states none.</param>
/// <param name="TrickHeals">The Trick's damage is partly absorbed as your HP.</param>
/// <param name="ReleaseHeals">The Tempered Release grants Life Siphon.</param>
/// <param name="ReleaseProtects">The Tempered Release lowers the damage you take.</param>
/// <param name="ReleaseRetreats">The familiar leaves combat after its Tempered Release, as after Parting Blow (Wespe's
/// Final Sting; R1, concept 01, A488).</param>
/// <param name="ReleaseStops">The Tempered Release stops the enemies it reaches from acting: Stun, Petrification or Deep
/// Freeze (C1, concept 01, A491).</param>
/// <param name="ReleaseStopsByStun">That stop is a stun, which halves with each application.</param>
/// <param name="ReleaseRadius">0 for a single-target Tempered Release, else the radius its area reaches.</param>
/// <param name="ReleasePotency">What the Tempered Release deals each enemy it reaches: the potency its tooltip first
/// states, times its hits for one that strikes several times ("sixfold"); 0 when it states none (B5, A522).</param>
/// <param name="ReleaseHurtsParty">The Tempered Release's party effect deals damage over time to the party (Hypodermic
/// Hustle; C3).</param>
/// <param name="BorrowProtects">The Beast Mode action Borrow unlocks for its kin lowers the damage you take.</param>
/// <param name="TrickHeart">The heart its Trick grants, by the instinctual affinity its tooltip states; 0 when it states
/// none.</param>
public sealed record FamiliarProfile(BeastmasterPet Pet, uint TrickId, uint ReleaseId, float TrickRadius, float TrickPotency,
	bool TrickHeals, bool ReleaseHeals, bool ReleaseProtects, bool ReleaseRetreats, bool ReleaseStops, bool ReleaseStopsByStun,
	float ReleaseRadius, float ReleasePotency, bool ReleaseHurtsParty, bool BorrowProtects, StatusID TrickHeart);

public partial class BeastmasterRotation
{
	// What the sheets say of a familiar regardless of level: the actions its Trick and Tempered Release execute, the reach of
	// the Trick, and its kin.
	private readonly record struct FamiliarRow(uint TrickId, uint ReleaseId, float TrickRadius, float ReleaseRadius,
		BeastmasterKinType Kin);

	// Built once from the sheets; null until then, empty when the familiar's Pet row cannot be told (B1).
	private static Dictionary<byte, FamiliarRow>? _familiarRows;

	// A1 (concept 01, "Was gilt beim Anfreunden", A487): the familiar a model belongs to - the model and base of the first
	// look its Pet row allows (Pet.Unknown8, AllowedPetMirage[0], to PetMirage.ModelChara). Built with the rows.
	private static readonly Dictionary<(ushort Model, byte Base), BeastmasterPet> _familiarByModel = [];
	private static string _familiarCatalogState = "not built yet";

	// The profiles read so far at _profileLevel, only for the familiars asked for - the three on the horns.
	private static readonly Dictionary<byte, FamiliarProfile> _profiles = [];
	private static int _profileLevel = -1;

	/// <summary>How the familiar catalog was built, for the trace.</summary>
	public static string FamiliarCatalogState => _familiarCatalogState;

	/// <summary>
	/// The profile of <paramref name="pet"/> at the player's synced level, null for none or when the catalog could not be
	/// built - the rotation then keeps its configured horn order.
	/// </summary>
	public static FamiliarProfile? FamiliarOf(BeastmasterPet pet)
	{
		if (pet == BeastmasterPet.None)
		{
			return null;
		}

		_familiarRows ??= BuildFamiliarRows();
		var level = DataCenter.PlayerSyncedLevel();
		if (level != _profileLevel)
		{
			// What an action brings follows the level: while he levels, and in every level sync.
			_profiles.Clear();
			_profileLevel = level;
		}

		if (_profiles.TryGetValue((byte)pet, out var known))
		{
			return known;
		}

		if (!_familiarRows.TryGetValue((byte)pet, out var row))
		{
			return null;
		}

		var profile = ReadProfile(pet, row);
		_profiles[(byte)pet] = profile;
		DefenseTrace.Line($"beastmaster familiar {pet} at level {level}: Trick {profile.TrickPotency:F0}"
			+ $"{(profile.TrickRadius > 0f ? $" in {profile.TrickRadius:F0} y" : " single")}"
			+ $"{(profile.TrickHeals ? ", Trick heals" : string.Empty)}, Release {profile.ReleasePotency:F0}"
			+ $"{(profile.ReleaseRadius > 0f ? $" in {profile.ReleaseRadius:F0} y" : " single")}"
			+ $"{(profile.ReleaseHeals ? ", Release heals" : string.Empty)}"
			+ $"{(profile.ReleaseProtects ? ", Release protects" : string.Empty)}{(profile.ReleaseRetreats ? ", Release retreats" : string.Empty)}"
			+ $"{(profile.ReleaseStops ? $", Release stops{(profile.ReleaseStopsByStun ? " (stun)" : string.Empty)}" : string.Empty)}"
			+ $"{(profile.ReleaseHurtsParty ? ", Release hurts the party" : string.Empty)}"
			+ $"{(profile.BorrowProtects ? ", Borrow protects" : string.Empty)}"
			+ $", Trick {(profile.TrickHeart == 0 ? "without affinity" : profile.TrickHeart.ToString())}");
		return profile;
	}

	/// <summary>
	/// The familiar <paramref name="horn"/> holds, from the slots the game keeps (<see cref="CurrentPets"/>, one per horn in
	/// the order of <see cref="HornOrder"/>).
	/// </summary>
	public static BeastmasterPet PetOnHorn(HornOrder horn)
	{
		var pets = CurrentPets;
		var index = HornSlot(horn);
		return horn != HornOrder.None && index < pets.Length ? (BeastmasterPet)pets[index] : BeastmasterPet.None;
	}

	/// <summary>Whether <paramref name="horn"/> can be used now, as the game answers it.</summary>
	public static bool HornReady(HornOrder horn) => horn switch
	{
		HornOrder.FirstBattlehorn => FirstBattleHornReady,
		HornOrder.SecondBattlehorn => SecondBattleHornReady,
		HornOrder.ThirdBattlehorn => ThirdBattleHornReady,
		_ => false,
	};

	/// <summary>The three battlehorns, in the order the game keeps their slots.</summary>
	protected static readonly HornOrder[] ThreeHorns = [HornOrder.FirstBattlehorn, HornOrder.SecondBattlehorn, HornOrder.ThirdBattlehorn];

	/// <summary>Where <paramref name="horn"/> sits in <see cref="CurrentPets"/>.</summary>
	protected static int HornSlot(HornOrder horn) => (int)horn - (int)HornOrder.FirstBattlehorn;

	/// <summary>
	/// Whether a horn other than the one whose familiar is out holds a familiar and is ready: one to take its place should
	/// it retreat (R1, concept 01, A488). In combat a horn's recast begins only when its familiar retreats, so the horn of
	/// the familiar out is never its own replacement, whatever the game answers for it. With the slots unreadable - all
	/// empty although a familiar is out - every other ready horn counts.
	/// </summary>
	public static bool ReplacementHornReady
	{
		get
		{
			var active = (HornOrder)ActiveBattlehorn;
			var slotsRead = false;
			foreach (var horn in ThreeHorns)
			{
				slotsRead |= PetOnHorn(horn) != BeastmasterPet.None;
			}

			foreach (var horn in ThreeHorns)
			{
				if (horn != active && HornReady(horn) && (!slotsRead || PetOnHorn(horn) != BeastmasterPet.None))
				{
					return true;
				}
			}

			return false;
		}
	}

	/// <summary>
	/// Whether the familiar out leaves combat with its Tempered Release (R1, concept 01, A488): false for none out or none
	/// known.
	/// </summary>
	public static bool ReleaseRetreatsFamiliar => FamiliarOf(CurrentPet)?.ReleaseRetreats == true;

	/// <summary>
	/// What <paramref name="profile"/>'s Trick brings against the enemies around <paramref name="target"/>: its potency
	/// times the enemies it reaches - one for a single-target Trick, else the target and every other hostile in combat
	/// within its radius of the target. A cone or a line counts like a circle around the target: where the familiar will
	/// stand is not known when it is called.
	/// </summary>
	public static float TrickValue(FamiliarProfile profile, IBattleChara? target)
	{
		var potency = profile.TrickPotency > 0f ? profile.TrickPotency : 1f;
		if (profile.TrickRadius <= 0f || target == null)
		{
			return potency;
		}

		var hits = 0;
		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			// An unpulled pack beside the target is no hit: the fight is with what is in combat.
			if (hostile != null && !hostile.IsDead && (hostile.GameObjectId == target.GameObjectId || hostile.InCombat())
				&& Vector3.Distance(hostile.Position, target.Position) - hostile.HitboxRadius <= profile.TrickRadius)
			{
				hits++;
			}
		}

		return potency * Math.Max(1, hits);
	}

	// C1 counts a stopping Release only against a pack (concept 01, A491): one attacker alone is the elite or boss case,
	// where an immunity the game does not show would leave the danger without protection.
	private const int PackAttackers = 2;

	/// <summary>
	/// C1 of concept 01 (A491): whether <paramref name="profile"/>'s Tempered Release would stop every enemy attacking you -
	/// at least two of them in combat, each within its radius of <paramref name="target"/>, none with a boss icon, and for
	/// a stun none immune or stunned since its last immunity (the next stun is halved).
	/// </summary>
	public static bool StopsAttackers(FamiliarProfile profile, IBattleChara? target)
	{
		var player = Player;
		if (!profile.ReleaseStops || profile.ReleaseRadius <= 0f || target == null || player == null)
		{
			return false;
		}

		var attackers = 0;
		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile == null || hostile.IsDead || !hostile.InCombat() || hostile.TargetObjectId != player.GameObjectId)
			{
				continue;
			}

			if (Vector3.Distance(hostile.Position, target.Position) - hostile.HitboxRadius > profile.ReleaseRadius
				|| hostile.IsBossFromIcon()
				|| (profile.ReleaseStopsByStun && (!StunWatch.CanStillBeStunned(hostile) || StunWatch.Applications(hostile) > 0)))
			{
				return false;
			}

			attackers++;
		}

		return attackers >= PackAttackers;
	}

	/// <summary>
	/// Whether the familiar out deals damage over time to the party with its Tempered Release (C3, concept 01, A491).
	/// </summary>
	public static bool ReleaseHurtsPartyFamiliar => FamiliarOf(CurrentPet)?.ReleaseHurtsParty == true;

	/// <summary>
	/// The familiar <paramref name="target"/> can become (A1): its base row marked capturable - 5 in the unnamed column 10
	/// of BNpcBase, as WrathCombo reads it - and its model that of a familiar's look. None for any other target.
	/// </summary>
	public static BeastmasterPet CapturableFamiliar(IBattleChara? target)
	{
		if (target == null)
		{
			return BeastmasterPet.None;
		}

		_familiarRows ??= BuildFamiliarRows();
		var bases = Svc.Data.GetExcelSheet<BNpcBase>();
		if (bases == null || !bases.TryGetRow(target.BaseId, out var npc) || npc.Unknown10 != CapturableBase
			|| !npc.ModelChara.IsValid)
		{
			return BeastmasterPet.None;
		}

		return _familiarByModel.TryGetValue((npc.ModelChara.Value.Model, npc.ModelChara.Value.Base), out var pet)
			? pet
			: BeastmasterPet.None;
	}

	// The value of BNpcBase column 10 on the base rows a beastmaster can capture (WrathCombo; four of the ten rows with the
	// squirrel's model carry it, A487). No source names the column.
	private const byte CapturableBase = 5;

	/// <summary>Whether the game has loaded the list of the player's familiars (<see cref="FamiliarUnlocked"/>).</summary>
	public static unsafe bool FamiliarListLoaded
	{
		get
		{
			var manager = XBMManager.Instance();
			return manager != null && manager->State == XBMManager.DataState.Received;
		}
	}

	/// <summary>
	/// Whether the player has <paramref name="pet"/> already, as the game's familiar list says; null while the game has
	/// not loaded that list.
	/// </summary>
	public static unsafe bool? FamiliarUnlocked(BeastmasterPet pet)
	{
		var manager = XBMManager.Instance();
		if (manager == null || manager->State != XBMManager.DataState.Received)
		{
			return null;
		}

		return manager->IsPetUnlocked((uint)pet);
	}

	/// <summary>
	/// A1 and A2 of concept 01 ("Was gilt beim Anfreunden", A487, A525): <paramref name="target"/> can become a familiar
	/// the player has not got, is not marked, not above his level, the game would take Capture on it - and waiting for
	/// the next weaponskill could let it fall before that mark arrives (<see cref="CaptureNow"/>), so the mark is sure
	/// to land at the least HP that allows it.
	/// </summary>
	public static unsafe bool CaptureDue(IBattleChara target)
	{
		var player = Player;
		if (player == null || target.Level > player.Level || target.HasStatus(false, StatusID.InterestCaptured))
		{
			return false;
		}

		var pet = CapturableFamiliar(target);
		if (pet == BeastmasterPet.None || FamiliarUnlocked(pet) != false)
		{
			return false;
		}

		// A2 (A525): now, when waiting for the next weaponskill could let it fall before that mark arrives.
		if (!CaptureNow(target, out _))
		{
			return false;
		}

		var actions = ActionManager.Instance();
		return actions != null && actions->GetActionStatus(ActionType.Action, (uint)ActionID.CapturePvE, target.GameObjectId,
			checkRecastActive: false, checkCastingActive: false) == 0;
	}

	// A2 (concept 01, A523): the mark of Capture arrives after its effect - 0.62 s later in both of the owner's recordings
	// (02.11.2026 Killermantis, 03.11.2026 Flechten-Goobbue). Until RSR has measured it from the press, that delay.
	private const float RecordedMarkDelay = 0.62f;
	private static float _captureLanding = -1f;

	/// <summary>
	/// How long the mark of Capture takes from pressing it to arriving on the enemy: last measured in this session, or the
	/// delay the owner's recordings showed before the first measurement (A2, A523).
	/// </summary>
	public static float CaptureLanding => _captureLanding >= 0f ? _captureLanding : RecordedMarkDelay;

	/// <summary>
	/// A2 of concept 01 (his precept of 04.11.2026: "so sicher sein, dass immer genug zeit bleibt"; A525, A527): whether
	/// waiting for the next weaponskill could let <paramref name="target"/> fall before that Capture's mark arrives. The
	/// span is what remains of the running weaponskill, one more and the landing; it could, when the target holds no more
	/// health than the larger of two falls within such a span - its own since its last full health, and the largest any
	/// enemy of this area took, its killing blow included -, or its corrected time to kill is no longer. No lower bound:
	/// a mark that may come too late still goes.
	/// </summary>
	/// <param name="target">The enemy.</param>
	/// <param name="reason">Why, for the trace.</param>
	/// <returns>True when Capture is due now.</returns>
	public static bool CaptureNow(IBattleChara target, out string reason)
	{
		reason = string.Empty;
		if (target == null)
		{
			return false;
		}

		var span = WeaponRemain + WeaponTotal + CaptureLanding;
		var held = target.GetHealthRatio();
		var own = TimeToKillTracker.LargestFall(target.GameObjectId, span, held);
		var area = target.MaxHp > 0 ? TimeToKillTracker.AreaFall(span) / target.MaxHp : 0f;
		var fall = float.IsNaN(own) ? area : Math.Max(own, area);
		if (fall > 0f && held <= fall)
		{
			reason = $"it holds {held:P1}; the largest fall within {span:F1} s was {(float.IsNaN(own) ? 0f : own):P1} of its "
				+ $"health on it, {area:P1} on an enemy here";
			return true;
		}

		var ttk = target.GetCorrectedTTK();
		if (!float.IsNaN(ttk) && ttk <= span)
		{
			reason = $"its time to kill {ttk:F1} s is within {span:F1} s";
			return true;
		}

		return false;
	}

	// An enemy in combat that would be a familiar the player lacks, followed until it falls (A2's trace, A523); when RSR
	// last chose Capture on it (A528).
	private readonly record struct CaptureWatch(string Name, BeastmasterPet Pet, bool Aimed, DateTime AimedAt, bool Marked,
		float Ttk);

	private static readonly Dictionary<ulong, CaptureWatch> _captureWatch = [];
	private static readonly HashSet<ulong> _captureSeen = [];
	private static readonly List<ulong> _captureGone = [];

	// A528: a familiar whose enemy fell marked, with that enemy, until the game lists it or the body is gone while it
	// still does not - the game enters a pact about 2 s after the fall, with the defeat (04.11.2026: Drake, Colibri,
	// Coeurl), and a verdict at once called the Slime and the Worm "no pact".
	private static readonly Dictionary<BeastmasterPet, (string Name, ulong Id)> _pactsAwaited = [];
	private static readonly List<BeastmasterPet> _pactsDecided = [];
	private static uint _pactArea;

	/// <summary>A2: the rotation chose Capture on <paramref name="target"/>.</summary>
	public static void CaptureAimed(IBattleChara target)
	{
		var pet = CapturableFamiliar(target);
		var now = DateTime.Now;
		_captureWatch[target.GameObjectId] = _captureWatch.TryGetValue(target.GameObjectId, out var known)
			? known with { Aimed = true, AimedAt = now }
			: new CaptureWatch(target.Name.TextValue, pet, true, now, false, target.GetCorrectedTTK());
	}

	/// <summary>
	/// A2 (concept 01, A523, A528): follows every living enemy in combat that would be a familiar the player lacks - when
	/// the mark of Capture arrives on it, measured from the press, and whether RSR or the player pressed it; whether it
	/// falls marked; whether the game lists the familiar after. One line each in DefenseTrace.log.
	/// </summary>
	public static void WatchCapture()
	{
		var player = Player;
		if (player == null)
		{
			return;
		}

		var area = (uint)Svc.ClientState.TerritoryType;
		var areaLeft = area != _pactArea;
		_pactArea = area;
		_pactsDecided.Clear();
		foreach (var (pet, from) in _pactsAwaited)
		{
			if (FamiliarUnlocked(pet) == true)
			{
				DefenseTrace.Line($"Capture: pact - {pet} from {from.Name} is in the bestiary now");
				_pactsDecided.Add(pet);
			}
			else if (areaLeft || Svc.Objects.SearchById(from.Id) == null)
			{
				// The defeat is long through once its body is gone, or the area is left.
				DefenseTrace.Line($"Capture: no pact - {from.Name} fell marked, {pet} is still not in the bestiary now that "
					+ (areaLeft ? "you left the area" : "its body is gone"));
				_pactsDecided.Add(pet);
			}
		}

		foreach (var pet in _pactsDecided)
		{
			_ = _pactsAwaited.Remove(pet);
		}

		_captureSeen.Clear();
		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile == null || hostile.IsDead || !hostile.InCombat())
			{
				continue;
			}

			var id = hostile.GameObjectId;
			if (!_captureWatch.TryGetValue(id, out var watch))
			{
				var pet = CapturableFamiliar(hostile);
				if (pet == BeastmasterPet.None || FamiliarUnlocked(pet) != false || hostile.Level > player.Level)
				{
					continue;
				}

				watch = new CaptureWatch(hostile.Name.TextValue, pet, false, DateTime.MinValue, false, float.NaN);
			}

			_captureSeen.Add(id);
			var marked = hostile.HasStatus(true, StatusID.InterestCaptured);
			if (marked && !watch.Marked && CapturePressedWithinGcd())
			{
				_captureLanding = DataCenter.DefaultGCDElapsed;
				// By RSR when it chose Capture on this enemy within a weaponskill before the press.
				var pressedAt = DateTime.Now.AddSeconds(-DataCenter.DefaultGCDElapsed);
				var byRsr = watch.Aimed && watch.AimedAt >= pressedAt.AddSeconds(-DataCenter.DefaultGCDTotal);
				DefenseTrace.Line($"Capture: mark on {watch.Name} ({watch.Pet}) {_captureLanding:F2} s after the press "
					+ (byRsr ? "by RSR" : "by hand") + $", {hostile.GetHealthRatio():P1} HP - the next Capture counts "
					+ "this landing");
			}

			_captureWatch[id] = watch with { Marked = watch.Marked || marked, Ttk = hostile.GetCorrectedTTK() };
		}

		_captureGone.Clear();
		foreach (var (id, watch) in _captureWatch)
		{
			if (!_captureSeen.Contains(id))
			{
				_captureGone.Add(id);
				DefenseTrace.Line($"Capture: {watch.Name} ({watch.Pet}) fell or left "
					+ (watch.Marked ? "marked - whether the pact came shows in the bestiary"
						: watch.Aimed ? "unmarked - Capture came too late for its mark"
						: $"unmarked, without Capture - its last time to kill {watch.Ttk:F1} s, landing {CaptureLanding:F2} s"));
				if (watch.Marked)
				{
					_pactsAwaited[watch.Pet] = (watch.Name, id);
				}
			}
		}

		foreach (var id in _captureGone)
		{
			_ = _captureWatch.Remove(id);
		}
	}

	// Whether the GCD now running began with Capture - its effect came after the press, so no later than the GCD began -
	// so the time since that GCD began is the time since its press. The newest record comes first.
	private static bool CapturePressedWithinGcd()
	{
		var elapsed = DataCenter.DefaultGCDElapsed;
		if (elapsed <= 0f)
		{
			return false;
		}

		var now = DateTime.Now;
		foreach (var record in DataCenter.RecordActions)
		{
			if (record.Action.RowId == (uint)ActionID.CapturePvE)
			{
				return (now - record.UsedTime).TotalSeconds <= elapsed;
			}
		}

		return false;
	}

	// Where Pet.Abilities holds what a familiar's Trick and Tempered Release execute.
	private const int TrickAbility = 0;
	private const int ReleaseAbility = TrickAbility + 1;

	// B1: XBMPet leads to the familiar's Pet row through a column Lumina leaves unnamed. Of its three id columns (EXDSchema:
	// Pet, Action, Location), the one whose values differ across all familiars and lead to Pet rows with abilities is it -
	// the Pet ids do; action ids lead to no Pet row, the places they are found at repeat. None or several: no catalog, and
	// the configured order stands. Until A520 only the last two were asked, and the catalog was never built on the
	// owner's client. Sheets only, no tooltip: cheap.
	private static Dictionary<byte, FamiliarRow> BuildFamiliarRows()
	{
		var catalog = new Dictionary<byte, FamiliarRow>();
		try
		{
			var xbm = Svc.Data.GetExcelSheet<XBMPet>();
			var pets = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Pet>();
			var actions = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>();
			var mirages = Svc.Data.GetExcelSheet<PetMirage>();
			if (xbm == null || pets == null || actions == null || mirages == null)
			{
				_familiarCatalogState = "NO CATALOG: a sheet is missing";
				return catalog;
			}

			var rows = new List<XBMPet>();
			foreach (var row in xbm)
			{
				if (row.RowId != 0 && Enum.IsDefined(typeof(BeastmasterPet), (byte)row.RowId))
				{
					rows.Add(row);
				}
			}

			bool LeadsToPets(Func<XBMPet, uint> column)
			{
				var seen = new HashSet<uint>();
				foreach (var row in rows)
				{
					var id = column(row);
					if (id == 0 || !seen.Add(id) || !pets.TryGetRow(id, out var pet) || pet.Abilities.Count <= ReleaseAbility
						|| pet.Abilities[TrickAbility].RowId == 0 || pet.Abilities[ReleaseAbility].RowId == 0)
					{
						return false;
					}
				}

				return rows.Count > 0;
			}

			(string Name, Func<XBMPet, uint> Read)[] columns =
				[("Unknown4", row => (uint)Math.Max(0, row.Unknown4)), ("Unknown5", row => row.Unknown5), ("Unknown6", row => row.Unknown6)];
			var petColumn = -1;
			var leading = 0;
			for (var i = 0; i < columns.Length; i++)
			{
				if (LeadsToPets(columns[i].Read))
				{
					petColumn = i;
					leading++;
				}
			}

			if (leading != 1)
			{
				_familiarCatalogState = $"NO CATALOG: the familiars' Pet column cannot be told ({(leading == 0 ? "none" : leading.ToString())} of {columns.Length})";
				return catalog;
			}

			var petOf = columns[petColumn].Read;

			foreach (var row in rows)
			{
				var pet = pets.GetRow(petOf(row));
				var trick = pet.Abilities[TrickAbility].RowId;
				var radius = actions.TryGetRow(trick, out var trickRow) && trickRow.CastType > 1 ? trickRow.EffectRange : 0f;
				var release = pet.Abilities[ReleaseAbility].RowId;
				var releaseRadius = actions.TryGetRow(release, out var releaseRow) && releaseRow.CastType > 1
					? releaseRow.EffectRange
					: 0f;
				catalog[(byte)row.RowId] = new FamiliarRow(trick, release, radius, releaseRadius, (BeastmasterKinType)row.Unknown7);

				// The Pet row leads back to its XBMPet row (Pet.Unknown18); one that does not is not trusted for a model.
				if (pet.Unknown18 == row.RowId && mirages.TryGetRow(pet.Unknown8, out var mirage)
					&& mirage.ModelChara.IsValid)
				{
					_familiarByModel[(mirage.ModelChara.Value.Model, mirage.ModelChara.Value.Base)] = (BeastmasterPet)row.RowId;
				}
			}

			_familiarCatalogState = $"{catalog.Count} familiars from the sheets (Pet column {columns[petColumn].Name}), "
				+ $"{_familiarByModel.Count} by model";
		}
		catch (Exception ex)
		{
			catalog.Clear();
			_familiarCatalogState = $"NO CATALOG: {ex.GetType().Name}";
		}

		DefenseTrace.Line($"beastmaster familiar catalog: {_familiarCatalogState}");
		return catalog;
	}

	// What the familiar's actions bring at the player's synced level, from their tooltips (ActionTooltip keeps each text
	// per level).
	private static FamiliarProfile ReadProfile(BeastmasterPet pet, FamiliarRow row)
	{
		var potency = ActionTooltip.TryPotencyOn(row.TrickId, 1, out var stated) ? stated : 0f;
		var trickText = ActionTooltip.Text(row.TrickId);
		var releaseText = ActionTooltip.Text(row.ReleaseId);
		var borrowText = ActionTooltip.Text((uint)BeastModeOf(row.Kin));
		return new FamiliarProfile(pet, row.TrickId, row.ReleaseId, row.TrickRadius, potency,
			TrickHeals: trickText.Contains("absorbed as beastmaster HP", StringComparison.Ordinal),
			ReleaseHeals: releaseText.Contains("Life Siphon", StringComparison.Ordinal),
			ReleaseProtects: Protects(releaseText, "nearby party members"),
			ReleaseRetreats: releaseText.Contains("retreats from combat", StringComparison.Ordinal),
			ReleaseStops: releaseText.Contains("with Stun", StringComparison.Ordinal)
				|| releaseText.Contains("with Petrification", StringComparison.Ordinal)
				|| releaseText.Contains("with Deep Freeze", StringComparison.Ordinal),
			ReleaseStopsByStun: releaseText.Contains("with Stun", StringComparison.Ordinal),
			ReleaseRadius: row.ReleaseRadius,
			ReleasePotency: ActionTooltip.TryStatedPotency(row.ReleaseId, out var release) ? release * HitsOf(releaseText) : 0f,
			ReleaseHurtsParty: releaseText.Contains("party members", StringComparison.Ordinal)
				&& releaseText.Contains("inflicts damage over time", StringComparison.Ordinal),
			BorrowProtects: Protects(borrowText, string.Empty),
			TrickHeart: HeartOf(trickText));
	}

	// How often an action strikes as its tooltip words it ("a sixfold slashing physical attack"), 1 when it says nothing.
	private static int HitsOf(string text)
	{
		for (var hits = Multiples.Length - 1; hits > 1; hits--)
		{
			if (text.Contains(Multiples[hits] + "fold", StringComparison.Ordinal))
			{
				return hits;
			}
		}

		return 1;
	}

	// The words of the tooltips by the number they stand for.
	private static readonly string[] Multiples = ["", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten"];

	// The heart of the affinity a tooltip states ("Instinctual Affinity: Eldritch"), 0 for none (K1, concept 01, A487).
	private static StatusID HeartOf(string text)
	{
		const string Marker = "Instinctual Affinity: ";
		var at = text.IndexOf(Marker, StringComparison.Ordinal);
		if (at < 0)
		{
			return 0;
		}

		var rest = text[(at + Marker.Length)..];
		return rest.StartsWith("Volant", StringComparison.Ordinal) ? StatusID.VolantHeart
			: rest.StartsWith("Rampant", StringComparison.Ordinal) ? StatusID.RampantHeart
			: rest.StartsWith("Durant", StringComparison.Ordinal) ? StatusID.DurantHeart
			: rest.StartsWith("Eldritch", StringComparison.Ordinal) ? StatusID.EldritchHeart
			: 0;
	}

	// What lowers the damage you take, as the effect texts word it: a lower vulnerability or damage taken, a barrier, or
	// enemies dealing less. For a Tempered Release only an effect on the party counts, not one on the enemies. Not a block
	// (Vileskin): how much a block takes off for a beastmaster is not known, and the rotation keeps it for a knockback.
	private static bool Protects(string text, string onParty)
	{
		if (text.Length == 0 || (onParty.Length > 0 && !text.Contains(onParty, StringComparison.Ordinal)))
		{
			return false;
		}

		return (text.Contains("vulnerability by", StringComparison.Ordinal) && text.Contains("reducing", StringComparison.Ordinal))
			|| GeneralDamageTaken(text)
			|| text.Contains("absorbs", StringComparison.Ordinal)
			|| text.Contains("Reduces damage dealt", StringComparison.Ordinal);
	}

	// C2 (concept 01, A491): a lower "damage taken" protects only when its clause names no element. Strut (earth) and
	// Formic Pheromones (water, earth and ice) keep nothing off physical or non-elemental damage, and no sheet says which
	// element an enemy uses; they counted as protection, so B2 could call one in a danger for nothing.
	private static readonly string[] Elements = ["fire", "ice", "wind", "earth", "lightning", "water"];

	private static bool GeneralDamageTaken(string text)
	{
		const string Taken = "damage taken";
		const string Reduction = "educ";
		var at = text.IndexOf(Taken, StringComparison.Ordinal);
		while (at >= 0)
		{
			var start = Math.Max(text.LastIndexOf(Reduction, at, StringComparison.Ordinal), 0);
			var clause = text[start..at];
			var named = false;
			foreach (var element in Elements)
			{
				named |= clause.Contains(element, StringComparison.OrdinalIgnoreCase);
			}

			if (!named)
			{
				return true;
			}

			at = text.IndexOf(Taken, at + Taken.Length, StringComparison.Ordinal);
		}

		return false;
	}

	// The action Beast Mode becomes for a kin's Kinship (Borrow's effect text).
	private static ActionID BeastModeOf(BeastmasterKinType kin) => kin switch
	{
		BeastmasterKinType.Beastkin => ActionID.BeastskinPvE,
		BeastmasterKinType.Vilekin => ActionID.VileskinPvE,
		BeastmasterKinType.Cloudkin => ActionID.CloudSkimPvE,
		BeastmasterKinType.Seedkin => ActionID.SeedsowerPvE,
		BeastmasterKinType.Wavekin => ActionID.QuellingWavePvE,
		BeastmasterKinType.Scalekin => ActionID.ScaleskinPvE,
		BeastmasterKinType.Soulkin => ActionID.SoulCrushPvE,
		BeastmasterKinType.Ashkin => ActionID.ScouringAshPvE,
		_ => 0,
	};

	// B5 (concept 01, "Belegung der Hörner in der Rotation", A520 to A522): the server command of the owner's
	// BestiaryTrace.log of 03.11.2026. 2420 assigns the three horns, with the familiars' numbers in the book, one per horn
	// (0 empties one): ten assignments, each shown by the slots 10 to 16 ms later. The server takes it out of combat in
	// every situation tried and in combat in none (04.11.2026, A521).
	private const int AssignBattlehornsCommand = 2420;

	private static byte[] _assignAwaited = [];
	private static byte[] _assignLast = [];
	private static DateTime _assignSentAt;

	private static bool SlotsShow(byte[] awaited)
	{
		var slots = CurrentPets;
		if (slots.Length < awaited.Length)
		{
			return false;
		}

		for (var i = 0; i < awaited.Length; i++)
		{
			if (slots[i] != awaited[i])
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// B5 of concept 01: puts <paramref name="first"/>, <paramref name="second"/> and <paramref name="third"/> on the three
	/// horns with the Bestiary's server command - taken out of combat only. <see cref="BattlehornAssignmentPending"/> then
	/// judges whether the server took it.
	/// </summary>
	public static void AssignBattlehorns(BeastmasterPet first, BeastmasterPet second, BeastmasterPet third)
	{
		_assignAwaited = [(byte)first, (byte)second, (byte)third];
		_assignLast = _assignAwaited;
		_assignSentAt = DateTime.Now;
		var answer = GameMain.ExecuteCommand(AssignBattlehornsCommand, (byte)first, (byte)second, (byte)third, 0);
		DefenseTrace.Line($"battlehorn assignment sent: {first}, {second}, {third} - the client answered {answer}");
	}

	/// <summary>
	/// Whether an assignment of <see cref="AssignBattlehorns"/> still waits for the slots: while it does, no horn is
	/// called, so a call never brings the familiar the horn held before. Judged on the way - shown within a GCD, the server
	/// took it; not shown, it did not, and the horns stay as they were.
	/// </summary>
	public static bool BattlehornAssignmentPending
	{
		get
		{
			if (_assignAwaited.Length == 0)
			{
				return false;
			}

			var waited = (DateTime.Now - _assignSentAt).TotalSeconds;
			var shown = SlotsShow(_assignAwaited);
			if (!shown && waited <= DataCenter.DefaultGCDTotal)
			{
				return true;
			}

			DefenseTrace.Line(shown
				? $"battlehorn assignment TAKEN after {waited:F3} s"
				: $"battlehorn assignment NOT TAKEN within {waited:F2} s - the horns stay as they were");
			_assignAwaited = [];
			return false;
		}
	}

	/// <summary>Whether the horns hold what RSR assigned last (B5): a change it did not send is the player's.</summary>
	public static bool HornsAsAssigned => _assignLast.Length > 0 && SlotsShow(_assignLast);

	/// <summary>
	/// The first enemy of the next fight, as far as RSR can see one: the nearest living enemy in its enemy list (in sight,
	/// within its range) that is not fighting yet. Null for none.
	/// </summary>
	public static IBattleChara? NextEnemy()
	{
		var player = Player;
		if (player == null)
		{
			return null;
		}

		IBattleChara? nearest = null;
		var distance = float.MaxValue;
		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile == null || hostile.IsDead || hostile.InCombat())
			{
				continue;
			}

			var away = Vector3.Distance(hostile.Position, player.Position);
			if (away < distance)
			{
				distance = away;
				nearest = hostile;
			}
		}

		return nearest;
	}

	/// <summary>
	/// The enemies not fighting yet within <paramref name="radius"/> of <paramref name="center"/>, it among them - what an
	/// area of that radius around it would reach, counted as <see cref="TrickValue"/> counts the enemies of a fight.
	/// </summary>
	public static int EnemiesWaitingAround(IBattleChara center, float radius)
	{
		var count = 0;
		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile != null && !hostile.IsDead && !hostile.InCombat()
				&& (hostile.GameObjectId == center.GameObjectId
					|| Vector3.Distance(hostile.Position, center.Position) - hostile.HitboxRadius <= radius))
			{
				count++;
			}
		}

		return count;
	}

	/// <summary>
	/// The living enemies fighting you, your party or your familiar now - not those another player fights beside you in
	/// the open world.
	/// </summary>
	public static int EnemiesFighting()
	{
		var player = Player;
		if (player == null)
		{
			return 0;
		}

		var count = 0;
		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile != null && !hostile.IsDead && hostile.InCombat()
				&& Svc.Objects.SearchById(hostile.TargetObjectId) is IBattleChara target
				&& (target.GameObjectId == player.GameObjectId || target.OwnerId == player.GameObjectId || target.IsParty()))
			{
				count++;
			}
		}

		return count;
	}

	/// <summary>Whether the Bestiary is open: the player may be setting the horns himself.</summary>
	public static unsafe bool BestiaryOpen
	{
		get
		{
			nint addon = Svc.GameGui.GetAddonByName(BestiaryAddon, 1);
			return addon != nint.Zero && ((FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitBase*)addon)->IsVisible;
		}
	}

	private const string BestiaryAddon = "XBMMonsterNotebook";
}
