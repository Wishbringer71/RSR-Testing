using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Space = RotationSolver.Basic.Helpers.CrucibleRoute.Space;

namespace RotationSolver.Basic.Rotations.Basic;

/// <summary>
/// K10 "Weg" of the Crucible (concept 01, A632; the owner's rule of 06.11.2026): at the next fork of the board, which way to
/// take - from where the run stands, one stretch ahead, by the player's HP and healing items, the final fight's healers and
/// the measured load of a fight; computed again on every space.
/// </summary>
public partial class BeastmasterRotation
{
	/// <summary>One way at the next fork: what it is, where it leaves the player and the healers, whether it is safe.</summary>
	public readonly record struct CrucibleRouteOption(string Way, string Outcome, bool Safe, bool Recommended);

	/// <summary>The ways at the next fork, the recommended one marked; empty without a fork ahead.</summary>
	public static IReadOnlyList<CrucibleRouteOption> CrucibleRouteHint { get; private set; } = [];

	/// <summary>Why, and from which space RSR computes.</summary>
	public static string CrucibleRouteBasis { get; private set; } = string.Empty;

	private static string _routeTraced = string.Empty;

	// The state a stretch is walked with: the player's share of HP, the lowest share of the final fight's healers, and whether
	// the way passes items (treasure, shop, a random space).
	private readonly record struct RouteState(float Player, float Healer, bool Items);

	/// <summary>
	/// Follows the run on the board every frame out of combat - the campsite, shop or treasure window that opens places it -
	/// and, when <paramref name="hint"/>, computes the hint for the next fork.
	/// </summary>
	internal static unsafe void FollowCrucibleRoute(bool hint)
	{
		if (!DataCenter.IsInCrucible)
		{
			CrucibleRouteHint = [];
			CrucibleRouteBasis = string.Empty;
			return;
		}

		if (!InCombat)
		{
			if (Visible("XBMContentsItemShop"))
			{
				CrucibleRoute.NoteSpace(Space.Shop);
			}
			else if (Visible("XBMContentsTreasure"))
			{
				CrucibleRoute.NoteSpace(Space.Treasure);
			}
			else if (AgentXBMPetParty.Instance() is var pets && pets != null && ((AgentInterface*)pets)->IsAgentActive()
				&& pets->Mode == CampsiteMode && !_feedFlow && Visible("XBMPetParty"))
			{
				// Read here, not from the campsite pick, so that turning that pick off does not lose the run's space.
				CrucibleRoute.NoteSpace(Space.Campsite);
			}
		}

		var at = CrucibleRoute.Current;
		if (!hint || InCombat || CrucibleRoute.Map is not { } map || at < 0 || at >= map.Next.Length || map.Next[at].Count <= 1
			|| Player is not { MaxHp: > 0 } player)
		{
			CrucibleRouteHint = [];
			CrucibleRouteBasis = string.Empty;
			return;
		}

		// The final fight's healer with the least share of HP, and its largest loss a use as a share of its HP.
		float healer = 1f, loss = 0f;
		var team = 1;
		var party = AgentXBMPetParty.Instance();
		if (party != null)
		{
			team = Math.Max(1, party->SelectedPets.AsSpan().Length);
		}

		if (party != null && FinalHealers.Count > 0)
		{
			var pets = party->SelectedPets.AsSpan();
			for (var i = 0; i < pets.Length && i < party->PetCurrentHealth.Length && i < party->PetMaxHealth.Length; i++)
			{
				if (FinalHealers.Contains((BeastmasterPet)pets[i].PetId) && party->PetMaxHealth[i] > 0)
				{
					var share = party->PetCurrentHealth[i] / (float)party->PetMaxHealth[i];
					if (share <= healer)
					{
						healer = share;
						loss = FinalLossPerUse / (float)party->PetMaxHealth[i];
					}
				}
			}
		}

		var self = player.CurrentHp / (float)player.MaxHp;
		var potions = HealingHeldShare();
		var loads = CrucibleBoard.Loads;
		var max = (float)player.MaxHp;
		float mean = 0f, heaviest = 0f;
		if (loads is { } measured)
		{
			mean = (float)Math.Max(0d, (measured.Mean.Damage - measured.Mean.Healed) / max);
			heaviest = (float)Math.Max(0d, (measured.Heaviest.Damage - measured.Heaviest.Healed) / max);
		}

		var start = new RouteState(self, healer, false);
		var options = new List<(string Way, RouteState End)>();
		foreach (var next in map.Next[at])
		{
			options.Add((Describe(map.Nodes[next]), Walk(next, start, 0)));
		}

		// Safe: one more heaviest fight in reserve with the items held, and no healer of the final fight short.
		bool IsSafe(RouteState end) => end.Player + potions >= heaviest && (loss == 0f || end.Healer >= loss + loss);

		var best = -1;
		for (var i = 0; i < options.Count; i++)
		{
			if (best < 0 || Better(options[i].End, options[best].End))
			{
				best = i;
			}
		}

		var hintList = new List<CrucibleRouteOption>();
		for (var i = 0; i < options.Count; i++)
		{
			var (way, end) = options[i];
			hintList.Add(new CrucibleRouteOption(way, loads == null
				? $"you {self:P0}, final healers {healer:P0} now"
				: $"after its next fight you ~{end.Player:P0} (+{potions:P0} in healing items), final healers ~{end.Healer:P0}"
					+ (end.Items ? ", passes items" : string.Empty),
				loads != null && IsSafe(end), i == best));
		}

		CrucibleRouteHint = hintList;
		CrucibleRouteBasis = $"From the {map.Nodes[at].Kind} at move {map.Nodes[at].Move}, one stretch ahead; "
			+ (loads == null
				? "no fight measured yet - the campsite while you or a final healer is hurt, else the way with items"
				: $"a fight costs ~{mean:P0} of your HP, the heaviest ~{heaviest:P0}");
		var traced = $"{at}:{best}";
		if (traced != _routeTraced)
		{
			_routeTraced = traced;
			DefenseTrace.Line($"crucible route: at move {map.Nodes[at].Move} recommends {options[best].Way} - "
				+ string.Join("; ", hintList.ConvertAll(static o => $"{o.Way}: {o.Outcome}{(o.Safe ? ", safe" : string.Empty)}")));
		}

		// Among safe ways the one with items, else the more HP; with none safe the more HP, then the healthier healers.
		bool Better(RouteState a, RouteState b)
		{
			if (loads == null)
			{
				var hurt = self < 1f || healer < 1f;
				return hurt ? a.Player + a.Healer > b.Player + b.Healer : a.Items && !b.Items;
			}

			bool safeA = IsSafe(a), safeB = IsSafe(b);
			if (safeA != safeB)
			{
				return safeA;
			}

			if (safeA && a.Items != b.Items)
			{
				return a.Items;
			}

			return a.Player != b.Player ? a.Player > b.Player : a.Healer > b.Healer;
		}

		// A stretch from a space: through it, to and through the next fight; at a fork the better way, a random space at worst.
		RouteState Walk(int index, RouteState state, int depth)
		{
			var node = map.Nodes[index];
			if (depth > map.Nodes.Length)
			{
				return state;
			}

			if (node.Kind == Space.Random && node.Options.Count > 0)
			{
				RouteState? worst = null;
				foreach (var (kind, _) in node.Options)
				{
					var end = Continue(index, Apply(kind, index, state with { Items = true }), kind, depth);
					if (worst is not { } w || Better(w, end))
					{
						worst = end;
					}
				}

				return worst ?? state;
			}

			return Continue(index, Apply(node.Kind, index, state), node.Kind, depth);
		}

		RouteState Continue(int index, RouteState state, Space kind, int depth)
		{
			if (kind is Space.Enemy or Space.Elite or Space.Boss || map.Next[index].Count == 0)
			{
				return state;
			}

			RouteState? best = null;
			foreach (var next in map.Next[index])
			{
				var end = Walk(next, state, depth + 1);
				if (best is not { } b || Better(end, b))
				{
					best = end;
				}
			}

			return best ?? state;
		}

		RouteState Apply(Space kind, int index, RouteState state)
		{
			switch (kind)
			{
				case Space.Campsite:
					// As K10 picks: familiars rest only while the player still comes out at full HP.
					var limit = int.MaxValue;
					foreach (var camp in map.Nodes[index].Camps)
					{
						if (camp >= 0 && camp < map.Camps.Count)
						{
							limit = Math.Min(limit, map.Camps[camp]);
						}
					}

					var resting = 0;
					for (var n = 1; n <= limit && n <= team; n++)
					{
						if (state.Player + RestShare(n) >= 1f)
						{
							resting = n;
						}
					}

					var each = RestShare(resting);
					return state with
					{
						Player = Math.Min(1f, state.Player + each),
						Healer = resting > 0 ? Math.Min(1f, state.Healer + each) : state.Healer,
					};
				case Space.Shop or Space.Treasure:
					return state with { Items = true };
				case Space.Enemy:
					return state with { Player = state.Player - mean, Healer = state.Healer - loss };
				case Space.Elite or Space.Boss:
					return state with { Player = state.Player - heaviest, Healer = state.Healer - loss };
				default:
					return state;
			}
		}

		static bool Visible(string name)
		{
			var addon = (AtkUnitBase*)Svc.GameGui.GetAddonByName(name).Address;
			return addon != null && addon->IsVisible;
		}
	}

	private static string Describe(CrucibleRoute.Node node)
	{
		return node.Kind switch
		{
			Space.Enemy or Space.Elite or Space.Boss => $"{node.Kind} (battle {node.Battle}) at move {node.Move}",
			Space.Random => $"Random at move {node.Move} ({string.Join(" or ", node.Options.ConvertAll(static o => o.Kind.ToString()))})",
			_ => $"{node.Kind} at move {node.Move}",
		};
	}

	// The healing items held for the player, as a share of his HP (K8's reading of their effect text).
	private static unsafe float HealingHeldShare()
	{
		var director = EventFramework.Instance() is var framework && framework != null ? framework->GetInstanceContentDirector() : null;
		if (director == null || director->InstanceContentType != InstanceContentType.CrucibleOfTheUnbroken)
		{
			return 0f;
		}

		var crucible = (InstanceContentCrucibleOfTheUnbroken*)director;
		var share = 0f;
		for (var slot = 0; slot < crucible->Inventory.Length; slot++)
		{
			if (TryItem(crucible, slot, CrucibleItemKind.Heal, out var item))
			{
				share += item.Value;
			}
		}

		return share;
	}
}
