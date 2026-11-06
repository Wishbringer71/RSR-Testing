using System.Globalization;
using System.Text.RegularExpressions;
using ECommons.DalamudServices;
using ECommons.UIHelpers.AtkReaderImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace RotationSolver.Basic.Rotations.Basic;

/// <summary>
/// K10 of the Crucible (concept 01, "K10 – Zwischen den Kämpfen", Campsite, A564): at a campsite RSR picks who rests with
/// the player - familiars only when he still comes out at full HP, so he never comes out worse than resting alone; the
/// number that brings the familiars the most HP, the most hurt standing, unfed ones. Resting stays his click.
/// </summary>
public partial class BeastmasterRotation
{
	// The campsite window: AgentXBMPetParty active in this mode with XBMPetParty visible (Daedalus 74659a6, CrucibleAutoPilot,
	// in the field 25. and 27.10.2026). FFXIVClientStructs aad0de6 calls the mode "buy feed result", so every click is
	// checked against the selection it should have made.
	internal const uint CampsiteMode = 4;

	// What a rest restores alone, of the player's maximum HP; with n familiars each gets this divided by 1 + n (wiki: 90 %
	// alone, 45 % with one, 30 % with two; 45 % in the game's prompt in Daedalus' field note, +30 % in his measurement of
	// 04.11.2026). Only until the game's prompt has been read once: from then on its percentage counts.
	private const float RestAloneShare = 0.9f;

	// The share the game's rest prompt offers, by the number of familiars resting ("Rest and recover 45% of HP for you
	// and your: ...", Addon 17660): any number before a percent sign, in any client language.
	private static readonly Regex OfferedShare = new(@"(?<share>\d+)\s*%", RegexOptions.Compiled);
	private static readonly Dictionary<int, float> _restShareOffered = [];

	private static int? _campsiteLimit;
	private static bool _campsiteHeld;
	private static string? _campsiteSelection;
	private static bool _campsiteSettled;
	private static DateTime _campsiteSince;
	private static string _campsiteTraced = string.Empty;
	private static (int Position, bool Select, int Before, DateTime At)? _campsiteClick;

	/// <summary>
	/// K10: with the campsite window open, one click a time toward the pick: no familiar when the player would not come
	/// out at full HP with one; else the number of familiars that brings them the most HP while he still does, and for it
	/// the standing, unfed familiars not at full HP with the smallest share of their HP. Never presses rest. Called every
	/// frame from the update; <paramref name="pick"/> is the rotation's setting.
	/// </summary>
	internal static unsafe void FollowCampsite(bool pick)
	{
		var party = AgentXBMPetParty.Instance();
		var player = Player;
		var screen = (AtkUnitBase*)Svc.GameGui.GetAddonByName("XBMPetParty").Address;
		if (!pick || !DataCenter.IsInCrucible || InCombat || party == null || player == null || player.MaxHp == 0
			|| !((AgentInterface*)party)->IsAgentActive() || party->Mode != CampsiteMode || screen == null || !screen->IsVisible)
		{
			_campsiteLimit = null;
			_campsiteHeld = false;
			_campsiteSelection = null;
			_campsiteSettled = false;
			_campsiteTraced = string.Empty;
			_campsiteClick = null;
			return;
		}

		var selected = new List<int>();
		foreach (var chosen in party->SelectedPetIds.AsSpan())
		{
			selected.Add((int)chosen.PetId);
		}

		// While the rest prompt is open it speaks for the selection as it stands; nothing is clicked under it.
		if (ReadOfferedShare(selected.Count))
		{
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
		var pets = party->SelectedPets.AsSpan();
		var current = party->PetCurrentHealth;
		var max = party->PetMaxHealth;
		var rows = new ReaderXBMPetParty(screen).TeamEntries;
		if (rows.Count != pets.Length)
		{
			Hold($"crucible campsite: the window lists {rows.Count} familiars, the team {pets.Length} - left to you");
			return;
		}

		var hurt = new List<(int Position, float Share, bool Short)>();
		for (var i = 0; i < pets.Length && i < current.Length && i < max.Length; i++)
		{
			// The window's rows carry the feed; they are read only where their HP is the team's at the same position.
			if (rows[i].MaxHP != max[i] || rows[i].HP != current[i])
			{
				Hold($"crucible campsite: the window's HP of position {i} ({rows[i].HP}/{rows[i].MaxHP}) is not the team's "
					+ $"({current[i]}/{max[i]}) - left to you");
				return;
			}

			// A fallen familiar is not healed there, one at full HP takes a share for nothing, a fed one loses its feed.
			if (current[i] > 0 && current[i] < max[i] && rows[i].FedCurrent == 0)
			{
				hurt.Add((i, current[i] / (float)max[i], ShortForFinal((BeastmasterPet)pets[i].PetId, current[i])));
			}
		}

		// K11 stage 2 (A590): a familiar the final fight needs to heal or protect, short of HP for it, rests first.
		hurt.Sort((a, b) => a.Short != b.Short ? b.Short.CompareTo(a.Short) : a.Share.CompareTo(b.Share));
		var self = player.CurrentHp / (float)player.MaxHp;
		var resting = 0;
		var bestGain = 0f;
		for (var n = 1; n <= hurt.Count && (_campsiteLimit is not { } limit || n <= limit); n++)
		{
			var share = RestShare(n);
			if (self + share < 1f)
			{
				continue;
			}

			var gain = 0f;
			for (var i = 0; i < n; i++)
			{
				gain += Math.Min(1f - hurt[i].Share, share);
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
					? $"crucible campsite: rest with {resting} ({DescribeResting(pets, wanted, current, max)}) - you come out at full HP, "
						+ $"{RestShare(resting):P0} each brings them {bestGain:P0} of their HP in all"
					: hurt.Count == 0
						? "crucible campsite: rest alone - no familiar here is standing, unfed and short of HP"
						: _campsiteLimit == 0
							? "crucible campsite: rest alone - this campsite takes no familiar"
							: $"crucible campsite: rest alone - at {self:P0} you would not come out at full HP with a familiar "
								+ $"({RestShare(1):P0} each)");
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
			DefenseTrace.Line(why);
		}
	}

	/// <summary>The share of his maximum HP each of the player and <paramref name="familiars"/> familiars recover in one rest.</summary>
	private static float RestShare(int familiars)
	{
		if (_restShareOffered.TryGetValue(familiars, out var offered))
		{
			return offered;
		}

		// Any prompt read so far tells the whole: the game divides one rest among everyone resting.
		foreach (var (count, share) in _restShareOffered)
		{
			return share * (1 + count) / (1 + familiars);
		}

		return RestAloneShare / (1 + familiars);
	}

	// The rest prompt, while the campsite window is open: the share it offers for the familiars selected now. True while a
	// prompt is open over the window.
	private static unsafe bool ReadOfferedShare(int familiars)
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

		var share = percent / 100f;
		if (!_restShareOffered.TryGetValue(familiars, out var known) || known != share)
		{
			var expected = RestShare(familiars);
			_restShareOffered[familiars] = share;
			DefenseTrace.Line($"crucible campsite: the game offers {share:P0} each with {familiars} familiars, RSR expected {expected:P0}");
		}

		return true;
	}

	private static string DescribeResting(Span<AgentXBMPetParty.PartyEntry> pets, List<int> wanted, Span<uint> current, Span<uint> max)
	{
		var parts = new List<string>();
		foreach (var position in wanted)
		{
			parts.Add($"{(BeastmasterPet)pets[position].PetId} at {current[position] / (float)max[position]:P0}");
		}

		return string.Join(", ", parts);
	}
}
