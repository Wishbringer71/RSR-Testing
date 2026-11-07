using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Space = RotationSolver.Basic.Helpers.CrucibleRoute.Space;

namespace RotationSolver.Basic.Rotations.Basic;

/// <summary>
/// K10 "Weg" of the Crucible (concept 21, A632, A656; the owner's rules of 06.11.2026 and 07.11.2026): at the next fork of the
/// board, which way to take - once the space the run stands on is done, by the player's HP and healing items, the final fight's
/// healers and the measured load of a fight, up to the next rest after the ways meet again; each way named by its side on the
/// drawn board and its spaces, the recommendation with its reason against the others.
/// </summary>
public partial class BeastmasterRotation
{
	/// <summary>One way at the next fork: its side and spaces, where it leaves the player and the healers, whether it is safe.</summary>
	public readonly record struct CrucibleRouteOption(string Way, string Outcome, bool Safe, bool Recommended);

	/// <summary>The ways at the next fork, left to right, the recommended one marked; empty without a fork ahead.</summary>
	public static IReadOnlyList<CrucibleRouteOption> CrucibleRouteHint { get; private set; } = [];

	/// <summary>Why the recommended way, against the others.</summary>
	public static string CrucibleRouteWhy { get; private set; } = string.Empty;

	/// <summary>From which space RSR computes, and with what load.</summary>
	public static string CrucibleRouteBasis { get; private set; } = string.Empty;

	private static string _routeTraced = string.Empty;

	// The state a way is walked with (A663: to the final fight, included): the player's share of HP, the lowest share of the
	// final fight's healers and that share as the final fight starts, how many spaces with items it passes (treasure, shop, a
	// random space), whether it passes a campsite, the space it ends on (the boss; -1 the end), and whether a fight on it
	// has no load known yet; (A671) the smoke bombs left and the move of the first enemy space the way flees with one.
	private readonly record struct RouteState(float Player, float Healer, int Items, bool Rested, int Stop, float HealerAtFinal = 1f,
		bool Unknown = false, int Escapes = 0, int FleeMove = -1);

	/// <summary>
	/// Follows the run on the board every frame - the campsite, shop or treasure window that opens places it, a fight that ends
	/// finishes its space - and, when <paramref name="hint"/>, computes the hint for the next fork once the space is done.
	/// </summary>
	internal static unsafe void FollowCrucibleRoute(bool hint)
	{
		if (!DataCenter.IsInCrucible)
		{
			ClearRouteHint();
			return;
		}

		CrucibleRoute.NoteCombat(InCombat);
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

		// The way is chosen after the space's fight, with the HP that fight left (his rule of 07.11.2026).
		var at = CrucibleRoute.Current;
		if (!hint || InCombat || !CrucibleRoute.CurrentDone || CrucibleRoute.Map is not { } map || at < 0 || at >= map.Next.Length
			|| map.Next[at].Count <= 1 || Player is not { MaxHp: > 0 } player)
		{
			ClearRouteHint();
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

		// Where the ways meet again: the earliest space every way reaches.
		var reach = new List<HashSet<int>>();
		foreach (var next in map.Next[at])
		{
			reach.Add(Reachable(next));
		}

		var merge = -1;
		for (var i = 0; i < map.Nodes.Length; i++)
		{
			var all = true;
			foreach (var set in reach)
			{
				all &= set.Contains(i);
			}

			if (all && (merge < 0 || map.Nodes[i].Move < map.Nodes[merge].Move))
			{
				merge = i;
			}
		}

		// A671: the smoke bombs held, and what the gear worn does on the way.
		var (escapeChance, treasureItems) = WornWayEffects();
		var start = new RouteState(self, healer, 0, false, -1, healer, Escapes: SmokeBombsHeld());
		var options = new List<(int Next, float Turn, string Side, string Way, RouteState End)>();
		foreach (var next in map.Next[at])
		{
			var (turn, side) = SideOf(next);
			var fields = Fields(next);
			options.Add((next, turn, side, side.Length > 0 ? $"{side}: {fields}" : fields, Walk(next, start, false, 0)));
		}

		// Left first, as the board is drawn from the start towards the final fight.
		options.Sort(static (a, b) => b.Turn.CompareTo(a.Turn));

		// A663: every way to the final fight weighed, or none - a fight with no load known leaves the old rule.
		var weighed = options.TrueForAll(static o => !o.End.Unknown);

		// Safe (A663): you live through every fight up to the final one, included, with the items held, and the final fight's
		// weakest healer outlasts a use as it starts.
		bool IsSafe(RouteState end) => end.Player + potions > 0f && (loss == 0f || end.HealerAtFinal > loss);

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
			var end = options[i].End;
			hintList.Add(new CrucibleRouteOption(options[i].Way, !weighed
				? $"you {self:P0}, final healers {healer:P0} now"
				: $"after {StopName(end.Stop)}: you ~{end.Player:P0} (+{potions:P0} in healing items), final healers ~{end.HealerAtFinal:P0} as it starts"
					+ (end.Rested ? ", rested" : string.Empty) + (end.Items > 0 ? $", {end.Items} item(s)" : string.Empty)
					+ (end.FleeMove >= 0 ? $", fleeing the fight at move {end.FleeMove} with a smoke bomb" : string.Empty),
				weighed && IsSafe(end), i == best));
		}

		CrucibleRouteHint = hintList;
		CrucibleRouteWhy = Why();
		CrucibleRouteBasis = $"From the {map.Nodes[at].Kind} at move {map.Nodes[at].Move}, after its fight; "
			+ (!weighed
				? "a fight ahead was never measured"
				: "each fight with the heaviest damage it ever dealt you, all fights up to the final one")
			+ (escapeChance > 0f ? $"; your gear offers escape from {escapeChance:P0} of the enemy spaces - take it when it does, it spares the fight's damage" : string.Empty);
		var traced = $"{at}:{best}";
		if (traced != _routeTraced)
		{
			_routeTraced = traced;
			DefenseTrace.Line($"crucible route: at move {map.Nodes[at].Move} recommends {options[best].Way} - "
				+ string.Join("; ", hintList.ConvertAll(static o => $"{o.Way}: {o.Outcome}{(o.Safe ? ", safe" : string.Empty)}"))
				+ $" - {CrucibleRouteWhy}");
		}

		// Among safe ways the one with more spaces with items, else the more HP; with none safe the more HP, then the healthier
		// healers. A663: spaces with items counted, so that the shop every way passes decides nothing.
		bool Better(RouteState a, RouteState b)
		{
			if (a.Unknown || b.Unknown)
			{
				var hurt = self < 1f || healer < 1f;
				return hurt ? a.Rested && !b.Rested : a.Items > b.Items;
			}

			bool safeA = IsSafe(a), safeB = IsSafe(b);
			if (safeA != safeB)
			{
				return safeA;
			}

			if (safeA && a.Items != b.Items)
			{
				return a.Items > b.Items;
			}

			return a.Player != b.Player ? a.Player > b.Player : a.HealerAtFinal > b.HealerAtFinal;
		}

		// The recommended way against each other one, in what the other would have brought.
		string Why()
		{
			var (_, _, side, way, end) = options[best];
			var name = side.Length > 0 ? side.ToLowerInvariant() : way;
			if (!weighed)
			{
				return $"Take the {name} way: a fight ahead was never measured - "
					+ (self < 1f || healer < 1f ? "the campsite while you or a final healer is hurt" : "the way with items, nothing is hurt");
			}

			var reasons = new List<string>();
			for (var i = 0; i < options.Count; i++)
			{
				if (i == best)
				{
					continue;
				}

				var other = options[i];
				var otherName = other.Side.Length > 0 ? $"the {other.Side.ToLowerInvariant()} way" : other.Way;
				if (!IsSafe(end))
				{
					reasons.Add($"no way is safe; this one leaves you highest (~{end.Player + potions:P0} with items after {StopName(end.Stop)})");
				}
				else if (!IsSafe(other.End))
				{
					reasons.Add(other.End.Player + potions <= 0f
						? $"{otherName} would leave you at ~{other.End.Player:P0} after {StopName(other.End.Stop)} with {potions:P0} in items - "
							+ "the fights up to it can cost more than you hold"
						: $"{otherName} would bring a final healer to the final fight at ~{other.End.HealerAtFinal:P0}, under one of its losses a use");
				}
				else if (end.Items > other.End.Items)
				{
					reasons.Add($"both ways are safe up to {StopName(end.Stop)}; {otherName}"
						+ (other.End.Rested ? " and its campsite" : string.Empty)
						+ $" would leave you {other.End.Player - end.Player:+0%;-0%;+0%} and the final healers "
						+ $"{other.End.HealerAtFinal - end.HealerAtFinal:+0%;-0%;+0%}, this one passes {end.Items - other.End.Items} more space(s) with items");
				}
				else
				{
					reasons.Add($"both ways are safe; this one leaves you {end.Player - other.End.Player:+0%;-0%;+0%} and the final healers "
						+ $"{end.HealerAtFinal - other.End.HealerAtFinal:+0%;-0%;+0%} against {otherName} after {StopName(end.Stop)}");
				}
			}

			return $"Take the {name} way: {string.Join("; ", reasons)}."
				+ (end.FleeMove >= 0 ? $" Flee the fight at move {end.FleeMove} with a smoke bomb: it is the one that spares you most on this way." : string.Empty);
		}

		// The side of a way as the board is drawn, seen from the start towards the final fight: the sign of the turn from the
		// board's direction to the step onto the way (the grid's rows grow south, so they are turned round). Empty when the map
		// draws none of the spaces.
		(float Turn, string Side) SideOf(int next)
		{
			int first = -1, last = -1;
			for (var i = 0; i < map.Nodes.Length; i++)
			{
				if (map.Nodes[i].Kind == Space.Start)
				{
					first = i;
				}
				else if (map.Nodes[i].Kind == Space.Boss)
				{
					last = i;
				}
			}

			if (first < 0 || last < 0 || !Drawn(first) || !Drawn(last) || !Drawn(at) || !Drawn(next))
			{
				return (0f, string.Empty);
			}

			float forwardX = map.Cells[last].X - map.Cells[first].X, forwardY = map.Cells[first].Y - map.Cells[last].Y;
			float stepX = map.Cells[next].X - map.Cells[at].X, stepY = map.Cells[at].Y - map.Cells[next].Y;
			var turn = (forwardX * stepY) - (forwardY * stepX);
			return (turn, turn > 0f ? "Left" : turn < 0f ? "Right" : "Straight on");
		}

		bool Drawn(int index) => index < map.Cells.Length && map.Cells[index].X >= 0 && map.Cells[index].Y >= 0;

		// The spaces of a way up to where the ways meet again.
		string Fields(int next)
		{
			var parts = new List<string>();
			var index = next;
			while (index >= 0 && index != merge && parts.Count < map.Nodes.Length)
			{
				parts.Add(FieldName(map.Nodes[index]));
				if (map.Next[index].Count != 1)
				{
					if (map.Next[index].Count > 1)
					{
						parts.Add("a fork");
					}

					break;
				}

				index = map.Next[index][0];
			}

			return string.Join(", then ", parts);
		}

		HashSet<int> Reachable(int from)
		{
			var seen = new HashSet<int>();
			var queue = new Queue<int>();
			queue.Enqueue(from);
			while (queue.Count > 0)
			{
				var index = queue.Dequeue();
				if (index < 0 || index >= map.Nodes.Length || !seen.Add(index))
				{
					continue;
				}

				foreach (var next in map.Next[index])
				{
					queue.Enqueue(next);
				}
			}

			return seen;
		}

		string StopName(int stop) => stop < 0
			? "the end of the board"
			: map.Nodes[stop].Kind == Space.Boss ? "the final fight" : $"the campsite at move {map.Nodes[stop].Move}";

		// A way from a space: through it and on to the final fight, included (A663, his rule of 07.11.2026: "sich immer auf die
		// summe der anstehenden kämpfe ausrichten"); at a fork the better way, a random space at worst.
		RouteState Walk(int index, RouteState state, bool merged, int depth)
		{
			var node = map.Nodes[index];
			merged |= index == merge;
			if (node.Kind == Space.Boss)
			{
				return Apply(node.Kind, node.Battle, index, state with { HealerAtFinal = state.Healer }) with { Stop = index };
			}

			if (depth > map.Nodes.Length)
			{
				return state with { Stop = index };
			}

			if (node.Kind == Space.Random && node.Options.Count > 0)
			{
				RouteState? worst = null;
				foreach (var (kind, battle) in node.Options)
				{
					var end = Continue(index, Apply(kind, battle, index, state), merged, depth);
					if (worst is not { } w || Better(w, end))
					{
						worst = end;
					}
				}

				return worst ?? state;
			}

			// A671: at an enemy space with a smoke bomb left, fight or flee, the better of the two ways on.
			var fought = Continue(index, Apply(node.Kind, node.Battle, index, state), merged, depth);
			if (node.Kind != Space.Enemy || state.Escapes <= 0)
			{
				return fought;
			}

			var fled = Continue(index, state with
			{
				Escapes = state.Escapes - 1,
				FleeMove = state.FleeMove >= 0 ? state.FleeMove : node.Move,
			}, merged, depth);
			return Better(fled, fought) ? fled : fought;
		}

		RouteState Continue(int index, RouteState state, bool merged, int depth)
		{
			if (map.Next[index].Count == 0)
			{
				return state with { Stop = -1 };
			}

			RouteState? bestEnd = null;
			foreach (var next in map.Next[index])
			{
				var end = Walk(next, state, merged, depth + 1);
				if (bestEnd is not { } b || Better(end, b))
				{
					bestEnd = end;
				}
			}

			return bestEnd ?? state;
		}

		RouteState Apply(Space kind, uint battle, int index, RouteState state)
		{
			switch (kind)
			{
				case Space.Campsite:
					// As K10 picks (A661): familiars rest while the player's HP after it carry the heaviest way to the next rest.
					var limit = int.MaxValue;
					foreach (var camp in map.Nodes[index].Camps)
					{
						if (camp >= 0 && camp < map.Camps.Count)
						{
							limit = Math.Min(limit, map.Camps[camp]);
						}
					}

					var resting = 0;
					var need = CrucibleRoute.WorstAhead(index, true, true, LoadNow);
					for (var n = 1; n <= limit && n <= team; n++)
					{
						if (Carries(state.Player + RestShare(n), need))
						{
							resting = n;
						}
					}

					var each = RestShare(resting);
					return state with
					{
						Player = Math.Min(1f, state.Player + each),
						Healer = resting > 0 ? Math.Min(1f, state.Healer + each) : state.Healer,
						Rested = true,
					};
				case Space.Shop:
					return state with { Items = state.Items + 1 };
				case Space.Treasure:
					return state with { Items = state.Items + treasureItems };
				// A661: a fight with the heaviest damage it ever dealt you; one never measured with the mean or heaviest here,
				// and with neither unknown. A663: the final fight's healer loses a use only while one would be left after it -
				// else K11 holds it - and the final fight itself takes none from the share it starts it with.
				case Space.Enemy or Space.Elite or Space.Boss:
					var fallback = loads == null ? (float?)null : kind == Space.Enemy ? mean : heaviest;
					var cost = LoadNow(battle) ?? fallback;
					var fights = kind != Space.Boss && loss > 0f && state.Healer >= loss + loss;
					return state with
					{
						Player = state.Player - (cost ?? 0f),
						Healer = fights ? state.Healer - loss : state.Healer,
						Unknown = state.Unknown || cost == null,
					};
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

	private static void ClearRouteHint()
	{
		CrucibleRouteHint = [];
		CrucibleRouteWhy = string.Empty;
		CrucibleRouteBasis = string.Empty;
	}

	private static string FieldName(CrucibleRoute.Node node)
	{
		return node.Kind switch
		{
			Space.Enemy => "fight",
			Space.Elite => "elite fight",
			Space.Boss => "final fight",
			Space.Shop => "shop",
			Space.Campsite => "campsite",
			Space.Treasure => "treasure",
			Space.Random => $"random ({string.Join(" or ", node.Options.ConvertAll(static o => o.Kind.ToString().ToLowerInvariant()))})",
			_ => node.Kind.ToString().ToLowerInvariant(),
		};
	}

	// A671: the smoke bombs held in the board's inventory.
	private static unsafe int SmokeBombsHeld()
	{
		var director = EventFramework.Instance() is var framework && framework != null ? framework->GetInstanceContentDirector() : null;
		if (director == null || director->InstanceContentType != InstanceContentType.CrucibleOfTheUnbroken)
		{
			return 0;
		}

		var crucible = (InstanceContentCrucibleOfTheUnbroken*)director;
		var count = 0;
		for (var slot = 0; slot < crucible->Inventory.Length; slot++)
		{
			if (TryItem(crucible, slot, CrucibleItemKind.Escape, out _))
			{
				count++;
			}
		}

		return count;
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
