using Dalamud.Game;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using EffectEntry = ECommons.Hooks.ActionEffectTypes.EffectEntry;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// The Crucible of the Unbroken as the game's sheets and the player's hits show it (concept 21, "Crucible of the Unbroken",
/// stage 1, A548, A549): the enemies of each fight with their weakness and stats, the fight in progress, the weakness each
/// action hits, and how much more a hit on a weakness deals - measured, since no source states it.
/// </summary>
/// <remarks>
/// <para>XBMBattleDetail has one row per fight and a subrow per enemy. EXDSchema (branch latest, 05.11.2026) orders its
/// columns Name, Icon, Action1, Action2, Resist, Strength, Intelligence, PhysResistance, MagResistance, Constitution,
/// Weakness; the built Lumina reads them as Unknown0 to Unknown10 (its named Experimental sheet is marked provisional). The
/// line on entering a board shows a probe.</para>
///
/// <para>Recognition only: the rules that read it - K2 holding a horn, B3 valuing a Trick, K3 ordering the targets - decide
/// what a figure is worth.</para>
/// </remarks>
internal static class CrucibleBoard
{
	/// <summary>
	/// One enemy of a fight: its name as the game's objects carry it, its weakness, its sheet ratings - strength and
	/// intelligence, and its physical and magic resistance grades (A582) - and its row of BNpcResist (K5, A589).
	/// </summary>
	internal readonly record struct Enemy(uint NameId, uint Weakness, byte Strength, byte Intelligence, byte PhysResistance,
		byte MagResistance, uint Resist = 0);

	/// <summary>
	/// The statuses BNpcResist states a resistance to, in its order (EXDSchema latest, 05.11.2026: ResistsSlow to
	/// ResistsDeath; the built Lumina reads them as Unknown0 to Unknown10). XBMPet's InflictsSlow to InflictsDeath use the
	/// same order (concept 21, K5, A589).
	/// </summary>
	internal enum Ailment
	{
		Slow,
		Petrify,
		Paralysis,
		Interrupt,
		Blind,
		Poison,
		Stun,
		Sleep,
		Bind,
		Heavy,
		Death,
	}

	/// <summary>
	/// K5 (A589): whether the enemy with <paramref name="nameId"/> in the fight in progress resists <paramref name="ailment"/>
	/// by the sheet. Null when the fight does not list it: then what the fight teaches (the learned stun immunity) decides
	/// alone.
	/// </summary>
	internal static bool? Resists(uint nameId, Ailment ailment)
	{
		return EnemyOf(nameId) is { } enemy ? Resists(enemy, ailment) : null;
	}

	/// <summary>
	/// K5 (A589): whether <paramref name="enemy"/> resists <paramref name="ailment"/> by its BNpcResist row. Null when the
	/// row cannot be read.
	/// </summary>
	internal static bool? Resists(Enemy enemy, Ailment ailment)
	{
		if (enemy.Resist == 0
			|| Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.BNpcResist>()?.GetRowOrDefault(enemy.Resist) is not { } row)
		{
			return null;
		}

		return ailment switch
		{
			Ailment.Slow => row.Unknown0,
			Ailment.Petrify => row.Unknown1,
			Ailment.Paralysis => row.Unknown2,
			Ailment.Interrupt => row.Unknown3,
			Ailment.Blind => row.Unknown4,
			Ailment.Poison => row.Unknown5,
			Ailment.Stun => row.Unknown6,
			Ailment.Sleep => row.Unknown7,
			Ailment.Bind => row.Unknown8,
			Ailment.Heavy => row.Unknown9,
			Ailment.Death => row.Unknown10,
			_ => null,
		};
	}

	private static List<(uint Row, Enemy[] Enemies)>? _fights;
	private static readonly Dictionary<uint, string> _elementNames = [];

	// K8 (A553): the final fight of each board, and per fight the dangerous statuses its enemies' actions inflict.
	private static readonly HashSet<uint> _finalFights = [];
	private static readonly Dictionary<uint, List<uint>> _boardFights = [];
	private static readonly Dictionary<uint, HashSet<uint>> _fightAilments = [];

	// K10 shop (A570): per fight the statuses its enemies' actions inflict that cost safety - the dangerous ones above, and
	// those that prevent actions only sometimes (Paralysis) or deal damage over time (Poison).
	private static readonly Dictionary<uint, HashSet<uint>> _fightHarms = [];
	private static readonly Dictionary<uint, HashSet<uint>> _actionElements = [];

	private static uint _fight;
	private static int _run;
	private static Enemy[] _enemies = [];
	private static readonly HashSet<uint> _atStart = [];
	private static readonly HashSet<uint> _present = [];
	private static readonly HashSet<uint> _seen = [];
	private static uint _tracedArea;

	// K1 (A549, A582): damage per potency of the player's and his familiars' hits on an enemy of the fight, kept apart for
	// hits on its weakness and on anything else, per action and resistance grade of the enemies hit (physical for slashing,
	// piercing and blunt hits, else magic), in this area; and in this fight the damage of that side in all, and of the hits
	// on each weakness.
	private readonly record struct Sums(double Weak, int WeakHits, double Other, int OtherHits);

	// A582: the effect entry's second parameter carries the hit's aspect in its upper half and its attack type in its lower
	// (ECommons EffectEntry.AttackType; the third byte of ACT's effect flags) - the game's layout of a hit, not a tuned value.
	private const int AspectShift = 4;

	// A582: the weakness bonus measured in his logs of 04 and 05.11.2026 - 18 pairs of enemies of one resistance grade on the
	// first board, weighted 1.271 -, until this board measures its own. A fallback from measurement, not a game value.
	private const float WeaknessBonusFallback = 0.27f;
	private static readonly Dictionary<byte, HashSet<uint>> _hitElements = [];

	// A586: the familiars' damage by source since RSR loaded - auto-attack, Trick, Tempered Release, the rest (Parting Blow's
	// Aetheric Burst) -, and the shares measured on the first board in his logs of 04 and 05.11.2026 (74,200 damage of all
	// familiars: 32, 18, 19 and 32 %) until every source has hit.
	private static double _familiarAuto, _familiarTrick, _familiarRelease, _familiarOther;
	private static readonly (float Auto, float Trick, float Release, float Parting) FamiliarSharesFallback = (0.32f, 0.18f, 0.19f, 0.32f);
	private static readonly Dictionary<uint, HashSet<uint>> _actionElementsSeen = [];

	/// <summary>K10 shop (A570): what one fight cost the player - damage by how it meets his defences - and what he healed himself.</summary>
	internal readonly record struct FightLoad(double Physical, double Magic, double Other, double Healed)
	{
		/// <summary>All the damage of the fight.</summary>
		internal double Damage => Physical + Magic + Other;
	}

	private static readonly Dictionary<(uint Action, bool Physical, byte Grade), Sums> _perPotency = [];

	// A672: the most HP each enemy kind (name) was seen with, over the session; the damage per potency of the player's damage
	// items in this area.
	private static readonly Dictionary<uint, uint> _maxHpSeen = [];
	private static (double Sum, int Hits) _itemPerPotency;
	private static uint _itemMeasuredIn;
	private static uint _measuredIn;
	private static double _ownDamage;
	private static readonly Dictionary<uint, double> _ownDamageByElement = [];
	private static bool _bonusTraced;

	// K10 shop (A569, A570): the load of each fight on the player - the damage he took by how it meets his defences, and
	// his own healing on himself - in this area and over every board since RSR loaded; and the fights between campsites.
	private static FightLoad _fightLoad;
	private static uint _areaLoadsIn;
	private static (int Fights, FightLoad Sum, FightLoad Heaviest) _areaLoads;
	private static (int Fights, FightLoad Sum, FightLoad Heaviest) _allLoads;
	private static int _stretchFights;
	private static (int Stretches, int Fights) _stretches;

	// A668: the hits on the player in the fight in progress - seconds since its first hit, damage, kind -, and those of the
	// heaviest fight in this area and over every board, kept with the loads above, for the share a protection catches.
	private static readonly List<Hit> _fightHits = [];
	private static DateTime _fightFirstHit;
	private static List<Hit> _areaHeaviestHits = [];
	private static List<Hit> _allHeaviestHits = [];

	/// <summary>A668: how a hit on the player meets his defences, by the attack type of its action.</summary>
	internal enum HitKind : byte
	{
		Physical,
		Magic,
		Other,
	}

	private readonly record struct Hit(double At, float Amount, HitKind Kind);

	private static List<(uint Row, Enemy[] Enemies)> Fights => _fights ??= Read();

	/// <summary>The fight in progress: its XBMBattleDetail row, 0 while none is told.</summary>
	internal static uint Fight => _fight;

	/// <summary>
	/// The run on a board (A662): counted up each time the player enters one. A new run on the same board is the same area,
	/// so what holds for one run - the gear worn, the feed eaten - is kept by run, not by area.
	/// </summary>
	internal static int Run => _run;

	// A661: the unit of the learned loads per fight (CrucibleFightLoads.json) - whole percent of the player's maximum HP, as
	// the window shows them; a choice of unit, not a figure of the game.
	internal const int PercentScale = 100;

	/// <summary>
	/// K10 Campsite (A661): the heaviest damage the player took in <paramref name="battle"/> (an XBMBattleDetail row), as a
	/// share of his maximum HP without his Crucible gear, over every session; null while it was never fought with RSR.
	/// </summary>
	internal static float? BattleLoad(uint battle)
	{
		return LearnedLoad(battle);
	}

	/// <summary>
	/// K8 step 13 (A676): what the fight in progress can still cost the player - its learned load less what it cost him so
	/// far, both as a share of his maximum HP without his gear; null while the fight has no learned load, and once it cost
	/// him more than that: then it is heavier than any before, and what is left is unknown.
	/// </summary>
	internal static float? LoadLeft()
	{
		if (_fight == 0 || LearnedLoad(_fight) is not { } load || Player.Object is not { MaxHp: > 0u } player)
		{
			return null;
		}

		var left = load - BareShare(_fightLoad, player.MaxHp);
		return left > 0f ? left : null;
	}

	// A661: a fight's damage taken back to the player without his gear - his maximum HP, the damage he takes, his physical
	// and magic vulnerability and evasion, as the shop weighs them.
	private static float BareShare(FightLoad load, uint maxHp)
	{
		var (hp, taken, physical, magic) = Rotations.Basic.BeastmasterRotation.WornFactors();
		var bare = maxHp / Math.Max(hp, float.Epsilon);
		return (float)(((load.Physical / Math.Max(physical, float.Epsilon)) + (load.Magic / Math.Max(magic, float.Epsilon))
			+ load.Other) / Math.Max(taken, float.Epsilon) / bare);
	}

	private static uint _fightStartHp;
	private static uint _fightPrevHp;
	private static long _fightLowHp;
	private static long _fightLastHp;
	private static long _fightOwnPending;
	private static long _fightOwnApplied;
	private static DateTime _fightOwnReported = DateTime.MinValue;

	// A706, A709 (K15): the player's HP when the fight is told, and since then his HP without his own healing - items and
	// his actions, taken off the moment his HP rise by it -, the lowest and the last, for the fight's net loss and deepest
	// dip. A familiar's healing stays in.
	private static void FollowPlayerHp()
	{
		if (_fight == 0 || Player.Object is not { MaxHp: > 0u } player)
		{
			_fightStartHp = 0;
			_fightOwnPending = 0;
			_fightOwnApplied = 0;
			return;
		}

		var hp = player.CurrentHp;
		if (_fightStartHp == 0)
		{
			_fightStartHp = Math.Max(1u, hp);
			_fightPrevHp = hp;
			_fightLowHp = hp;
			_fightOwnApplied = 0;
		}

		// A720: what did not show as a rise within a GCD of its report - a hit at the same moment took it - is gone; kept,
		// it took a familiar's later healing for his own.
		if (_fightOwnPending > 0 && (DateTime.Now - _fightOwnReported).TotalSeconds > DataCenter.DefaultGCDTotal)
		{
			_fightOwnPending = 0;
		}

		// His own healing shows as a rise of his HP after its effect was reported; what a full bar cannot take is gone.
		if (hp > _fightPrevHp && _fightOwnPending > 0)
		{
			var own = Math.Min(hp - _fightPrevHp, _fightOwnPending);
			_fightOwnApplied += own;
			_fightOwnPending -= own;
		}

		if (hp >= player.MaxHp)
		{
			_fightOwnPending = 0;
		}

		_fightPrevHp = hp;
		_fightLastHp = hp - _fightOwnApplied;
		_fightLowHp = Math.Min(_fightLowHp, _fightLastHp);
	}

	// A706, A709 (K15): the fight's net loss (start less end) and its deepest dip below the start, both without his own
	// healing, into the learned heaviest, in whole percent of his HP without gear.
	private static void LearnNetAndDip()
	{
		if (_fightStartHp == 0 || Player.Object is not { MaxHp: > 0u } player)
		{
			return;
		}

		var end = player.IsDead ? -_fightOwnApplied : _fightLastHp;
		var net = Math.Max(0L, _fightStartHp - end);
		var dip = Math.Max(0L, _fightStartHp - Math.Min(_fightLowHp, end));
		var netPercent = (int)Math.Ceiling(BareShare(new FightLoad(0d, 0d, net, 0d), player.MaxHp) * PercentScale);
		var dipPercent = (int)Math.Ceiling(BareShare(new FightLoad(0d, 0d, dip, 0d), player.MaxHp) * PercentScale);
		var changed = false;
		lock (OtherConfiguration.CrucibleFightNetGate)
		{
			if (!OtherConfiguration.CrucibleFightNet.TryGetValue(_fight, out var knownNet) || knownNet < netPercent)
			{
				OtherConfiguration.CrucibleFightNet[_fight] = netPercent;
				changed = true;
			}

			var dipKey = _fight * FamiliarKeyStride;
			if (!OtherConfiguration.CrucibleFightNet.TryGetValue(dipKey, out var knownDip) || knownDip < dipPercent)
			{
				OtherConfiguration.CrucibleFightNet[dipKey] = dipPercent;
				changed = true;
			}
		}

		if (changed)
		{
			DefenseTrace.Line($"crucible fight {_fight}: net loss {netPercent} %, deepest dip {dipPercent} % of your HP without gear, "
				+ $"your own healing of {_fightOwnApplied} HP taken off - the heaviest so far stored");
			_ = OtherConfiguration.SaveCrucibleFightNet();
		}
	}

	/// <summary>K15 (A706, A709): the most HP the player lost in <paramref name="battle"/> from its start to its end without his own healing, as a share of his HP without gear; null before it was measured.</summary>
	internal static float? NetLoad(uint battle)
	{
		lock (OtherConfiguration.CrucibleFightNetGate)
		{
			return OtherConfiguration.CrucibleFightNet.TryGetValue(battle, out var percent) ? percent / (float)PercentScale : null;
		}
	}

	/// <summary>K15 (A706, A709): the deepest <paramref name="battle"/> took the player below its start without his own healing, as a share of his HP without gear; null before it was measured.</summary>
	internal static float? DipLoad(uint battle)
	{
		lock (OtherConfiguration.CrucibleFightNetGate)
		{
			return OtherConfiguration.CrucibleFightNet.TryGetValue(battle * FamiliarKeyStride, out var percent)
				? percent / (float)PercentScale
				: null;
		}
	}

	private static float? LearnedLoad(uint battle)
	{
		lock (OtherConfiguration.CrucibleFightLoadsGate)
		{
			return OtherConfiguration.CrucibleFightLoads.TryGetValue(battle, out var percent) ? percent / (float)PercentScale : null;
		}
	}

	/// <summary>The enemies of the fight in progress, as the sheet lists them.</summary>
	internal static Enemy[] Enemies => _enemies;

	/// <summary>Whether an enemy of this name was here when the fight was told.</summary>
	internal static bool WasThereAtStart(uint nameId) => _atStart.Contains(nameId);

	/// <summary>Whether an enemy of this name is here now.</summary>
	internal static bool IsHere(uint nameId) => _present.Contains(nameId);

	/// <summary>
	/// Whether an enemy of this name, listed for the fight in progress, has not been seen in it yet - it is still to come. One
	/// that came and fell does not come again (A553; before, it counted as still to come once it was gone).
	/// </summary>
	internal static bool IsToCome(uint nameId) => _fight != 0 && !_seen.Contains(nameId);

	// A581: the battle the horn screen named last, until a fight is told - so K2 knows the enemies of a fight at its pull,
	// before combat tells it (on 05.11.2026 the Raptor came against the Seelenlose at the pull, 55 s before the fire-weak adds).
	private static uint _upcoming;

	/// <summary>A581: the battle the horn screen named last, while no fight is told; 0 otherwise.</summary>
	internal static uint Upcoming => _fight == 0 ? _upcoming : 0;

	/// <summary>
	/// A581: whether an enemy named <paramref name="nameId"/> stands in the hostile list now - before a fight is told, at its
	/// pull. Read only while the list is built in this frame.
	/// </summary>
	internal static bool PresentNow(uint nameId)
	{
		if (!DataCenter.HostileListLive)
		{
			return false;
		}

		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile != null && !hostile.IsDead && hostile.NameId == nameId)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Whether the fight in progress is the board's final one, against its boss: subrow 0 of the board's row in
	/// XBMContentBattle (A553). The board's items are lost after the run.
	/// </summary>
	internal static bool IsFinalFight => _fight != 0 && _finalFights.Contains(_fight);

	/// <summary>
	/// The dangerous statuses the enemies of the fight in progress inflict, as the sheet lists their two actions (A553):
	/// detrimental, and taking control from their target (it cannot act or does not act on its own will) or ending in death
	/// by their English effect text. Empty while no fight is told.
	/// </summary>
	internal static IReadOnlyCollection<uint> Ailments => _fight != 0 && _fightAilments.TryGetValue(_fight, out var ailments)
		? ailments
		: [];

	/// <summary>
	/// K10 shop (A570): the statuses the fight in XBMBattleDetail row <paramref name="battleRow"/> inflicts that cost safety:
	/// the dangerous ones, and those that prevent actions only sometimes or deal damage over time.
	/// </summary>
	internal static IReadOnlyCollection<uint> HarmsOf(uint battleRow)
	{
		_ = Fights;
		return _fightHarms.TryGetValue(battleRow, out var harms) ? harms : [];
	}

	/// <summary>K10 shop (A569): every fight the sheets list, for when the board is not known.</summary>
	internal static List<uint> AllFights()
	{
		var rows = new List<uint>();
		foreach (var (row, _) in Fights)
		{
			rows.Add(row);
		}

		return rows;
	}

	/// <summary>The enemies the sheet lists for the fight in XBMBattleDetail row <paramref name="battleRow"/>.</summary>
	internal static Enemy[] EnemiesOf(uint battleRow)
	{
		foreach (var (row, enemies) in Fights)
		{
			if (row == battleRow)
			{
				return enemies;
			}
		}

		return [];
	}

	/// <summary>K7 (A563): the fights of a board (XBMContentBattle row = XBMContent row), its final fight first.</summary>
	internal static IReadOnlyList<uint> FightsOfBoard(uint board)
	{
		_ = Fights;
		return _boardFights.TryGetValue(board, out var fights) ? fights : [];
	}

	/// <summary>K7 (A563): how many familiars a board's team holds - XBMContent's TeamSize (EXDSchema latest: after the 33
	/// bonus points and ContentFinderCondition, Lumina's Unknown34); 0 when the row is missing.</summary>
	internal static int TeamSize(uint board)
	{
		return Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.XBMContent>()?.GetRowOrDefault(board) is { } content
			? content.Unknown34
			: 0;
	}

	/// <summary>
	/// K15 (A695): rows of XBMScoreBonus that RSR's rules read; the row is also the index into XBMContent's BonusPoints.
	/// </summary>
	internal enum ScoreBonus : byte
	{
		/// <summary>"In Unterzahl I" (Beast Squad I): a team of the sizes its condition names (6 or 7).</summary>
		SquadI = 4,

		/// <summary>"In Unterzahl II" (Beast Squad II): 4 or 5.</summary>
		SquadII = 5,

		/// <summary>"In Unterzahl III" (Beast Squad III): 1 to 3.</summary>
		SquadIII = 6,

		/// <summary>"Unversehrter Bändiger" (Self-preservation): the player at 100 % HP.</summary>
		SelfPreservation = 7,

		/// <summary>"Unversehrte Bestien" (Selfless): every familiar at 100 % HP.</summary>
		Selfless = 8,

		/// <summary>"Sparsamkeit" (Economic Ethos): no crucible item used.</summary>
		EconomicEthos = 15,

		/// <summary>"Sammelleidenschaft" (Habitual Hoarder): ten crucible items held.</summary>
		HabitualHoarder = 16,
	}

	private static uint _boardContent;
	private static uint _board;

	/// <summary>
	/// K15 (A695): the XBMContent row of the board the player is on - the row whose ContentFinderCondition is the current
	/// one; 0 outside the Crucible.
	/// </summary>
	internal static uint CurrentBoard
	{
		get
		{
			var content = DataCenter.IsInCrucible ? DataCenter.Territory?.ContentFinderConditionId ?? 0 : 0;
			if (content == _boardContent)
			{
				return _board;
			}

			_boardContent = content;
			_board = 0;
			var sheet = content == 0 ? null : Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.XBMContent>();
			if (sheet != null)
			{
				foreach (var row in sheet)
				{
					if (row.Unknown33 == content)
					{
						_board = row.RowId;
						break;
					}
				}
			}

			return _board;
		}
	}

	/// <summary>
	/// K15 (A695): the points the board the player is on grants for <paramref name="bonus"/> - XBMContent's BonusPoints, one
	/// per XBMScoreBonus row and the sheet's first 33 columns (EXDSchema latest; Lumina's Experimental sheet names them);
	/// 0 where the board grants none, outside the Crucible, or without the row.
	/// </summary>
	internal static int BonusPoints(ScoreBonus bonus) => BonusPoints((uint)bonus);

	/// <summary>
	/// K15 (A699): the points the board the player is on grants for the XBMScoreBonus row <paramref name="bonusRow"/> - by
	/// the row, for bonuses RSR finds by their condition (the kin of "a team of only beastkin").
	/// </summary>
	internal static int BonusPoints(uint bonusRow)
	{
		var board = CurrentBoard;
#pragma warning disable PendingExcelSchema // the sheet's named BonusPoints; the regular sheet holds them as Unknown0 to Unknown32
		return board != 0
			&& Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Experimental.XBMContent>()?.GetRowOrDefault(board) is { } row
			&& bonusRow < row.BonusPoints.Count
			? row.BonusPoints[(int)bonusRow]
			: 0;
#pragma warning restore PendingExcelSchema
	}

	// XBMContent's recommended rank is stored two above what the board screen shows: "Take this value subtract 2. It is how
	// the exe is doing it." (EXDSchema latest, XBMContent.RecommendedRank); board 2 stores 8 and shows "Recommended Beast
	// Rank: 6 (Sync from 10)" (Daedalus 74659a6, field note). The game's own definition, not a tuned value (A579).
	private const int RecommendedRankStored = 2;

	/// <summary>
	/// K7 (A579): the beast rank the game recommends for a board - XBMContent's RecommendedRank (Lumina's Unknown35) as the
	/// board screen shows it; 0 when the row is missing.
	/// </summary>
	internal static int RecommendedRank(uint board)
	{
		return Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.XBMContent>()?.GetRowOrDefault(board) is { } content
			? Math.Max(1, content.Unknown35 - RecommendedRankStored)
			: 0;
	}

	/// <summary>
	/// K7 (A579): the highest beast rank - the highest board's sync rank (XBMContent's SyncRank, Lumina's Unknown36: 5, 10,
	/// 15, 20, 25). A familiar there gains nothing more from a run.
	/// </summary>
	internal static int MaxRank
	{
		get
		{
			if (_maxRank > 0)
			{
				return _maxRank;
			}

			var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.XBMContent>();
			if (sheet != null)
			{
				foreach (var row in sheet)
				{
					_maxRank = Math.Max(_maxRank, (int)row.Unknown36);
				}
			}

			return _maxRank;
		}
	}

	private static int _maxRank;

	/// <summary>Whether XBMBattleDetail row <paramref name="battleRow"/> is a board's final fight (A553).</summary>
	internal static bool IsFinalBattle(uint battleRow)
	{
		_ = Fights;
		return _finalFights.Contains(battleRow);
	}

	/// <summary>The XBMElement row whose English name is <paramref name="name"/>, ignoring case; 0 when none is.</summary>
	internal static uint ElementNamed(string name)
	{
		_ = Fights;
		foreach (var (row, element) in _elementNames)
		{
			if (string.Equals(element, name, StringComparison.OrdinalIgnoreCase))
			{
				return row;
			}
		}

		return 0;
	}

	private static List<(uint Row, Enemy[] Enemies)> Read()
	{
		var fights = new List<(uint Row, Enemy[] Enemies)>();
		var elements = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.XBMElement>(ClientLanguage.English);
		if (elements != null)
		{
			foreach (var element in elements)
			{
				var name = element.Unknown0.ExtractText().Trim();
				if (name.Length > 0)
				{
					_elementNames[element.RowId] = name;
				}
			}
		}

		var sheet = Svc.Data.GetSubrowExcelSheet<Lumina.Excel.Sheets.XBMBattleDetail>();
		if (sheet == null)
		{
			return fights;
		}

		var actions = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.XBMBattleDetailAction>();
		var statuses = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(ClientLanguage.English);
		var enemies = new List<Enemy>();
		foreach (var row in sheet)
		{
			enemies.Clear();
			var ailments = new HashSet<uint>();
			var harms = new HashSet<uint>();
			foreach (var enemy in row)
			{
				if (enemy.Unknown0 != 0)
				{
					enemies.Add(new Enemy(enemy.Unknown0, enemy.Unknown10, enemy.Unknown5, enemy.Unknown6, enemy.Unknown7,
						enemy.Unknown8, enemy.Unknown4));
					AddAilment(enemy.Unknown2);
					AddAilment(enemy.Unknown3);
				}
			}

			if (enemies.Count > 0)
			{
				fights.Add((row.RowId, enemies.ToArray()));
				if (ailments.Count > 0)
				{
					_fightAilments[row.RowId] = ailments;
				}

				if (harms.Count > 0)
				{
					_fightHarms[row.RowId] = harms;
				}
			}

			// XBMBattleDetailAction: Action, Status, ActionTarget, ActionEffectType (EXDSchema latest, 05.11.2026).
			void AddAilment(ushort action)
			{
				if (action == 0 || actions?.GetRowOrDefault(action) is not { } detail || detail.Unknown1 == 0
					|| statuses?.GetRowOrDefault(detail.Unknown1) is not { } status)
				{
					return;
				}

				if (IsDangerous(status))
				{
					_ = ailments.Add(detail.Unknown1);
					_ = harms.Add(detail.Unknown1);
				}
				else if (CostsSafety(status))
				{
					_ = harms.Add(detail.Unknown1);
				}
			}
		}

		// XBMContentBattle: one row per board, a subrow per fight naming its XBMBattleDetail row; subrow 0 is the boss.
		var boards = Svc.Data.GetSubrowExcelSheet<Lumina.Excel.Sheets.XBMContentBattle>();
		if (boards != null)
		{
			foreach (var board in boards)
			{
				var ofBoard = new List<uint>();
				foreach (var fight in board)
				{
					if (fight.Unknown0 == 0)
					{
						continue;
					}

					ofBoard.Add(fight.Unknown0);
					if (fight.SubrowId == 0)
					{
						_ = _finalFights.Add(fight.Unknown0);
					}
				}

				_boardFights[board.RowId] = ofBoard;
			}
		}

		return fights;
	}

	// Detrimental, and it takes control from its bearer or its English effect text names death (Doom, Stone Curse).
	private static bool IsDangerous(Lumina.Excel.Sheets.Status status)
	{
		return status.StatusCategory == (byte)StatusCategory.Detrimental
			&& (status.LockActions || status.LockControl
				|| status.Description.ExtractText().Contains("death", StringComparison.OrdinalIgnoreCase));
	}

	// A570: detrimental, and by its English effect text it prevents actions, if only sometimes (Paralysis: "sometimes
	// preventing the execution of actions"), or deals damage over time (Poison: "causing damage over time"). Blind, which
	// only lowers accuracy, is neither.
	private static bool CostsSafety(Lumina.Excel.Sheets.Status status)
	{
		if (status.StatusCategory != (byte)StatusCategory.Detrimental)
		{
			return false;
		}

		var text = status.Description.ExtractText();
		return text.Contains("execution of actions", StringComparison.OrdinalIgnoreCase)
			|| text.Contains("damage over time", StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>A718: counted up each time a hit teaches an action's elements anew - for whoever keeps what it read of them.</summary>
	internal static int ElementsLearned { get; private set; }

	/// <summary>
	/// The weakness (XBMElement row) an action hits: once a hit of it was seen in a Crucible fight, what that hit carried
	/// (A582); before, by the action sheet - its element (Fire to Water), else its physical attack type (Slashing, Piercing,
	/// Blunt), matched by the English names; none for an action without either.
	/// </summary>
	internal static HashSet<uint> ElementsOf(uint actionId)
	{
		// A582: what a hit of the action carried, once one was seen - the sheet gives the beastmaster's axes no attack type,
		// though they strike as slashing.
		if (_actionElementsSeen.TryGetValue(actionId, out var seen))
		{
			return seen;
		}

		if (_actionElements.TryGetValue(actionId, out var known))
		{
			return known;
		}

		_ = Fights;
		var hits = new HashSet<uint>();
		if (actionId != 0 && Service.GetSheet<Lumina.Excel.Sheets.Action>()?.GetRowOrDefault(actionId) is { } action)
		{
			var aspect = (Aspect)action.Aspect;
			var name = aspect is >= Aspect.Fire and <= Aspect.Water
				? aspect.ToString()
				: action.AttackType.RowId is >= (uint)AttackType.Slashing and <= (uint)AttackType.Blunt
					? ((AttackType)action.AttackType.RowId).ToString()
					: string.Empty;
			foreach (var (row, element) in _elementNames)
			{
				if (name.Length > 0 && string.Equals(element, name, StringComparison.OrdinalIgnoreCase))
				{
					_ = hits.Add(row);
				}
			}
		}

		_actionElements[actionId] = hits;
		return hits;
	}

	/// <summary>The weakness of an enemy of the fight in progress, 0 when none or not in the fight.</summary>
	internal static uint WeaknessOf(uint nameId)
	{
		foreach (var enemy in _enemies)
		{
			if (enemy.NameId == nameId)
			{
				return enemy.Weakness;
			}
		}

		return 0;
	}

	/// <summary>K4 (A587): whether any of <paramref name="enemies"/> is a kind seen casting interruptibly on a board.</summary>
	internal static bool CastsInterruptibly(Enemy[] enemies)
	{
		lock (OtherConfiguration.InterruptibleKindsGate)
		{
			foreach (var enemy in enemies)
			{
				if (OtherConfiguration.InterruptibleCasterKinds.ContainsKey(enemy.NameId))
				{
					return true;
				}
			}
		}

		return false;
	}

	// K4 (A587): the kinds already counted in this fight.
	private static readonly HashSet<uint> _interruptibleNoted = [];
	private static uint _interruptibleFight;

	/// <summary>
	/// K4 (A587): an enemy of the fight casting a spell the game marks interruptible - its kind is stored once per fight,
	/// with a trace line. Called every frame with the frame's hostile list.
	/// </summary>
	internal static void NoteInterruptibleCasts()
	{
		if (_fight == 0 || !DataCenter.HostileListLive)
		{
			return;
		}

		if (_interruptibleFight != _fight)
		{
			_interruptibleFight = _fight;
			_interruptibleNoted.Clear();
		}

		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile == null || !hostile.IsCasting || !hostile.IsCastInterruptible || hostile.NameId == 0
				|| !_interruptibleNoted.Add(hostile.NameId))
			{
				continue;
			}

			lock (OtherConfiguration.InterruptibleKindsGate)
			{
				OtherConfiguration.InterruptibleCasterKinds[hostile.NameId] = OtherConfiguration.InterruptibleCasterKinds.GetValueOrDefault(hostile.NameId) + 1;
			}

			_ = OtherConfiguration.SaveInterruptibleCasterKinds();
			DefenseTrace.Line($"crucible: {hostile.Name.TextValue} (kind {hostile.NameId}) casts interruptibly ({hostile.CastActionId}) - stored for the horns and the team");
		}
	}

	/// <summary>
	/// K3 before the first measurement (A549): the larger of the sheet's Strength and Intelligence of an enemy of the fight
	/// in progress, 0 when it is not listed.
	/// </summary>
	internal static float SheetDanger(uint nameId)
	{
		foreach (var enemy in _enemies)
		{
			if (enemy.NameId == nameId)
			{
				return Math.Max(enemy.Strength, enemy.Intelligence);
			}
		}

		return 0f;
	}

	/// <summary>
	/// Follows the Crucible every frame: on entering a board, whether the sheets were read; in combat, which fight is in
	/// progress and who is there.
	/// </summary>
	internal static void Follow()
	{
		FollowKillStacks();
		FollowPlayerHp();
		if (!DataCenter.IsInCrucible)
		{
			EndFight();
			EndStretch();
			_tracedArea = 0;
			_upcoming = 0;
			CrucibleRoute.Reset();
			return;
		}

		var fights = Fights;
		if (_tracedArea != DataCenter.TerritoryID)
		{
			EndFight();
			EndStretch();
			_run++;
			_upcoming = 0;
			CrucibleRoute.Reset();
			_tracedArea = DataCenter.TerritoryID;
			var count = 0;
			foreach (var (_, enemies) in fights)
			{
				count += enemies.Length;
			}

			var probe = fights.Count > 0 ? fights[0].Enemies[0] : default;
			DefenseTrace.Line(fights.Count == 0 || _elementNames.Count == 0
				? "crucible: no fights read from the sheets - the rules reading them stay idle"
				: $"crucible: {fights.Count} fights with {count} enemies read from the sheets, {_elementNames.Count} weaknesses; "
					+ $"first enemy {EnemyName(probe.NameId)}, weak to {ElementName(probe.Weakness)}, strength {probe.Strength}, "
					+ $"intelligence {probe.Intelligence}");
			TraceTeamAndField("entering the board");
		}

		FollowHornScreen();
		if (!DataCenter.InCombat)
		{
			EndFight();
			return;
		}

		_present.Clear();
		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile != null && !hostile.IsDead && hostile.NameId != 0)
			{
				_ = _present.Add(hostile.NameId);
			}
		}

		if (_fight != 0)
		{
			_seen.UnionWith(_present);
			return;
		}

		if (_present.Count == 0)
		{
			return;
		}

		// The row naming most of the enemies here, the one with the fewest if several do: a row naming all of them wins, and
		// an enemy the sheet does not list (a helper with a name of its own) does not hide the fight.
		var bestOverlap = 0;
		foreach (var (row, enemies) in fights)
		{
			var overlap = 0;
			foreach (var present in _present)
			{
				foreach (var enemy in enemies)
				{
					if (enemy.NameId == present)
					{
						overlap++;
						break;
					}
				}
			}

			if (overlap > bestOverlap || (overlap == bestOverlap && overlap > 0 && enemies.Length < _enemies.Length))
			{
				bestOverlap = overlap;
				_fight = row;
				_enemies = enemies;
				_upcoming = 0;
			}
		}

		if (_fight == 0)
		{
			return;
		}

		_atStart.UnionWith(_present);
		_seen.UnionWith(_present);
		var coming = new List<string>();
		foreach (var enemy in _enemies)
		{
			if (!_atStart.Contains(enemy.NameId))
			{
				coming.Add($"{EnemyName(enemy.NameId)} ({ElementName(enemy.Weakness)})");
			}
		}

		DefenseTrace.Line($"crucible fight {_fight}: {bestOverlap} of {_present.Count} kind(s) here listed, "
			+ (coming.Count == 0 ? "nobody to come" : $"to come: {string.Join(", ", coming)}"));
		TraceTeamAndField($"fight {_fight} told");
	}

	/// <summary>
	/// K11 probe (A556, A561): what the game's agents hold on a board - the team with each familiar's HP and the horns chosen
	/// (<c>AgentXBMPetParty</c>: <c>SelectedPets</c> is the team in screen order with <c>PetCurrentHealth</c> and
	/// <c>PetMaxHealth</c> per position, <c>SelectedPetIds</c> the chosen positions - as Daedalus 74659a6 reads them on the
	/// horn screen, mode 2) and the board's fields (<c>AgentXBMStageDetailList</c>: its battle entries, at horn time the
	/// battle about to start). Written on entering a board, when the horn screen opens, and when a fight is told and over.
	/// Fields only, no calls.
	/// </summary>
	private static unsafe void TraceTeamAndField(string when)
	{
		var team = "no team agent";
		var party = AgentXBMPetParty.Instance();
		if (party != null)
		{
			var pets = party->SelectedPets.AsSpan();
			var current = party->PetCurrentHealth;
			var max = party->PetMaxHealth;
			var members = new List<string>();
			for (var i = 0; i < pets.Length; i++)
			{
				var hp = i < current.Length && i < max.Length ? $" {current[i]}/{max[i]}" : string.Empty;
				members.Add($"{(pets[i].PetId <= byte.MaxValue ? ((Rotations.Basic.BeastmasterPet)pets[i].PetId).ToString() : "#" + pets[i].PetId)}{hp}");
			}

			var horns = new List<uint>();
			foreach (var chosen in party->SelectedPetIds.AsSpan())
			{
				horns.Add(chosen.PetId);
			}

			team = $"team mode {party->Mode}, content {party->ContentId}, {pets.Length} in the team: {string.Join(", ", members)}; "
				+ $"horn positions {string.Join(" ", horns)}; {party->CandidatePets.Count} candidates";
		}

		var field = "no field agent";
		var stages = AgentXBMStageDetailList.Instance();
		if (stages != null)
		{
			var entries = stages->Entries;
			var stageRows = 0;
			var battles = new List<uint>();
			for (var i = 0; i < entries.Length; i++)
			{
				ref readonly var entry = ref entries[i];
				if (entry.EntryType == BattleEntryKind && entry.BattleDetailId != 0)
				{
					if (!battles.Contains(entry.BattleDetailId))
					{
						battles.Add(entry.BattleDetailId);
					}
				}
				else if (entry.StageEventId != 0 || entry.EntryType != 0)
				{
					stageRows++;
				}
			}

			var selected = new List<int>();
			var selection = stages->EntrySelection;
			for (var i = 0; i < selection.Length; i++)
			{
				if (selection[i] != 0)
				{
					selected.Add(i);
				}
			}

			field = $"field mode {stages->Mode}, content {stages->ContentId}, built {stages->AddonInitState}, {stageRows} stage rows, "
				+ $"battles {string.Join(" ", battles)}, selected {string.Join(" ", selected)}";
		}

		DefenseTrace.Line($"crucible agents ({when}): {team} | {field} | {StateAndConditions()}");
	}

	// A576: whether RSR was on and which of the game's states that turn it off or end its update were set - his report of
	// 05.11.2026 that turning RSR on at the horn screen did not stay.
	private static string StateAndConditions()
	{
		var flags = new List<string>();
		foreach (var flag in (ReadOnlySpan<Dalamud.Game.ClientState.Conditions.ConditionFlag>)[
			Dalamud.Game.ClientState.Conditions.ConditionFlag.BetweenAreas,
			Dalamud.Game.ClientState.Conditions.ConditionFlag.BetweenAreas51,
			Dalamud.Game.ClientState.Conditions.ConditionFlag.OccupiedInCutSceneEvent,
			Dalamud.Game.ClientState.Conditions.ConditionFlag.WatchingCutscene,
			Dalamud.Game.ClientState.Conditions.ConditionFlag.OccupiedInEvent,
			Dalamud.Game.ClientState.Conditions.ConditionFlag.OccupiedInQuestEvent,
			Dalamud.Game.ClientState.Conditions.ConditionFlag.Occupied])
		{
			if (Svc.Condition[flag])
			{
				flags.Add(flag.ToString());
			}
		}

		return $"RSR {(DataCenter.IsActivated() ? "on" : "off")}, conditions {(flags.Count == 0 ? "none of the blocking ones" : string.Join(" ", flags))}";
	}

	// K4 probe (A557): per target the last action of the player's side on it, the kinds of its entries, and when.
	private static readonly Dictionary<uint, (uint Action, List<byte> Kinds, DateTime At)> _lastOwnAction = [];

	/// <summary>K4 probe (A557): an action of the player's side reached <paramref name="targetId"/>. Effect handler.</summary>
	internal static void RecordOwnAction(uint actionId, uint targetId, List<byte> kinds)
	{
		if (_fight != 0 && targetId != 0)
		{
			_lastOwnAction[targetId] = (actionId, kinds, DateTime.Now);
		}
	}

	/// <summary>
	/// K4 probe (A557): the server stopped the cast of <paramref name="entityId"/> (ActorControl 15). Writes the enemy, the
	/// control's parameters and the last action of the player's side on it, with the kinds of its entries and how long
	/// before - the answer to which familiar action interrupts, which no text states.
	/// </summary>
	internal static void RecordCastStopped(uint entityId, uint p1, uint p2, uint p3, uint p4)
	{
		if (_fight == 0 || Svc.Objects.SearchByEntityId(entityId) is not IBattleChara enemy || !enemy.IsEnemy())
		{
			return;
		}

		var last = _lastOwnAction.TryGetValue(entityId, out var own)
			? $"last own action on it {own.Action} (entry kinds {string.Join(" ", own.Kinds)}) {(DateTime.Now - own.At).TotalSeconds:F2} s before"
			: "no own action on it in this fight";
		DefenseTrace.Line($"crucible: {enemy.Name} stopped its cast (control {p1} {p2} {p3} {p4}); {last}");
	}

	private static uint _hornScreenMode;

	// A561: one probe line when the horn screen (pet-party agent active in mode 2) opens before a battle.
	private static unsafe void FollowHornScreen()
	{
		var party = AgentXBMPetParty.Instance();
		var mode = party != null && ((AgentInterface*)party)->IsAgentActive() ? party->Mode : uint.MaxValue;
		if (mode == HornScreenMode && UpcomingBattle() is var battle and not 0)
		{
			_upcoming = battle;
			CrucibleRoute.NoteBattle(battle);
		}

		if (mode != _hornScreenMode)
		{
			_hornScreenMode = mode;
			if (mode == HornScreenMode)
			{
				TraceTeamAndField("horn screen open");
			}
			else if (mode == BeastmasterRotation.CampsiteMode)
			{
				// A570: a campsite closes the stretch of fights since the board's entry or the last campsite.
				EndStretch();
			}
		}
	}

	/// <summary>AgentXBMPetParty.Mode 2: the party preview from content - the horn screen before a battle (FFXIVClientStructs
	/// aad0de66).</summary>
	internal const uint HornScreenMode = 2;

	/// <summary>AgentXBMStageDetailList entry kind 3: a battle detail (FFXIVClientStructs aad0de66).</summary>
	internal const uint BattleEntryKind = 3;

	/// <summary>K6 (A562): the battle about to start - the first battle entry of the stage list; 0 when it lists none.</summary>
	internal static unsafe uint UpcomingBattle()
	{
		var stages = AgentXBMStageDetailList.Instance();
		if (stages == null)
		{
			return 0;
		}

		foreach (ref readonly var entry in stages->Entries)
		{
			if (entry.EntryType == BattleEntryKind && entry.BattleDetailId != 0)
			{
				return entry.BattleDetailId;
			}
		}

		return 0;
	}

	private static int _finalOverRun = -1;

	/// <summary>A714: whether the final fight of the run in progress is over - the board is done, nothing after it counts for the run.</summary>
	internal static bool FinalFightOver => _finalOverRun == _run;

	private static void EndFight()
	{
		if (_fight != 0)
		{
			TraceTeamAndField($"fight {_fight} over");
			FinishLoad();
			LearnFamiliarHits();
			LearnNetAndDip();
			if (_finalFights.Contains(_fight))
			{
				_finalOverRun = _run;
				_lossFinalOver = true;
				LearnFamiliarLoss();
			}
		}

		_familiarFight.Clear();
		if (_fight != 0)
		{
			_lastFight = _fight;
		}

		_fight = 0;
		_enemies = [];
		_atStart.Clear();
		_present.Clear();
		_seen.Clear();
		_lastOwnAction.Clear();
		_ownDamage = 0d;
		_ownDamageByElement.Clear();
	}

	/// <summary>
	/// K1 (A549, A582): a hit of the player or one of his familiars on <paramref name="target"/> in a fight of the Crucible,
	/// with what the hit itself carries - its aspect and attack type from the effect entry (<paramref name="entry"/>), not
	/// the action sheet, which gives the beastmaster's axes and auto-attack none though they strike as slashing. Its damage
	/// per potency goes to the action's sums for hits on the target's weakness or on anything else of the same resistance
	/// grade; only actions of a fixed potency count - a plain potency in their effect text, or an auto-attack - and hits
	/// that strike no weakness are left out. Effect handler, game thread.
	/// </summary>
	internal static void RecordHit(Lumina.Excel.Sheets.Action action, IBattleChara target, EffectEntry entry, uint amount,
		bool byFamiliar = false)
	{
		if (_fight == 0 || amount == 0)
		{
			return;
		}

		if (byFamiliar)
		{
			RecordFamiliarDamage(action, amount);
		}

		var elements = ElementsOfHit(entry);

		// A678: a hit a tome converted measures the tome, and does not teach the elements of its action.
		var converted = MeasureTome(action, target, entry, amount, elements);
		if (!converted && (elements.Count > 0 || _elementNames.Count > 0)
			&& (!_actionElementsSeen.TryGetValue(action.RowId, out var before) || !before.SetEquals(elements)))
		{
			_actionElementsSeen[action.RowId] = elements;
			ElementsLearned++;
		}

		_ownDamage += amount;
		foreach (var element in elements)
		{
			_ownDamageByElement[element] = (_ownDamageByElement.TryGetValue(element, out var dealt) ? dealt : 0d) + amount;
		}

		// A672: the most HP each enemy kind was seen with, and the damage per potency of the player's damage items - their hit
		// is the action of the item's name -, taken back from a hit on a weakness by its bonus; by area, as K1.
		if (target.MaxHp > 0u)
		{
			_maxHpSeen[target.NameId] = Math.Max(_maxHpSeen.TryGetValue(target.NameId, out var seen) ? seen : 0u, target.MaxHp);
		}

		// A677: an item's hit, for whether a kill by an item grants a kill stack.
		if (!byFamiliar && Rotations.Basic.BeastmasterRotation.ItemPotencyOfAction(action.RowId) > 0f)
		{
			_itemHits[target.GameObjectId] = DateTime.Now;
		}

		if (!byFamiliar && Rotations.Basic.BeastmasterRotation.ItemPotencyOfAction(action.RowId) is var itemPotency && itemPotency > 0f
			&& EnemyOf(target.NameId) is { } hitEnemy)
		{
			if (_itemMeasuredIn != DataCenter.TerritoryID)
			{
				_itemMeasuredIn = DataCenter.TerritoryID;
				_itemPerPotency = default;
			}

			var weak = hitEnemy.Weakness != 0 && elements.Contains(hitEnemy.Weakness);
			var perItemPotency = amount / (itemPotency * (weak ? 1f + WeaknessBonus : 1f));
			_itemPerPotency = (_itemPerPotency.Sum + perItemPotency, _itemPerPotency.Hits + 1);
		}

		var potency = 1f;
		if (elements.Count == 0 || (action.GetActionCate() != ActionCate.Autoattack
			&& (!ActionTooltip.TryPlainPotency(action.RowId, out potency) || potency <= 0f)))
		{
			return;
		}

		if (_measuredIn != DataCenter.TerritoryID)
		{
			_measuredIn = DataCenter.TerritoryID;
			_perPotency.Clear();
			_bonusTraced = false;
		}

		var enemy = EnemyOf(target.NameId);
		if (enemy is not { } listed)
		{
			return;
		}

		var physical = AttackTypeKind.IsPhysical(entry.AttackType);
		var key = (action.RowId, physical, physical ? listed.PhysResistance : listed.MagResistance);
		var perPotency = amount / (double)potency;
		var sums = _perPotency.TryGetValue(key, out var known) ? known : default;
		sums = listed.Weakness != 0 && elements.Contains(listed.Weakness)
			? sums with { Weak = sums.Weak + perPotency, WeakHits = sums.WeakHits + 1 }
			: sums with { Other = sums.Other + perPotency, OtherHits = sums.OtherHits + 1 };
		_perPotency[key] = sums;

		if (!_bonusTraced && MeasuredWeaknessBonus is { } measured)
		{
			_bonusTraced = true;
			DefenseTrace.Line($"crucible: a hit on a weakness deals {measured:P0} more, first measured on action {action.RowId}");
		}
	}

	// K16 (A692, A711): the stride of a fight and a familiar in the learned Crucible files - the fight's XBMBattleDetail row times this stride
	// plus the familiar's pet row; the pet rows of the familiars lie below it (Pet rows 47 to 97). An encoding, not a figure
	// of the game.
	internal const uint FamiliarKeyStride = 1000;

	// K16 (A711): each familiar's Trick and Release hits in the fight under way - their damage and the potency of those hits.
	private static readonly Dictionary<BeastmasterPet, (double Damage, double Potency)> _familiarFight = [];

	/// <summary>K16 (A711): the two sums kept per fight and familiar in CrucibleFamiliarHits.</summary>
	internal enum HitSlot : uint
	{
		/// <summary>The damage of the familiar's Trick and Release hits.</summary>
		Damage,

		/// <summary>The potency of those hits.</summary>
		Potency,
	}

	private static readonly uint HitSlots = (uint)Enum.GetValues<HitSlot>().Length;

	// K16 (A711): the key of a fight's and a familiar's sum in CrucibleFamiliarHits.
	private static uint HitKey(uint battle, BeastmasterPet pet, HitSlot slot)
		=> (((battle * FamiliarKeyStride) + (uint)pet) * HitSlots) + (uint)slot;

	/// <summary>K16 (A711): the fight, familiar and sum a key of CrucibleFamiliarHits names.</summary>
	internal static (uint Battle, BeastmasterPet Pet, HitSlot Slot) HitOf(uint key)
		=> (key / HitSlots / FamiliarKeyStride, (BeastmasterPet)(key / HitSlots % FamiliarKeyStride), (HitSlot)(key % HitSlots));

	// A586: a familiar's hit by its source, the familiar out being the player's current one.
	private static void RecordFamiliarDamage(Lumina.Excel.Sheets.Action action, uint amount)
	{
		var pet = BeastmasterRotation.CurrentPet;
		var profile = BeastmasterRotation.FamiliarOf(pet);

		// K16 (A711): a hit of its Trick or Release, with the potency its sheet states - measured per hit, whatever the
		// familiar's time out and role in the fight.
		var potency = profile == null ? 0f
			: action.RowId == profile.TrickId ? profile.TrickPotency
			: action.RowId == profile.ReleaseId ? profile.ReleasePotency
			: 0f;
		if (_fight != 0 && pet != BeastmasterPet.None && potency > 0f)
		{
			var seen = _familiarFight.GetValueOrDefault(pet);
			_familiarFight[pet] = (seen.Damage + amount, seen.Potency + potency);
		}

		if (action.GetActionCate() == ActionCate.Autoattack)
		{
			_familiarAuto += amount;
		}
		else if (profile != null && action.RowId == profile.TrickId)
		{
			_familiarTrick += amount;
		}
		else if (profile != null && action.RowId == profile.ReleaseId)
		{
			_familiarRelease += amount;
		}
		else
		{
			_familiarOther += amount;
		}
	}

	/// <summary>
	/// A586 (concept 21, "K1 über alle Schadensquellen"): the share of familiar damage each source carries - measured over
	/// every familiar's hits since RSR loaded once each source has hit, else the shares of the first board in his logs.
	/// </summary>
	internal static (float Auto, float Trick, float Release, float Parting) FamiliarDamageShares
	{
		get
		{
			var all = _familiarAuto + _familiarTrick + _familiarRelease + _familiarOther;
			return _familiarAuto > 0d && _familiarTrick > 0d && _familiarRelease > 0d && _familiarOther > 0d
				? ((float)(_familiarAuto / all), (float)(_familiarTrick / all), (float)(_familiarRelease / all),
					(float)(_familiarOther / all))
				: FamiliarSharesFallback;
		}
	}

	/// <summary>
	/// A582: the weakness a hit strikes, by what the hit carries - its aspect (fire to water) in the upper half of the effect
	/// entry's second parameter, else its attack type (slashing, piercing, blunt) in the lower -, as rows of XBMElement.
	/// </summary>
	internal static HashSet<uint> ElementsOfHit(EffectEntry entry)
	{
		if (_hitElements.TryGetValue(entry.param1, out var known))
		{
			return known;
		}

		_ = Fights;
		var aspect = (Aspect)(entry.param1 >> AspectShift);
		var type = (AttackType)entry.AttackType;
		var name = aspect is >= Aspect.Fire and <= Aspect.Water
			? aspect.ToString()
			: type is >= AttackType.Slashing and <= AttackType.Blunt ? type.ToString() : string.Empty;
		var hits = new HashSet<uint>();
		foreach (var (row, element) in _elementNames)
		{
			if (name.Length > 0 && string.Equals(element, name, StringComparison.OrdinalIgnoreCase))
			{
				_ = hits.Add(row);
			}
		}

		// Before the element names are read there is nothing to map; do not keep the empty answer.
		if (_elementNames.Count > 0)
		{
			_hitElements[entry.param1] = hits;
		}

		return hits;
	}

	// The listed enemy of the fight in progress with this name, null when it is not listed.
	private static Enemy? EnemyOf(uint nameId)
	{
		foreach (var enemy in _enemies)
		{
			if (enemy.NameId == nameId)
			{
				return enemy;
			}
		}

		return null;
	}

	/// <summary>
	/// K10 shop (A569): a hit of an enemy on the player in a fight of the Crucible, <paramref name="amount"/> damage of an
	/// action of <paramref name="attackType"/> (the action sheet's AttackType). Effect handler, game thread.
	/// </summary>
	internal static void RecordTaken(uint attackType, uint amount)
	{
		if (_fight == 0 || amount == 0)
		{
			return;
		}

		var kind = AttackTypeKind.IsPhysical(attackType) ? HitKind.Physical
			: AttackTypeKind.IsMagic(attackType) ? HitKind.Magic
			: HitKind.Other;
		_fightLoad = kind switch
		{
			HitKind.Physical => _fightLoad with { Physical = _fightLoad.Physical + amount },
			HitKind.Magic => _fightLoad with { Magic = _fightLoad.Magic + amount },
			_ => _fightLoad with { Other = _fightLoad.Other + amount },
		};

		var now = DateTime.Now;
		if (_fightHits.Count == 0)
		{
			_fightFirstHit = now;
		}

		_fightHits.Add(new Hit((now - _fightFirstHit).TotalSeconds, amount, kind));
	}

	/// <summary>
	/// K13 (A668): of the damage of <paramref name="kind"/> (every kind with null) the heaviest fight dealt the player - in
	/// this area, else over every board, as <see cref="Loads"/> -, the largest share that falls within
	/// <paramref name="seconds"/> of one of its hits, counting at most <paramref name="hits"/> hits of that kind in a row (0:
	/// any number): what a protection of that duration, or of that many stacks, catches when it stands from that hit on. 1
	/// while no hit of that kind was recorded.
	/// </summary>
	internal static float CaughtShare(double seconds, int hits, HitKind? kind)
	{
		var series = _areaLoadsIn == DataCenter.TerritoryID && _areaLoads.Fights > 0 ? _areaHeaviestHits : _allHeaviestHits;
		double total = 0d, best = 0d;
		foreach (var hit in series)
		{
			if (kind == null || hit.Kind == kind)
			{
				total += hit.Amount;
			}
		}

		if (total <= 0d)
		{
			return 1f;
		}

		for (var first = 0; first < series.Count; first++)
		{
			if (kind != null && series[first].Kind != kind)
			{
				continue;
			}

			var caught = 0d;
			var counted = 0;
			for (var next = first; next < series.Count && series[next].At - series[first].At < seconds; next++)
			{
				if (kind != null && series[next].Kind != kind)
				{
					continue;
				}

				if (hits > 0 && counted >= hits)
				{
					break;
				}

				caught += series[next].Amount;
				counted++;
			}

			best = Math.Max(best, caught);
		}

		return (float)(best / total);
	}

	/// <summary>
	/// K13 (A670): of the damage the heaviest fight dealt the player, the share within <paramref name="seconds"/> of its first
	/// hit - what an item set at the start of a fight and standing that long (the Beast Potion Kit) is there for. 1 while no
	/// hit was recorded.
	/// </summary>
	internal static float ShareFromStart(double seconds)
	{
		var series = _areaLoadsIn == DataCenter.TerritoryID && _areaLoads.Fights > 0 ? _areaHeaviestHits : _allHeaviestHits;
		double total = 0d, early = 0d;
		foreach (var hit in series)
		{
			total += hit.Amount;
			if (hit.At < seconds)
			{
				early += hit.Amount;
			}
		}

		return total > 0d ? (float)(early / total) : 1f;
	}

	/// <summary>
	/// K10 shop (A570): the player's own healing on himself in a fight of the Crucible - his actions and items. Effect
	/// handler, game thread.
	/// </summary>
	internal static void RecordHealed(uint amount)
	{
		if (_fight != 0 && amount != 0)
		{
			_fightLoad = _fightLoad with { Healed = _fightLoad.Healed + amount };
			_fightOwnPending += amount;
			_fightOwnReported = DateTime.Now;
		}
	}

	/// <summary>
	/// K8 step 13 (A672): the damage per potency of the player's damage items, measured on their hits in this area without
	/// the weakness bonus; null before the first.
	/// </summary>
	internal static float? ItemDamagePerPotency
		=> _itemMeasuredIn == DataCenter.TerritoryID && _itemPerPotency.Hits > 0 ? (float)(_itemPerPotency.Sum / _itemPerPotency.Hits) : null;

	/// <summary>A672: the most HP an enemy of this name was seen with in the session; 0 before it was hit.</summary>
	internal static uint MaxHpSeen(uint nameId) => _maxHpSeen.TryGetValue(nameId, out var hp) ? hp : 0u;

	// A678: the tome RSR took and until when it stands; the gain measured per tome kind on this board; whether the first
	// converted hit was traced.
	private static (bool ToMagic, DateTime Until)? _tome;
	private static readonly Dictionary<bool, (double Sum, int Samples)> _tomeGain = [];
	private static uint _tomeGainIn;
	private static bool _tomeTraced;

	/// <summary>A678: RSR took a tome that makes every attack magic (or physical) for <paramref name="seconds"/>.</summary>
	internal static void TomeUsed(bool toMagic, float seconds)
	{
		if (seconds > 0f)
		{
			_tome = (toMagic, DateTime.Now.AddSeconds(seconds));
			_tomeTraced = false;
		}
	}

	/// <summary>
	/// A678: what a tome of this kind multiplied the damage by on this board - each converted hit against the same action in
	/// its own kind on the same enemy kind -, null until measured.
	/// </summary>
	internal static float? TomeGain(bool toMagic)
	{
		return _tomeGainIn == DataCenter.TerritoryID && _tomeGain.TryGetValue(toMagic, out var gain) && gain.Samples > 0
			? (float)(gain.Sum / gain.Samples) : null;
	}

	// A678: whether this hit came converted by the tome RSR took - in the tome's kind while its action has the other -, and
	// if so its gain over the same action in its own kind on the same enemy kind, the same weakness counted on both sides.
	private static bool MeasureTome(Lumina.Excel.Sheets.Action action, IBattleChara target, EffectEntry entry, uint amount,
		HashSet<uint> elements)
	{
		if (_tome is not { } tome || DateTime.Now > tome.Until)
		{
			return false;
		}

		var hitPhysical = AttackTypeKind.IsPhysical(entry.AttackType);
		var ownPhysical = AttackTypeKind.IsPhysical(action.AttackType.RowId);
		if (hitPhysical == tome.ToMagic || hitPhysical == ownPhysical)
		{
			return false;
		}

		if (!_tomeTraced)
		{
			_tomeTraced = true;
			DefenseTrace.Line($"crucible: a hit the {(tome.ToMagic ? "magic" : "physical")} tome converted (action {action.RowId}) carries "
				+ (elements.Count > 0 ? $"element(s) {string.Join(", ", elements)}" : "no element"));
		}

		var potency = 1f;
		if (EnemyOf(target.NameId) is not { } listed
			|| (action.GetActionCate() != ActionCate.Autoattack && (!ActionTooltip.TryPlainPotency(action.RowId, out potency) || potency <= 0f))
			|| !_perPotency.TryGetValue((action.RowId, ownPhysical, ownPhysical ? listed.PhysResistance : listed.MagResistance), out var own))
		{
			return true;
		}

		var normallyWeak = listed.Weakness != 0 && _actionElementsSeen.TryGetValue(action.RowId, out var normal) && normal.Contains(listed.Weakness);
		var (sum, hits) = normallyWeak ? (own.Weak, own.WeakHits) : (own.Other, own.OtherHits);
		if (hits == 0 || sum <= 0d)
		{
			return true;
		}

		if (_tomeGainIn != DataCenter.TerritoryID)
		{
			_tomeGainIn = DataCenter.TerritoryID;
			_tomeGain.Clear();
		}

		var (gainSum, samples) = _tomeGain.TryGetValue(tome.ToMagic, out var known) ? known : default;
		_tomeGain[tome.ToMagic] = (gainSum + (amount / (double)potency / (sum / hits)), samples + 1);
		return true;
	}

	/// <summary>
	/// K8 "Auslöser" (A677): whether a kill by a damage item grants the worn gear's kill stack - measured this session: an
	/// enemy fell within a GCD of an item's hit, and the stacks rose within a GCD after (true) or stayed below the most
	/// (false); null until then.
	/// </summary>
	internal static bool? ItemKillsStack { get; private set; }

	private static readonly Dictionary<ulong, DateTime> _itemHits = [];
	private static DateTime _itemKill = DateTime.MinValue;
	private static int _stacksSeen;

	// A719: the stacks the frame before, while the watch runs.
	private static int? _stacksBefore;

	// A677: follows the item hits to a kill and the player's kill stacks after it, every frame while the gear is worn.
	private static void FollowKillStacks()
	{
		if (ItemKillsStack != null || _itemHits.Count == 0 && _itemKill == DateTime.MinValue)
		{
			_stacksBefore = null;
			return;
		}

		var now = DateTime.Now;
		var gcd = DataCenter.DefaultGCDTotal;
		if (Rotations.Basic.BeastmasterRotation.WornKillStack() is not { } stack || Player.Object is not { } player)
		{
			_itemHits.Clear();
			_itemKill = DateTime.MinValue;
			_stacksBefore = null;
			return;
		}

		// A719: the fall is held against the stacks of the frame before it was seen - the stack may come in the same frame.
		var stacks = Rotations.Basic.BeastmasterRotation.StacksHeld(player, stack.Status);
		var before = _stacksBefore ?? stacks;
		_stacksBefore = stacks;
		List<ulong>? done = null;
		foreach (var (id, at) in _itemHits)
		{
			var gone = Svc.Objects.SearchById(id) is not IBattleChara hit || hit.IsDead || hit.CurrentHp == 0u;
			if (gone && _itemKill == DateTime.MinValue)
			{
				_itemKill = now;
				_stacksSeen = before;
			}

			if (gone || (now - at).TotalSeconds > gcd)
			{
				(done ??= []).Add(id);
			}
		}

		foreach (var id in done ?? [])
		{
			_ = _itemHits.Remove(id);
		}

		if (_itemKill == DateTime.MinValue)
		{
			return;
		}

		if (stacks > _stacksSeen)
		{
			ItemKillsStack = true;
			DefenseTrace.Line($"crucible: a kill by an item granted {stack.Status} ({_stacksSeen} -> {stacks}) - its stacks count in the item worth");
		}
		else if ((now - _itemKill).TotalSeconds > gcd && _stacksSeen < stack.Max)
		{
			ItemKillsStack = false;
			DefenseTrace.Line($"crucible: a kill by an item granted no {stack.Status} ({stacks} of {stack.Max}) - its stacks do not count");
		}
		else if ((now - _itemKill).TotalSeconds > gcd)
		{
			_itemKill = DateTime.MinValue;
		}
	}

	/// <summary>
	/// K10 shop (A570): the player's load per fight - the mean and the heaviest fight - in this area, else over every board
	/// since RSR loaded (<c>HereOnly</c> false); null before the first fight ended.
	/// </summary>
	internal static (FightLoad Mean, FightLoad Heaviest, bool HereOnly)? Loads
	{
		get
		{
			var (fights, sum, heaviest) = _areaLoadsIn == DataCenter.TerritoryID && _areaLoads.Fights > 0 ? _areaLoads : _allLoads;
			if (fights == 0)
			{
				return null;
			}

			var mean = new FightLoad(sum.Physical / fights, sum.Magic / fights, sum.Other / fights, sum.Healed / fights);
			return (mean, heaviest, _areaLoadsIn == DataCenter.TerritoryID && _areaLoads.Fights > 0);
		}
	}

	/// <summary>K10 shop (A570): the mean number of fights between two campsites (or a board's entry and exit); 0 before the first.</summary>
	internal static float FightsPerStretch => _stretches.Stretches > 0 ? _stretches.Fights / (float)_stretches.Stretches : 0f;

	// A570: the fight just over joins the loads of its area and of every board. A661: and, without the gear worn, the
	// learned load of this fight, should it be its heaviest.
	private static void FinishLoad()
	{
		LearnLoad();
		if (_areaLoadsIn != _tracedArea)
		{
			_areaLoadsIn = _tracedArea;
			_areaLoads = default;
			_areaHeaviestHits = [];
		}

		// A668: the hits of the heaviest fight, by the same test as its load below.
		if (_fightLoad.Damage > _areaLoads.Heaviest.Damage)
		{
			_areaHeaviestHits = [.. _fightHits];
		}

		if (_fightLoad.Damage > _allLoads.Heaviest.Damage)
		{
			_allHeaviestHits = [.. _fightHits];
		}

		_areaLoads = Add(_areaLoads, _fightLoad);
		_allLoads = Add(_allLoads, _fightLoad);
		_fightLoad = default;
		_fightHits.Clear();
		_stretchFights++;

		static (int Fights, FightLoad Sum, FightLoad Heaviest) Add((int Fights, FightLoad Sum, FightLoad Heaviest) loads, FightLoad load)
		{
			return (loads.Fights + 1,
				new FightLoad(loads.Sum.Physical + load.Physical, loads.Sum.Magic + load.Magic, loads.Sum.Other + load.Other,
					loads.Sum.Healed + load.Healed),
				load.Damage > loads.Heaviest.Damage ? load : loads.Heaviest);
		}
	}

	private static int _lossRun = -1;
	private static long _runFamiliarLoss;
	private static bool _lossFinalOver;
	private static readonly List<uint> _runUseLosses = [];

	/// <summary>
	/// K7 team size (A698): HP a familiar lost in one use on a board - what it came out with less what it left with, 0 when
	/// it fell. Summed per run; once the final fight is over, a later use (the familiar out leaves after the end) still
	/// counts.
	/// </summary>
	internal static void RecordFamiliarLoss(uint loss)
	{
		if (!DataCenter.IsInCrucible || loss == 0)
		{
			return;
		}

		if (_lossRun != _run)
		{
			_lossRun = _run;
			_runFamiliarLoss = 0;
			_lossFinalOver = false;
			_runUseLosses.Clear();
		}

		_runFamiliarLoss += loss;
		_runUseLosses.Add(loss);
		LearnUseLoss(_fight != 0 ? _fight : _lastFight, loss);
		if (_lossFinalOver)
		{
			LearnFamiliarLoss();
		}
	}

	// The fight last in progress: a familiar that leaves after its end closes its use for that fight (A710).
	private static uint _lastFight;

	// K11 (A710): the key of a fight's largest loss of a use in CrucibleFamiliarLoss - beyond the boards' rows and their
	// ranked uses.
	private static uint UseLossKey(uint battle) => battle * FamiliarKeyStride * FamiliarKeyStride;

	// K11 (A710): the loss of a use into the largest learned for the fight it was in.
	private static void LearnUseLoss(uint battle, uint loss)
	{
		if (battle == 0)
		{
			return;
		}

		var key = UseLossKey(battle);
		lock (OtherConfiguration.CrucibleFamiliarLossGate)
		{
			if (OtherConfiguration.CrucibleFamiliarLoss.TryGetValue(key, out var known) && known >= loss)
			{
				return;
			}

			OtherConfiguration.CrucibleFamiliarLoss[key] = (int)Math.Min(int.MaxValue, loss);
		}

		DefenseTrace.Line($"crucible fight {battle}: a use cost a familiar {loss} HP, the most in this fight so far - stored");
		_ = OtherConfiguration.SaveCrucibleFamiliarLoss();
	}

	/// <summary>
	/// K11 (A710): the most HP a use cost a familiar in <paramref name="battle"/> (an XBMBattleDetail row), over every
	/// session; null before a use there was measured.
	/// </summary>
	internal static uint? UseLossIn(uint battle)
	{
		lock (OtherConfiguration.CrucibleFamiliarLossGate)
		{
			return battle != 0 && OtherConfiguration.CrucibleFamiliarLoss.TryGetValue(UseLossKey(battle), out var loss) ? (uint)loss : null;
		}
	}

	/// <summary>Whether a key of CrucibleFamiliarLoss holds a fight's largest loss of a use, and that fight (A710).</summary>
	internal static bool IsUseLossKey(uint key, out uint battle)
	{
		battle = key / (FamiliarKeyStride * FamiliarKeyStride);
		return battle != 0 && key % (FamiliarKeyStride * FamiliarKeyStride) == 0;
	}

	// K7 team size (A698, A703): the run's familiar loss into the board's learned heaviest - the whole under the board's
	// row, each use under the board's row times FamiliarKeyStride plus its rank (largest first), each rank the largest seen.
	private static void LearnFamiliarLoss()
	{
		var board = CurrentBoard;
		if (board == 0 || _lossRun != _run || _runFamiliarLoss <= 0)
		{
			return;
		}

		var loss = (int)Math.Min(int.MaxValue, _runFamiliarLoss);
		var uses = new List<uint>(_runUseLosses);
		uses.Sort((a, b) => b.CompareTo(a));
		var changed = false;
		lock (OtherConfiguration.CrucibleFamiliarLossGate)
		{
			if (!OtherConfiguration.CrucibleFamiliarLoss.TryGetValue(board, out var known) || known < loss)
			{
				OtherConfiguration.CrucibleFamiliarLoss[board] = loss;
				changed = true;
			}

			for (var i = 0; i < uses.Count && i < FamiliarKeyStride - 1; i++)
			{
				var key = (board * FamiliarKeyStride) + (uint)i + 1;
				var use = (int)uses[i];
				if (!OtherConfiguration.CrucibleFamiliarLoss.TryGetValue(key, out var seen) || seen < use)
				{
					OtherConfiguration.CrucibleFamiliarLoss[key] = use;
					changed = true;
				}
			}
		}

		if (!changed)
		{
			return;
		}

		DefenseTrace.Line($"crucible board {board}: the familiars lost {loss} HP in this run over {uses.Count} uses, "
			+ $"{(uses.Count > 0 ? uses[0] : 0)} the largest; the heaviest so far stored");
		_ = OtherConfiguration.SaveCrucibleFamiliarLoss();
	}

	/// <summary>K7 team size (A698): the most HP the familiars lost together in one run on <paramref name="board"/>; null before one was measured.</summary>
	internal static int? FamiliarLossOf(uint board)
	{
		lock (OtherConfiguration.CrucibleFamiliarLossGate)
		{
			return OtherConfiguration.CrucibleFamiliarLoss.TryGetValue(board, out var loss) ? loss : null;
		}
	}

	/// <summary>
	/// K7 team size (A703): the losses of the familiars' uses on <paramref name="board"/>, largest first, each rank the
	/// largest any run there had; empty before one was measured.
	/// </summary>
	internal static List<uint> FamiliarUseLossesOf(uint board)
	{
		var uses = new List<uint>();
		lock (OtherConfiguration.CrucibleFamiliarLossGate)
		{
			for (var rank = 1u; rank < FamiliarKeyStride; rank++)
			{
				if (!OtherConfiguration.CrucibleFamiliarLoss.TryGetValue((board * FamiliarKeyStride) + rank, out var use))
				{
					break;
				}

				uses.Add((uint)use);
			}
		}

		uses.Sort((a, b) => b.CompareTo(a));
		return uses;
	}

	// K16 (A711): each familiar's measured hits of the fight just over into its sums for this fight, over every session.
	private static void LearnFamiliarHits()
	{
		var learned = new List<string>();
		lock (OtherConfiguration.CrucibleFamiliarHitsGate)
		{
			foreach (var (pet, (damage, potency)) in _familiarFight)
			{
				if (damage <= 0d || potency <= 0d)
				{
					continue;
				}

				var damageKey = HitKey(_fight, pet, HitSlot.Damage);
				var potencyKey = HitKey(_fight, pet, HitSlot.Potency);
				var allDamage = OtherConfiguration.CrucibleFamiliarHits.GetValueOrDefault(damageKey) + (long)Math.Round(damage);
				var allPotency = OtherConfiguration.CrucibleFamiliarHits.GetValueOrDefault(potencyKey) + (long)Math.Round(potency);
				if (allDamage > int.MaxValue || allPotency > int.MaxValue)
				{
					continue;
				}

				OtherConfiguration.CrucibleFamiliarHits[damageKey] = (int)allDamage;
				OtherConfiguration.CrucibleFamiliarHits[potencyKey] = (int)allPotency;
				learned.Add($"{pet} {allDamage / (double)allPotency:F2} over {allPotency} potency");
			}
		}

		if (learned.Count > 0)
		{
			_ = OtherConfiguration.SaveCrucibleFamiliarHits();
			DefenseTrace.Line($"crucible fight {_fight}: familiar damage per potency, summed over the sessions - {string.Join(", ", learned)}");
		}
	}

	/// <summary>
	/// K16 (A711): how a familiar's measured damage per potency in <paramref name="battle"/> stands against what the
	/// estimate expects of it (<paramref name="expected"/>: the estimate's factor on a familiar's potency there), over the
	/// mean of that ratio among the familiars measured there - 1 while it or no other was measured.
	/// </summary>
	internal static float FamiliarHitFactor(uint battle, BeastmasterPet pet, Func<BeastmasterPet, float> expected)
	{
		if (battle == 0 || pet == BeastmasterPet.None)
		{
			return 1f;
		}

		var measured = new Dictionary<BeastmasterPet, (double Damage, double Potency)>();
		lock (OtherConfiguration.CrucibleFamiliarHitsGate)
		{
			foreach (var (key, sum) in OtherConfiguration.CrucibleFamiliarHits)
			{
				var (fight, familiar, slot) = HitOf(key);
				if (fight != battle || sum <= 0)
				{
					continue;
				}

				var seen = measured.GetValueOrDefault(familiar);
				measured[familiar] = slot == HitSlot.Damage ? (sum, seen.Potency) : (seen.Damage, sum);
			}
		}

		double ratios = 0d;
		var count = 0;
		double? own = null;
		foreach (var (familiar, (damage, potency)) in measured)
		{
			var expects = expected(familiar);
			if (damage <= 0d || potency <= 0d || expects <= 0f)
			{
				continue;
			}

			var ratio = damage / potency / expects;
			ratios += ratio;
			count++;
			if (familiar == pet)
			{
				own = ratio;
			}
		}

		return own is { } mine && count > 1 ? (float)(mine / (ratios / count)) : 1f;
	}

	// A661: the damage of the fight just over, taken back to the player without his gear - his maximum HP, the damage he
	// takes, his physical and magic vulnerability and evasion, as the shop weighs them -, in whole percent rounded up.
	private static void LearnLoad()
	{
		if (_fight == 0 || _fightLoad.Damage <= 0d || Player.Object is not { MaxHp: > 0u } player)
		{
			return;
		}

		var share = BareShare(_fightLoad, player.MaxHp);
		var percent = (int)Math.Ceiling(share * PercentScale);
		lock (OtherConfiguration.CrucibleFightLoadsGate)
		{
			if (OtherConfiguration.CrucibleFightLoads.TryGetValue(_fight, out var known) && known >= percent)
			{
				return;
			}

			OtherConfiguration.CrucibleFightLoads[_fight] = percent;
		}

		_ = OtherConfiguration.SaveCrucibleFightLoads();
		DefenseTrace.Line($"crucible fight {_fight}: cost you {_fightLoad.Damage:F0} HP, {percent} % of your HP without gear - "
			+ "its heaviest so far, stored");
	}

	// A570: a campsite, or the board's end, closes the stretch of fights since the last one.
	private static void EndStretch()
	{
		if (_stretchFights > 0)
		{
			_stretches = (_stretches.Stretches + 1, _stretches.Fights + _stretchFights);
			_stretchFights = 0;
		}
	}

	/// <summary>
	/// K1 (A549, A582): how much more a hit on a weakness deals on this board - measured where this board has measured it,
	/// else the fallback measured in his logs. 0 outside the Crucible.
	/// </summary>
	internal static float WeaknessBonus => MeasuredWeaknessBonus ?? (DataCenter.IsInCrucible ? WeaknessBonusFallback : 0f);

	/// <summary>
	/// K1 (A549, A582): the weakness bonus measured in this area - per action and resistance grade that hit both a weakness
	/// and an enemy of that grade without it, the mean damage per potency on the weakness against the rest, weighted by the
	/// smaller of the two counts; null until one has.
	/// </summary>
	internal static float? MeasuredWeaknessBonus
	{
		get
		{
			if (_measuredIn != DataCenter.TerritoryID)
			{
				return null;
			}

			double weighted = 0d;
			double weights = 0d;
			foreach (var (_, sums) in _perPotency)
			{
				if (sums.WeakHits == 0 || sums.OtherHits == 0 || sums.Other <= 0d)
				{
					continue;
				}

				var ratio = sums.Weak / sums.WeakHits / (sums.Other / sums.OtherHits);
				var weight = Math.Min(sums.WeakHits, sums.OtherHits);
				weighted += ratio * weight;
				weights += weight;
			}

			return weights > 0d ? Math.Max(0f, (float)(weighted / weights) - 1f) : null;
		}
	}

	/// <summary>
	/// K3 (A549): by how much the measured weakness bonus speeds the fall of an enemy of the fight in progress - one plus the
	/// bonus times the share of the player's side's damage in this fight dealt by actions hitting its weakness; 1 while
	/// nothing is measured or it has none.
	/// </summary>
	internal static float FallSpeedup(uint nameId)
	{
		var weakness = WeaknessOf(nameId);
		var bonus = WeaknessBonus;
		if (weakness == 0 || bonus <= 0f || _ownDamage <= 0d || !_ownDamageByElement.TryGetValue(weakness, out var dealt))
		{
			return 1f;
		}

		return 1f + (bonus * (float)(dealt / _ownDamage));
	}

	internal static string EnemyName(uint nameId)
	{
		return Service.GetSheet<Lumina.Excel.Sheets.BNpcName>()?.GetRowOrDefault(nameId) is { } row
			? row.Singular.ExtractText()
			: $"#{nameId}";
	}

	internal static string ElementName(uint element)
	{
		return _elementNames.TryGetValue(element, out var name) ? name : "nothing";
	}
}
