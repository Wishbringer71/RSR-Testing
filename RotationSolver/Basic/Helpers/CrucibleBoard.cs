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
	private const int PercentScale = 100;

	/// <summary>
	/// K10 Campsite (A661): the heaviest damage the player took in <paramref name="battle"/> (an XBMBattleDetail row), as a
	/// share of his maximum HP without his Crucible gear, over every session; null while it was never fought with RSR.
	/// </summary>
	internal static float? BattleLoad(uint battle)
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

	private static void EndFight()
	{
		if (_fight != 0)
		{
			TraceTeamAndField($"fight {_fight} over");
			FinishLoad();
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
		if (elements.Count > 0 || _elementNames.Count > 0)
		{
			_actionElementsSeen[action.RowId] = elements;
		}

		_ownDamage += amount;
		foreach (var element in elements)
		{
			_ownDamageByElement[element] = (_ownDamageByElement.TryGetValue(element, out var dealt) ? dealt : 0d) + amount;
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

	// A586: a familiar's hit by its source, the familiar out being the player's current one.
	private static void RecordFamiliarDamage(Lumina.Excel.Sheets.Action action, uint amount)
	{
		var profile = BeastmasterRotation.FamiliarOf(BeastmasterRotation.CurrentPet);
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

		_fightLoad = AttackTypeKind.IsPhysical(attackType) ? _fightLoad with { Physical = _fightLoad.Physical + amount }
			: AttackTypeKind.IsMagic(attackType) ? _fightLoad with { Magic = _fightLoad.Magic + amount }
			: _fightLoad with { Other = _fightLoad.Other + amount };
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
		}

		_areaLoads = Add(_areaLoads, _fightLoad);
		_allLoads = Add(_allLoads, _fightLoad);
		_fightLoad = default;
		_stretchFights++;

		static (int Fights, FightLoad Sum, FightLoad Heaviest) Add((int Fights, FightLoad Sum, FightLoad Heaviest) loads, FightLoad load)
		{
			return (loads.Fights + 1,
				new FightLoad(loads.Sum.Physical + load.Physical, loads.Sum.Magic + load.Magic, loads.Sum.Other + load.Other,
					loads.Sum.Healed + load.Healed),
				load.Damage > loads.Heaviest.Damage ? load : loads.Heaviest);
		}
	}

	// A661: the damage of the fight just over, taken back to the player without his gear - his maximum HP, the damage he
	// takes, his physical and magic vulnerability and evasion, as the shop weighs them -, in whole percent rounded up.
	private static void LearnLoad()
	{
		if (_fight == 0 || _fightLoad.Damage <= 0d || Player.Object is not { MaxHp: > 0u } player)
		{
			return;
		}

		var (hp, taken, physical, magic) = Rotations.Basic.BeastmasterRotation.WornFactors();
		var bare = player.MaxHp / Math.Max(hp, float.Epsilon);
		var share = ((_fightLoad.Physical / Math.Max(physical, float.Epsilon)) + (_fightLoad.Magic / Math.Max(magic, float.Epsilon))
			+ _fightLoad.Other) / Math.Max(taken, float.Epsilon) / bare;
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
