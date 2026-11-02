using ECommons.DalamudServices;
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
/// <param name="BorrowProtects">The Beast Mode action Borrow unlocks for its kin lowers the damage you take.</param>
public sealed record FamiliarProfile(BeastmasterPet Pet, uint TrickId, uint ReleaseId, float TrickRadius, float TrickPotency,
	bool TrickHeals, bool ReleaseHeals, bool ReleaseProtects, bool BorrowProtects);

public partial class BeastmasterRotation
{
	// What the sheets say of a familiar regardless of level: the actions its Trick and Tempered Release execute, the reach of
	// the Trick, and its kin.
	private readonly record struct FamiliarRow(uint TrickId, uint ReleaseId, float TrickRadius, BeastmasterKinType Kin);

	// Built once from the sheets; null until then, empty when the familiar's Pet row cannot be told (B1).
	private static Dictionary<byte, FamiliarRow>? _familiarRows;
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
			+ $"{(profile.TrickHeals ? ", Trick heals" : string.Empty)}{(profile.ReleaseHeals ? ", Release heals" : string.Empty)}"
			+ $"{(profile.ReleaseProtects ? ", Release protects" : string.Empty)}{(profile.BorrowProtects ? ", Borrow protects" : string.Empty)}");
		return profile;
	}

	/// <summary>
	/// The familiar <paramref name="horn"/> holds, from the slots the game keeps (<see cref="CurrentPets"/>, one per horn in
	/// the order of <see cref="HornOrder"/>).
	/// </summary>
	public static BeastmasterPet PetOnHorn(HornOrder horn)
	{
		var pets = CurrentPets;
		var index = (int)horn - (int)HornOrder.FirstBattlehorn;
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

	// Where Pet.Abilities holds what a familiar's Trick and Tempered Release execute.
	private const int TrickAbility = 0;
	private const int ReleaseAbility = TrickAbility + 1;

	// B1: XBMPet leads to the familiar's Pet row through a column Lumina leaves unnamed. Of its two id columns, the one whose
	// values differ across all familiars and lead to Pet rows with abilities is it - the Pet ids do, the places they are
	// found at repeat. Neither or both: no catalog, and the configured order stands. Sheets only, no tooltip: cheap.
	private static Dictionary<byte, FamiliarRow> BuildFamiliarRows()
	{
		var catalog = new Dictionary<byte, FamiliarRow>();
		try
		{
			var xbm = Svc.Data.GetExcelSheet<XBMPet>();
			var pets = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Pet>();
			var actions = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>();
			if (xbm == null || pets == null || actions == null)
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

			bool LeadsToPets(Func<XBMPet, ushort> column)
			{
				var seen = new HashSet<ushort>();
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

			var fifth = LeadsToPets(row => row.Unknown5);
			var sixth = LeadsToPets(row => row.Unknown6);
			if (fifth == sixth)
			{
				_familiarCatalogState = $"NO CATALOG: the familiars' Pet column cannot be told ({(fifth ? "both" : "neither")} of two)";
				return catalog;
			}

			foreach (var row in rows)
			{
				var pet = pets.GetRow(fifth ? row.Unknown5 : row.Unknown6);
				var trick = pet.Abilities[TrickAbility].RowId;
				var radius = actions.TryGetRow(trick, out var trickRow) && trickRow.CastType > 1 ? trickRow.EffectRange : 0f;
				catalog[(byte)row.RowId] = new FamiliarRow(trick, pet.Abilities[ReleaseAbility].RowId, radius,
					(BeastmasterKinType)row.Unknown7);
			}

			_familiarCatalogState = $"{catalog.Count} familiars from the sheets (Pet column {(fifth ? 5 : 6)})";
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
			BorrowProtects: Protects(borrowText, string.Empty));
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
			|| text.Contains("damage taken", StringComparison.Ordinal)
			|| text.Contains("absorbs", StringComparison.Ordinal)
			|| text.Contains("Reduces damage dealt", StringComparison.Ordinal);
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
}
