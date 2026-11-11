using System.Globalization;
using System.Text.RegularExpressions;
using ECommons.DalamudServices;
using ECommons.UIHelpers.AtkReaderImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Rotations.Basic;

/// <summary>
/// K10 of the Crucible (concept 21, "K10 – Zwischen den Kämpfen", Campsite, A564): at a campsite RSR picks who rests with
/// the player - familiars only when he still comes out at full HP, so he never comes out worse than resting alone; the
/// number that brings the familiars the most HP, the most hurt standing, unfed ones. Resting stays his click.
/// </summary>
public partial class BeastmasterRotation
{
	// The campsite window: AgentXBMPetParty active in this mode with XBMPetParty visible (Daedalus 74659a6, CrucibleAutoPilot,
	// in the field 25. and 27.10.2026). FFXIVClientStructs aad0de6 calls the mode "buy feed result", so every click is
	// checked against the selection it should have made.
	internal const uint CampsiteMode = 4;

	// The whole a rest shares among the player and the familiars resting, of each one's maximum HP, where no prompt of the
	// board was read: the smallest known (board 1: 90 % alone, 45 % each with one, 30 % with two - wiki, Daedalus' field
	// note, his runs of 07.11.2026; the First Master's Board's sixth move shares 150 %, A667).
	private const float RestAloneShare = 0.9f;

	// The share the game's rest prompt offers, by the number of familiars resting ("Rest and recover 45% of HP for you
	// and your: ...", Addon 17660): any number before a percent sign, in any client language.
	private static readonly Regex OfferedShare = new(@"(?<share>\d+)\s*%", RegexOptions.Compiled);

	// A667: the whole read at a campsite of this run - board and space - which counts there at once.
	private static readonly Dictionary<(uint Board, int Node), float> _restPoolRead = [];

	// ClientStructs 0adb2e5 names the pet party's modes: 3 buy feed, 4 buy feed result, 5 feed detail. Daedalus saw the campsite
	// window in mode 4; with 3 or 5 seen since the window opened, or a feed in it, mode 4 is a feeding, not a campsite (A631).
	internal const uint FeedBuyMode = 3;
	internal const uint FeedDetailMode = 5;
	private static bool _feedFlow;
	private static List<int>? _campsiteRequested;
	private static string _finalRestTraced = string.Empty;

	/// <summary>One familiar the campsite window recommends to rest, and why.</summary>
	public readonly record struct CrucibleCampPick(BeastmasterPet Pet, float Share, string Why);

	/// <summary>Whether the campsite window is open and RSR reads it.</summary>
	public static bool CrucibleCampOpen { get; private set; }

	/// <summary>What RSR selects here without a click, and why.</summary>
	public static string CrucibleCampSummary { get; private set; } = string.Empty;

	/// <summary>K14: at the last campsite before the final fight, who should rest for it; empty elsewhere.</summary>
	public static IReadOnlyList<CrucibleCampPick> CrucibleCampRecommendation { get; private set; } = [];

	/// <summary>K14: what the recommendation leaves the player with, for the window.</summary>
	public static string CrucibleCampOutcome { get; private set; } = string.Empty;

	/// <summary>K14: while a familiar is to be fed, the familiar the feed is meant for; empty otherwise.</summary>
	public static string CrucibleFeedTarget { get; private set; } = string.Empty;

	/// <summary>The window's "Apply": the recommendation is selected, one click at a time; resting stays the player's click.</summary>
	public static void RequestCrucibleCamp()
	{
		_campsiteRequested = [];
		foreach (var pick in CrucibleCampRecommendation)
		{
			_campsiteRequested.Add((int)pick.Pet);
		}

		_campsiteHeld = false;
	}

	private static int? _campsiteLimit;
	private static bool _campsiteHeld;
	private static string? _campsiteSelection;
	private static bool _campsiteSettled;
	private static DateTime _campsiteSince;
	private static string _campsiteTraced = string.Empty;
	private static (int Position, bool Select, int Before, DateTime At)? _campsiteClick;

	/// <summary>
	/// K10: with the campsite window open, one click a time toward the pick: the number of familiars that brings them the
	/// most HP while the player's HP after the rest still carry the heaviest way to the next campsite (A661) - full HP where
	/// a fight on those ways was never measured -, and for it the standing, unfed familiars not at full HP with the smallest
	/// share of their HP. Never presses rest. Called every frame from the update; <paramref name="pick"/> is the rotation's
	/// setting.
	/// </summary>
	internal static unsafe void FollowCampsite(bool pick)
	{
		var party = AgentXBMPetParty.Instance();
		var player = Player;
		var screen = (AtkUnitBase*)Svc.GameGui.GetAddonByName("XBMPetParty").Address;
		var active = party != null && ((AgentInterface*)party)->IsAgentActive();
		var visible = screen != null && screen->IsVisible;
		_feedFlow = active && (_feedFlow || party->Mode is FeedBuyMode or FeedDetailMode
			|| (visible && new ReaderXBMPetParty(screen).FeedItem.ItemId != 0));
		if (active && visible)
		{
			NoteFeedSlots(party, screen);
		}

		FollowFeeding(party, screen, _feedFlow && visible && DataCenter.IsInCrucible);
		if (!pick || !DataCenter.IsInCrucible || InCombat || party == null || player == null || player.MaxHp == 0
			|| !active || party->Mode != CampsiteMode || !visible || _feedFlow)
		{
			CrucibleCampOpen = false;
			CrucibleCampSummary = string.Empty;
			CrucibleCampRecommendation = [];
			CrucibleCampOutcome = string.Empty;
			_campsiteRequested = null;
			_campsiteLimit = null;
			_campsiteHeld = false;
			_campsiteSelection = null;
			_campsiteSettled = false;
			_campsiteTraced = string.Empty;
			_campsiteClick = null;
			return;
		}

		CrucibleCampOpen = true;
		var selected = new List<int>();
		foreach (var chosen in party->SelectedPetIds.AsSpan())
		{
			selected.Add((int)chosen.PetId);
		}

		// While the rest prompt is open it speaks for the selection as it stands; nothing is clicked under it. A669: the
		// familiars selected that a rest fills (Lassi Simular) may not share the whole - the reading takes them as not sharing.
		var selectedPets = party->SelectedPets.AsSpan();
		var filling = 0;
		foreach (var position in selected)
		{
			if (position >= 0 && position < selectedPets.Length && RestFills((BeastmasterPet)selectedPets[position].PetId))
			{
				filling++;
			}
		}

		if (ReadOfferedShare(selected.Count, filling))
		{
			// K15 (A691): the game offers the rest's share - the prompt before a rest.
			NoteRestOffered();
			return;
		}

		var selection = string.Join(",", selected);
		var now = DateTime.Now;

		// The selection flickers to the whole team as the window opens (Daedalus, in the field): RSR acts once it has
		// stood unchanged for a GCD, and only then takes a change it did not make as his.
		if (!_campsiteSettled)
		{
			if (selection != _campsiteSelection)
			{
				_campsiteSelection = selection;
				_campsiteSince = now;
				return;
			}

			if ((now - _campsiteSince).TotalSeconds < DataCenter.DefaultGCDTotal)
			{
				return;
			}

			_campsiteSettled = true;
		}

		if (_campsiteClick is { } click)
		{
			var took = selected.Contains(click.Position) == click.Select;
			if (!took && (now - click.At).TotalSeconds < DataCenter.DefaultGCDTotal)
			{
				return;
			}

			_campsiteClick = null;
			if (click.Select && (!took || selected.Count <= click.Before))
			{
				// The window refused one more, or took it in place of another: this campsite rests no more than before.
				_campsiteLimit = click.Before;
				DefenseTrace.Line($"crucible campsite: rests at most {click.Before} familiars here - the window "
					+ (took ? "took position " + click.Position + " in place of another" : "refused position " + click.Position));
			}
			else if (!took)
			{
				_campsiteHeld = true;
				DefenseTrace.Line($"crucible campsite: the selection did not follow the click on position {click.Position} - left to you");
			}

			_campsiteSelection = selection;
		}
		else if (selection != _campsiteSelection && !_campsiteHeld)
		{
			_campsiteHeld = true;
			DefenseTrace.Line($"crucible campsite: changed by hand to positions {selection} - kept for this campsite");
		}

		if (_campsiteHeld)
		{
			return;
		}

		_campsiteSelection = selection;

		// K15 (A691): while no rest has been taken in this run and the run needs none, walk on - a rest ends the bonus
		// Schlaflose Nächte (1,000).
		if (NoRestBonusStands && NeedsNoRestOrFeed(out var restless))
		{
			CrucibleCampSummary = $"RSR advises walking on without a rest - {restless}; a rest ends the bonus Schlaflose Nächte (1,000 points)";
			CrucibleCampRecommendation = [];
			if (_campsiteTraced != CrucibleCampSummary)
			{
				_campsiteTraced = CrucibleCampSummary;
				DefenseTrace.Line($"crucible campsite: {CrucibleCampSummary}");
			}

			return;
		}

		var pets = party->SelectedPets.AsSpan();
		var current = party->PetCurrentHealth;
		var max = party->PetMaxHealth;
		var rows = new ReaderXBMPetParty(screen).TeamEntries;
		if (rows.Count != pets.Length)
		{
			Hold($"crucible campsite: the window lists {rows.Count} familiars, the team {pets.Length} - left to you");
			return;
		}

		var hurt = new List<(int Position, float Share, bool Short, BeastmasterPet Pet, bool Fills, float Need)>();
		for (var i = 0; i < pets.Length && i < current.Length && i < max.Length; i++)
		{
			// The window's rows carry the feed; they are read only where their HP is the team's at the same position.
			if (rows[i].MaxHP != max[i] || rows[i].HP != current[i])
			{
				Hold($"crucible campsite: the window's HP of position {i} ({rows[i].HP}/{rows[i].MaxHP}) is not the team's "
					+ $"({current[i]}/{max[i]}) - left to you");
				return;
			}

			// A fallen familiar is not healed there, one at full HP takes a share for nothing.
			if (current[i] == 0 || current[i] >= max[i])
			{
				continue;
			}

			// A669: a fed one loses its feed unless a Lily Simular keeps it; a rest fills one fed a Lassi Simular, and without
			// the Lily it rests only to more effective HP than now - full HP without its feed against its HP now through the
			// damage its feed takes off.
			var share = current[i] / (float)max[i];
			var misses = 1f - share;
			var fed = new List<ushort>();
			foreach (var entry in rows[i].FedItems)
			{
				if (entry.ItemId is > 0 and <= ushort.MaxValue)
				{
					fed.Add((ushort)entry.ItemId);
				}
			}

			var (keeps, fills, feedHp, feedTaken) = FeedAtRest(fed.ToArray());
			if (rows[i].FedCurrent > 0 && !keeps)
			{
				var bare = max[i] / Math.Max(feedHp, float.Epsilon);
				if (!fills || bare <= current[i] / Math.Max(feedTaken, float.Epsilon))
				{
					continue;
				}

				misses = (bare - current[i]) / max[i];
			}

			hurt.Add((i, share, ShortForFinal((BeastmasterPet)pets[i].PetId, current[i]), (BeastmasterPet)pets[i].PetId, fills, misses));
		}

		// K11 stage 2 (A590): a familiar the final fight needs to heal or protect, short of HP for it, rests first; then the
		// most a rest brings it (A669: all it misses where the rest fills it), then the least HP.
		var one = RestShare(1);
		hurt.Sort((a, b) => a.Short != b.Short ? b.Short.CompareTo(a.Short)
			: Brings(b, one).CompareTo(Brings(a, one)) is var byGain && byGain != 0 ? byGain : a.Share.CompareTo(b.Share));
		var self = player.CurrentHp / (float)player.MaxHp;
		var resting = 0;
		var bestGain = 0f;
		_campsiteLimit ??= CrucibleRoute.CampsiteLimit();
		var need = StretchNeed();
		for (var n = 1; n <= hurt.Count && (_campsiteLimit is not { } limit || n <= limit); n++)
		{
			var share = RestShare(n);
			if (!Carries(self + share, need))
			{
				continue;
			}

			var gain = 0f;
			for (var i = 0; i < n; i++)
			{
				gain += Brings(hurt[i], share);
			}

			if (gain > bestGain)
			{
				bestGain = gain;
				resting = n;
			}
		}

		var wanted = new List<int>();
		for (var i = 0; i < resting; i++)
		{
			wanted.Add(hurt[i].Position);
		}

		CrucibleCampSummary = resting > 0
			? $"RSR selects {DescribeResting(pets, wanted, current, max)} - {Outcome(self + RestShare(resting), need)}"
			: hurt.Count == 0
				? "RSR selects nobody - no familiar here is standing, short of HP and able to rest without losing its feed for less"
				: $"RSR selects nobody - {Shortfall(self, need)}";
		var ranked = new List<(int Position, float Share, bool Short, BeastmasterPet Pet)>(hurt.Count);
		foreach (var familiar in hurt)
		{
			ranked.Add((familiar.Position, familiar.Share, familiar.Short, familiar.Pet));
		}

		RecommendForFinal(ranked, self);

		// The window's "Apply": his choice from the recommendation, clicked toward like RSR's own.
		if (_campsiteRequested is { } requested)
		{
			wanted.Clear();
			for (var i = 0; i < pets.Length; i++)
			{
				if (requested.Contains((int)pets[i].PetId))
				{
					wanted.Add(i);
				}
			}
		}

		// One click toward the pick: take off whoever should not rest, then add the next one to rest.
		var target = -1;
		var add = false;
		foreach (var position in selected)
		{
			if (!wanted.Contains(position))
			{
				target = position;
				break;
			}
		}

		for (var i = 0; target < 0 && i < wanted.Count; i++)
		{
			if (!selected.Contains(wanted[i]))
			{
				target = wanted[i];
				add = true;
			}
		}

		if (target < 0)
		{
			var plan = string.Join(",", wanted);
			if (plan != _campsiteTraced)
			{
				_campsiteTraced = plan;
				DefenseTrace.Line(resting > 0
					? $"crucible campsite: rest with {resting} ({DescribeResting(pets, wanted, current, max)}) - "
						+ $"{Outcome(self + RestShare(resting), need)}, {RestShare(resting):P0} each brings them {bestGain:P0} of their HP in all"
					: hurt.Count == 0
						? "crucible campsite: rest alone - no familiar here is standing, short of HP and able to rest without losing its feed for less"
						: _campsiteLimit == 0
							? "crucible campsite: rest alone - this campsite takes no familiar"
							: $"crucible campsite: rest alone - {Shortfall(self, need)} ({RestShare(1):P0} each)");
			}

			return;
		}

		ReadOnlySpan<int> sent = [1, target];
		var values = stackalloc AtkValue[sent.Length];
		for (var i = 0; i < sent.Length; i++)
		{
			values[i] = default;
			values[i].Type = AtkValueType.Int;
			values[i].Int = sent[i];
		}

		screen->FireCallback((uint)sent.Length, values, true);
		_campsiteClick = (target, add, selected.Count, now);

		static void Hold(string why)
		{
			_campsiteHeld = true;
			CrucibleCampSummary = why;
			DefenseTrace.Line(why);
		}

		// What a rest brings a familiar, as a share of its HP: all it misses where the rest fills it (A669), else the share
		// each gets, at most what it misses.
		static float Brings((int Position, float Share, bool Short, BeastmasterPet Pet, bool Fills, float Need) familiar, float each)
		{
			return familiar.Fills ? familiar.Need : Math.Min(familiar.Need, each);
		}
	}

	/// <summary>
	/// K10 (A661): the heaviest way from where the run stands to the next campsite - the final fight included on a way
	/// without one -, each fight with the heaviest damage it ever dealt the player, with the gear worn now; null where a fight
	/// on those ways was never measured or the board's ways are unknown.
	/// </summary>
	internal static float? StretchNeed()
	{
		return CrucibleRoute.WorstAhead(true, LoadNow);
	}

	/// <summary>
	/// K10 (A661): the heaviest damage <paramref name="battle"/> ever dealt the player, as a share of his maximum HP with the
	/// gear worn now - the learned share without gear, times the damage he takes, over his raised maximum (his
	/// vulnerabilities by kind left out: the learned share carries no kind, so it errs high); null before it was measured.
	/// </summary>
	internal static float? LoadNow(uint battle)
	{
		var (hp, taken, _, _) = WornFactors();
		return CrucibleBoard.BattleLoad(battle) is { } bare ? bare * taken / Math.Max(hp, float.Epsilon) : null;
	}

	// K10 (A661): whether the player's HP after a rest (a share, above 1 when the rest would fill more than he misses) carry
	// the way ahead - above its load, or full where that is unknown or a full bar or more.
	internal static bool Carries(float after, float? need)
	{
		return need is { } stretch && stretch < 1f ? after > stretch : after >= 1f;
	}

	private static string Outcome(float after, float? need)
	{
		return after >= 1f
			? "you come out at full HP"
			: $"you come out at {after:P0}, above the {need ?? 1f:P0} the heaviest way to the next rest can cost you";
	}

	private static string Shortfall(float self, float? need)
	{
		var one = Math.Min(1f, self + RestShare(1));
		return need is { } stretch && stretch < 1f
			? $"with a familiar you would come out at {one:P0}, not above the {stretch:P0} the heaviest way to the next rest can cost you"
			: $"with a familiar you would come out at {one:P0}, not full - "
				+ (need == null ? "a fight on the ways to the next rest was never measured with RSR" : "the way to the next rest can cost a full bar");
	}

	/// <summary>
	/// The share of his maximum HP each of the player and <paramref name="familiars"/> familiars recover in one rest: the
	/// whole of this campsite as read here, else the smallest whole read on this board, else the smallest known, divided
	/// among everyone resting, at most all of one's HP (concept 21, K10 Campsite, A667).
	/// </summary>
	private static float RestShare(int familiars)
	{
		return Math.Min(1f, RestPool() / (1 + familiars));
	}

	private static float RestPool()
	{
		var board = CrucibleRoute.LastBattle.Board;
		if (_restPoolRead.TryGetValue((board, CrucibleRoute.Current), out var here))
		{
			return here;
		}

		lock (OtherConfiguration.CrucibleCampPoolsGate)
		{
			return OtherConfiguration.CrucibleCampPools.TryGetValue(board, out var percent)
				? percent / (float)CrucibleBoard.PercentScale
				: RestAloneShare;
		}
	}

	// The rest prompt, while the campsite window is open: the share it offers for the familiars selected now, of which
	// <paramref name="filling"/> a rest fills (A669: taken as not sharing the whole - the smaller whole). True while a prompt
	// is open over the window.
	private static unsafe bool ReadOfferedShare(int familiars, int filling)
	{
		var prompt = (AddonSelectYesno*)Svc.GameGui.GetAddonByName("SelectYesno").Address;
		if (prompt == null || !prompt->IsVisible)
		{
			return false;
		}

		if (prompt->PromptText == null
			|| OfferedShare.Match(prompt->PromptText->NodeText.ToString()) is not { Success: true } match
			|| !float.TryParse(match.Groups["share"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var percent))
		{
			return true;
		}

		// Capped at all of one's HP, a share tells only "at least": the whole is known from a share below it.
		var share = percent / (float)CrucibleBoard.PercentScale;
		var key = (CrucibleRoute.LastBattle.Board, CrucibleRoute.Current);
		var pool = share * (1 + familiars - filling);
		if (share < 1f && (!_restPoolRead.TryGetValue(key, out var known) || known != pool))
		{
			var expected = RestShare(familiars);
			_restPoolRead[key] = pool;
			DefenseTrace.Line($"crucible campsite: the game offers {share:P0} each with {familiars} familiars, RSR expected {expected:P0}"
				+ (filling > 0 ? $" ({filling} of them filled by their feed, counted as not sharing)" : string.Empty)
				+ $" - a rest here shares {pool:P0}");
			var whole = (int)Math.Round(pool * CrucibleBoard.PercentScale);
			var learned = false;
			lock (OtherConfiguration.CrucibleCampPoolsGate)
			{
				if (!OtherConfiguration.CrucibleCampPools.TryGetValue(key.Board, out var stored) || whole < stored)
				{
					OtherConfiguration.CrucibleCampPools[key.Board] = whole;
					learned = true;
				}
			}

			if (learned)
			{
				_ = OtherConfiguration.SaveCrucibleCampPools();
			}
		}

		return true;
	}

	// K14 (A631): at the last campsite before the final fight, the familiars that should rest for it - healers and protectors
	// of the final fight's plan first, then the rest of that plan, then the others by their share of HP - as many as the
	// campsite takes, with the share the player comes out with.
	private static void RecommendForFinal(List<(int Position, float Share, bool Short, BeastmasterPet Pet)> hurt, float self)
	{
		if (CrucibleRoute.CampsiteAfterAnotherBattle() != false || hurt.Count == 0)
		{
			CrucibleCampRecommendation = [];
			CrucibleCampOutcome = string.Empty;
			return;
		}

		var ranked = new List<(int Position, float Share, bool Short, BeastmasterPet Pet)>(hurt);
		ranked.Sort((a, b) =>
		{
			var order = FinalHealers.Contains(b.Pet).CompareTo(FinalHealers.Contains(a.Pet));
			if (order == 0)
			{
				order = FinalTeam.Contains(b.Pet).CompareTo(FinalTeam.Contains(a.Pet));
			}

			return order != 0 ? order : a.Share.CompareTo(b.Share);
		});

		var count = Math.Min(ranked.Count, _campsiteLimit ?? ranked.Count);
		var picks = new List<CrucibleCampPick>();
		for (var i = 0; i < count; i++)
		{
			var (_, share, _, pet) = ranked[i];
			picks.Add(new CrucibleCampPick(pet, share, FinalHealers.Contains(pet)
				? "heals or protects in the final fight's plan"
				: FinalTeam.Contains(pet) ? "fights in the final fight's plan" : "the most hurt of the others"));
		}

		CrucibleCampRecommendation = picks;
		var traced = string.Join(", ", picks.ConvertAll(static pick => pick.Pet.ToString()));
		if (traced != _finalRestTraced)
		{
			_finalRestTraced = traced;
			DefenseTrace.Line($"crucible campsite: last before the final fight - for it rest with {traced} "
				+ $"(final plan {string.Join(", ", FinalTeam)}, healers {string.Join(", ", FinalHealers)}; at most {_campsiteLimit?.ToString() ?? "unknown"})");
		}

		var each = RestShare(count);
		var after = Math.Min(1f, self + each);
		CrucibleCampOutcome = $"Last campsite before the final fight: each gets {each:P0} of their HP; you come out at {after:P0}"
			+ (after < 1f ? $" instead of full (resting alone: {Math.Min(1f, self + RestShare(0)):P0})" : string.Empty)
			+ (FinalTeam.Count == 0 ? " - no final-fight plan yet (it is made at a horn screen before another fight)" : string.Empty);

	}

	// K14 (A631): while a familiar is to be fed, the one the feed is meant for - the familiar of the final fight's plan the feed
	// suits best (BeastmasterCrucibleShop) - shown, never clicked.
	private static unsafe void FollowFeeding(AgentXBMPetParty* party, AtkUnitBase* screen, bool feeding)
	{
		if (!feeding || party == null || screen == null)
		{
			CrucibleFeedTarget = string.Empty;
			return;
		}

		var item = (ushort)new ReaderXBMPetParty(screen).FeedItem.ItemId;
		CrucibleFeedTarget = item == 0 ? "Feeding: no feed read in the window" : FeedTargetText(item);
	}

	private static string DescribeResting(Span<AgentXBMPetParty.PartyEntry> pets, List<int> wanted, Span<uint> current, Span<uint> max)
	{
		var parts = new List<string>();
		foreach (var position in wanted)
		{
			parts.Add($"{FamiliarName((BeastmasterPet)pets[position].PetId)} ({current[position]}/{max[position]})");
		}

		return string.Join(", ", parts);
	}
}
