using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;
using Dalamud.Game;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace RotationSolver.Basic.Rotations.Basic;

/// <summary>
/// K8 of the Crucible (concept 01, "Stufe 3: K8, Gegenstände des Bretts", A551 to A555): the board's items - healing below
/// the player's HP potion line, a cure for poison or blindness, Reraise, Sandstill before a fight, the Beast Potion Kit
/// against a boss, protection against an announced hit on the player or on a familiar about to fall, a serum or the Beast
/// Remedy Kit against the fight's dangerous statuses, an absorber against an elemental hit, the Crucible Needle for a
/// petrified familiar, more maximum HP against a boss, and the damage items - on a pack once a fight, freely in the final
/// fight, whose items are lost after the run.
/// </summary>
public partial class BeastmasterRotation
{
	private enum CrucibleItemKind : byte
	{
		Heal,
		Reraise,
		Sandstill,
		AutoPotion,
		CurePoison,
		CureBlindness,
		Mitigate,
		NullifyPhysical,
		ReflectMagic,
		AutoRemedy,
		ResistAilment,
		Damage,
		AbsorbElement,
		CureFamiliar,
		MaxHp,
		Buff,
	}

	// What an item of the board does, read once from its English effect text (XBMItem): its kind; the share it heals or
	// mitigates, the chance it revives with, or its potency; whether it reaches the allies nearby as well; its name; the
	// status it grants, or for a serum the ailment it resists; for a damage item its element (XBMElement), radius and whether
	// it knocks back.
	private readonly record struct CrucibleItem(CrucibleItemKind Kind, float Value, bool Allies, string Name, string Status = "",
		uint Element = 0, float Radius = 0f, bool Knockback = false);

	private static Dictionary<ushort, CrucibleItem>? _crucibleItems;

	// The last use: its slot, its item, when, and for a serum the status it covers; the slots the game did not take in this
	// fight; whether Reraise has gone without a fall, and whether it stood the frame before.
	private static (int Slot, ushort Item, DateTime At, uint Covers) _crucibleItemUsed = (-1, 0, DateTime.MinValue, 0);
	private static readonly HashSet<int> _crucibleSlotsRefused = [];
	private static bool _reraiseLostWithoutFall;
	private static bool _reraiseStood;
	private static bool _fellUnderReraise;

	// In this fight: the dangerous statuses a serum covers, the damage items used. For the session: the statuses Auto-remedy
	// was seen not to cure, and since when each dangerous status has stood on the player under Auto-remedy.
	private static readonly HashSet<uint> _serumCovers = [];
	private static int _damageItemsInFight;
	private static readonly HashSet<uint> _notCuredByRemedy = [];
	private static readonly Dictionary<uint, DateTime> _ailmentUnderRemedySince = [];

	private static readonly Regex HealsShare = new(@"^Restores (?<share>\d+)% of HP to self (?<whom>or familiar|and nearby allies)",
		RegexOptions.CultureInvariant);
	private static readonly Regex GrantsReraise = new(@"^Grants Reraise to self\..*?(?<share>\d+)% chance of automatic revival",
		RegexOptions.CultureInvariant | RegexOptions.Singleline);
	private static readonly Regex Cures = new(@"^Instantly cures most instances of (?<ailment>poison|blindness) for self",
		RegexOptions.CultureInvariant);
	private static readonly Regex GrantsStatus = new(@"^Grants (?:\w+ stacks of )?(?<status>[A-Z][\w' -]*?) to self(?: or familiar)?\.",
		RegexOptions.CultureInvariant);
	private static readonly Regex Mitigates = new(@"Reduces damage taken by (?<share>\d+)%", RegexOptions.CultureInvariant);
	private static readonly Regex Resists = new(@"^Increases (?<ailment>\w+) resistance by \d+% for self (?<whom>or familiar|and nearby allies)",
		RegexOptions.CultureInvariant);
	private static readonly Regex AbsorbsElement = new(
		@"^Grants (?<status>.+?) to self and nearby allies\..*?(?<share>\d+)% chance to absorb (?<type>[a-z]+) damage as HP",
		RegexOptions.CultureInvariant | RegexOptions.Singleline);
	private static readonly Regex CuresFamiliar = new(@"^Instantly cures most instances of (?<ailment>\w+) for familiar",
		RegexOptions.CultureInvariant);
	private static readonly Regex RaisesMaxHp = new(
		@"^Grants (?<status>.+?) while also restoring \d+% of HP to self or familiar\..*?Increases maximum HP by",
		RegexOptions.CultureInvariant | RegexOptions.Singleline);
	private static readonly Regex GrantsBuff = new(@"^Grants (?:\w+ stacks of )?(?<status>.+?)(?: while also| to self)",
		RegexOptions.CultureInvariant);
	private static readonly Regex Deals = new(
		@"deals (?:ranged )?(?<type>[a-z]+)(?: physical)? damage with a potency of (?<potency>[\d,]+) to target and all enemies within (?<radius>\d+) yalms",
		RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

	private static Dictionary<ushort, CrucibleItem> CrucibleItems => _crucibleItems ??= ReadCrucibleItems();

	private static Dictionary<ushort, CrucibleItem> ReadCrucibleItems()
	{
		var items = new Dictionary<ushort, CrucibleItem>();
		var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.XBMItem>(ClientLanguage.English);
		if (sheet == null)
		{
			return items;
		}

		foreach (var row in sheet)
		{
			var text = row.Unknown3.ExtractText();
			var name = row.Unknown2.ExtractText();
			var id = (ushort)row.RowId;
			if (HealsShare.Match(text) is { Success: true } heal && Share(heal, out var healed))
			{
				items[id] = new CrucibleItem(CrucibleItemKind.Heal, healed,
					heal.Groups["whom"].Value.StartsWith("and", StringComparison.Ordinal), name);
			}
			else if (GrantsReraise.Match(text) is { Success: true } reraise && Share(reraise, out var chance))
			{
				items[id] = new CrucibleItem(CrucibleItemKind.Reraise, chance, false, name);
			}
			else if (text.StartsWith("Grants Sandstill to self", StringComparison.Ordinal))
			{
				items[id] = new CrucibleItem(CrucibleItemKind.Sandstill, 0f, false, name);
			}
			else if (text.StartsWith("Grants Auto-potion to self", StringComparison.Ordinal))
			{
				items[id] = new CrucibleItem(CrucibleItemKind.AutoPotion, 0f, false, name);
			}
			else if (Cures.Match(text) is { Success: true } cure)
			{
				items[id] = new CrucibleItem(cure.Groups["ailment"].Value == "poison" ? CrucibleItemKind.CurePoison
					: CrucibleItemKind.CureBlindness, 0f, false, name);
			}
			else if (GrantsStatus.Match(text) is { Success: true } grants && ProtectionKind(text, out var kind, out var mitigates))
			{
				items[id] = new CrucibleItem(kind, mitigates, false, name, grants.Groups["status"].Value);
			}
			else if (AbsorbsElement.Match(text) is { Success: true } absorbs && Share(absorbs, out var absorbChance))
			{
				items[id] = new CrucibleItem(CrucibleItemKind.AbsorbElement, absorbChance, true, name, absorbs.Groups["status"].Value,
					CrucibleBoard.ElementNamed(absorbs.Groups["type"].Value));
			}
			else if (CuresFamiliar.Match(text) is { Success: true } curesFamiliar)
			{
				items[id] = new CrucibleItem(CrucibleItemKind.CureFamiliar, 0f, false, name, curesFamiliar.Groups["ailment"].Value);
			}
			else if (RaisesMaxHp.Match(text) is { Success: true } maxHp)
			{
				items[id] = new CrucibleItem(CrucibleItemKind.MaxHp, 0f, false, name, maxHp.Groups["status"].Value);
			}
			else if (Resists.Match(text) is { Success: true } resists)
			{
				items[id] = new CrucibleItem(CrucibleItemKind.ResistAilment, 0f,
					resists.Groups["whom"].Value.StartsWith("and", StringComparison.Ordinal), name, resists.Groups["ailment"].Value);
			}
			else if (Deals.Match(text) is { Success: true } deals
				&& float.TryParse(deals.Groups["potency"].Value, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var potency)
				&& float.TryParse(deals.Groups["radius"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var radius))
			{
				items[id] = new CrucibleItem(CrucibleItemKind.Damage, potency, false, name, string.Empty,
					CrucibleBoard.ElementNamed(deals.Groups["type"].Value), radius,
					text.Contains("knockback", StringComparison.OrdinalIgnoreCase));
			}
			else if (GrantsBuff.Match(text) is { Success: true } buff)
			{
				items[id] = new CrucibleItem(CrucibleItemKind.Buff, 0f, false, name, buff.Groups["status"].Value);
			}
		}

		return items;

		static bool Share(Match match, out float share)
		{
			var parsed = float.TryParse(match.Groups["share"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var percent);
			share = percent / 100f;
			return parsed;
		}

		// A status the item grants that mitigates, nullifies physical or reflects magic attacks, or cures the next ailment.
		static bool ProtectionKind(string text, out CrucibleItemKind kind, out float mitigates)
		{
			mitigates = 0f;
			kind = CrucibleItemKind.Mitigate;
			if (Mitigates.Match(text) is { Success: true } share && Share(share, out mitigates))
			{
				return true;
			}

			kind = text.Contains("Nullifies most physical attacks", StringComparison.Ordinal) ? CrucibleItemKind.NullifyPhysical
				: text.Contains("Reflects most magic attacks", StringComparison.Ordinal) ? CrucibleItemKind.ReflectMagic
				: text.Contains("Automatically cures the next status ailment", StringComparison.Ordinal) ? CrucibleItemKind.AutoRemedy
				: CrucibleItemKind.Heal;
			return kind != CrucibleItemKind.Heal;
		}
	}

	// The English names of the statuses, for the ones the items answer: Reraise, Sandstill, Auto-potion, poison, blindness,
	// the protections and Auto-remedy.
	private static readonly Dictionary<uint, string> _statusNames = [];

	private static bool PlayerHasStatusNamed(IBattleChara player, string fragment)
	{
		if (player.StatusList == null)
		{
			return false;
		}

		foreach (var status in player.StatusList)
		{
			if (status != null && status.StatusId != 0 && StatusName(status.StatusId).Contains(fragment, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}

		return false;
	}

	private static string StatusName(uint statusId)
	{
		if (!_statusNames.TryGetValue(statusId, out var name))
		{
			name = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(ClientLanguage.English)?.GetRowOrDefault(statusId)?.Name.ExtractText()
				?? string.Empty;
			_statusNames[statusId] = name;
		}

		return name;
	}

	private static bool HasStatusId(IBattleChara chara, uint statusId)
	{
		if (chara.StatusList == null)
		{
			return false;
		}

		foreach (var status in chara.StatusList)
		{
			if (status != null && status.StatusId == statusId)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// K8 (A551 to A553): on a board, one item at a time, used through the soft target. With <paramref name="safety"/>: in
	/// combat below <paramref name="line"/> of his HP the healing item that lifts him back to it with the least, else the
	/// largest; in combat a cure for the poison or blindness he carries; Reraise while none stands (once Reraise was seen to go
	/// without a fall, only against a boss); out of combat Sandstill while none stands; against a boss the Beast Potion Kit
	/// while no Auto-potion stands; protection against an announced hit on him, and on his familiar when it holds no more HP
	/// than <paramref name="familiarLargestHit"/> (in the final fight against any); a serum, else the Beast Remedy Kit, against
	/// the fight's dangerous statuses; the absorber of the element of a hit on him; the cure of his familiar's ailment; against
	/// a boss more maximum HP. With <paramref name="damage"/>: in the final fight every buff, then a damage item on his
	/// target - in the final fight any,
	/// else one a fight on a pack. Called every frame from the update.
	/// </summary>
	internal static unsafe void UseCrucibleItems(float line, uint familiarLargestHit, bool safety, bool damage)
	{
		var player = Player;
		if (!DataCenter.IsInCrucible || player == null || player.MaxHp == 0)
		{
			_crucibleSlotsRefused.Clear();
			return;
		}

		FollowReraise(player);
		if (player.IsDead)
		{
			return;
		}

		var director = EventFramework.Instance() is var framework && framework != null
			? framework->GetInstanceContentDirector()
			: null;
		if (director == null || director->InstanceContentType != InstanceContentType.CrucibleOfTheUnbroken)
		{
			return;
		}

		var crucible = (InstanceContentCrucibleOfTheUnbroken*)director;
		if (!InCombat)
		{
			_crucibleSlotsRefused.Clear();
			_serumCovers.Clear();
			_damageItemsInFight = 0;
		}

		FollowRemedy(player);

		// The last use is pending until its slot shows another item, at most one GCD; a slot the game did not take is not
		// tried again in this fight.
		if (_crucibleItemUsed.Slot >= 0)
		{
			if (crucible->Inventory[_crucibleItemUsed.Slot].ItemId != _crucibleItemUsed.Item)
			{
				_crucibleItemUsed = (-1, 0, DateTime.MinValue, 0);
			}
			else if ((DateTime.Now - _crucibleItemUsed.At).TotalSeconds < DataCenter.DefaultGCDTotal)
			{
				return;
			}
			else
			{
				_ = _crucibleSlotsRefused.Add(_crucibleItemUsed.Slot);
				var refused = CrucibleItems[_crucibleItemUsed.Item];
				if (refused.Kind == CrucibleItemKind.Damage)
				{
					_damageItemsInFight--;
				}
				else if (refused.Kind == CrucibleItemKind.ResistAilment)
				{
					_ = _serumCovers.Remove(_crucibleItemUsed.Covers);
				}

				DefenseTrace.Line($"crucible item: slot {_crucibleItemUsed.Slot} ({refused.Name}) not taken "
					+ "by the game - not tried again in this fight");
				_crucibleItemUsed = (-1, 0, DateTime.MinValue, 0);
			}
		}

		var actions = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
		if (actions == null || actions->AnimationLock > 0f || player.IsCasting)
		{
			return;
		}

		var hp = player.GetHealthRatio();
		var bossFighting = BossFighting();
		var finalFight = InCombat && CrucibleBoard.IsFinalFight;
		var slot = -1;
		var why = string.Empty;
		IBattleChara on = player;
		uint covers = 0;
		if (!safety)
		{
			goto Damage;
		}

		if (InCombat && hp < line)
		{
			slot = BestHeal(crucible, hp, line);
			why = $"at {hp:P0} HP (line {line:P0})";
		}

		if (slot < 0 && InCombat && PlayerHasStatusNamed(player, "Poison"))
		{
			slot = FirstOf(crucible, CrucibleItemKind.CurePoison);
			why = "against poison";
		}

		if (slot < 0 && InCombat && PlayerHasStatusNamed(player, "Blind"))
		{
			slot = FirstOf(crucible, CrucibleItemKind.CureBlindness);
			why = "against blindness";
		}

		if (slot < 0 && !_reraiseStood && (!_reraiseLostWithoutFall || bossFighting))
		{
			slot = MostOf(crucible, CrucibleItemKind.Reraise);
			why = _reraiseLostWithoutFall ? "against a boss, no Reraise stands" : "no Reraise stands";
		}

		if (slot < 0 && !InCombat && !PlayerHasStatusNamed(player, "Sandstill"))
		{
			slot = FirstOf(crucible, CrucibleItemKind.Sandstill);
			why = "before the next fight, no Sandstill stands";
		}

		if (slot < 0 && InCombat && bossFighting && !PlayerHasStatusNamed(player, "Auto-potion"))
		{
			slot = FirstOf(crucible, CrucibleItemKind.AutoPotion);
			why = "against a boss, no Auto-potion stands";
		}

		// 6: protection against an announced hit on him; in the final fight also while an enemy attacks him.
		if (slot < 0 && InCombat && (HitAnnouncedOn(player) || (finalFight && EnemyAttacks(player))))
		{
			slot = Protection(crucible, player, out why);
		}

		// 7, first (A577): heal the familiar when it would fall to a hit as large as its largest in this fight (K9's measure),
		// before the first measured hit below his potion line as a share of its HP - with a Beast Potion ("to self or
		// familiar"), never the last healing item: that one stays for him, his HP are the run.
		var familiar = Familiar;
		if (slot < 0 && InCombat && familiar != null && !familiar.IsDead && familiar.CurrentHp > 0 && familiar.MaxHp > 0
			&& (familiarLargestHit > 0 ? familiar.CurrentHp <= familiarLargestHit : familiar.GetHealthRatio() < line)
			&& HealItemsHeld(crucible) > 1)
		{
			var mark = familiarLargestHit > 0 ? Math.Min(1f, familiarLargestHit / (float)familiar.MaxHp) : line;
			slot = BestHeal(crucible, familiar.GetHealthRatio(), mark, true);
			if (slot >= 0)
			{
				on = familiar;
				why = $"for {familiar.Name} at {familiar.CurrentHp} HP, "
					+ (familiarLargestHit > 0 ? $"its largest hit this fight {familiarLargestHit}" : $"below {line:P0}");
			}
		}

		// 7: the same protection for the familiar when it would fall to a hit as large as its largest in this fight (K9's
		// measure); in the final fight against any announced hit.
		if (slot < 0 && InCombat && familiar != null && !familiar.IsDead && familiar.CurrentHp > 0
			&& DataCenter.IsTankbusterAimedAt(familiar)
			&& (finalFight || (familiarLargestHit > 0 && familiar.CurrentHp <= familiarLargestHit)))
		{
			slot = Protection(crucible, familiar, out why);
			if (slot >= 0)
			{
				on = familiar;
				why = $"for {familiar.Name} at {familiar.CurrentHp} HP {why}";
			}
		}

		// 8: a serum, else the Beast Remedy Kit, against the fight's dangerous statuses.
		if (slot < 0 && InCombat && CrucibleBoard.Ailments.Count > 0)
		{
			slot = AgainstAilments(crucible, player, out covers, out why);
		}

		// 9: the absorber of the element a cast carries that is announced on him or whose area reaches him.
		if (slot < 0 && InCombat && (HitAnnouncedOn(player) || DataCenter.AreaHitReachesPlayer))
		{
			slot = AgainstElement(crucible, player, out why);
		}

		// 10: the cure for the ailment the familiar carries, on it.
		if (slot < 0 && familiar != null && !familiar.IsDead && familiar.CurrentHp > 0)
		{
			slot = FamiliarCure(crucible, familiar, out why);
			if (slot >= 0)
			{
				on = familiar;
			}
		}

		// 11: against a boss, more maximum HP while none stands.
		if (slot < 0 && InCombat && bossFighting)
		{
			for (var i = 0; i < crucible->Inventory.Length && slot < 0; i++)
			{
				if (TryItem(crucible, i, CrucibleItemKind.MaxHp, out var raise) && !PlayerHasStatusNamed(player, raise.Status))
				{
					slot = i;
					why = $"against a boss, no {raise.Status} stands";
				}
			}
		}

	Damage:
		// 12: in the final fight every buff while its status does not stand, before the damage items it may raise.
		if (slot < 0 && damage && finalFight)
		{
			for (var i = 0; i < crucible->Inventory.Length && slot < 0; i++)
			{
				if (TryItem(crucible, i, CrucibleItemKind.Buff, out var buff) && !PlayerHasStatusNamed(player, buff.Status))
				{
					slot = i;
					on = player;
					why = $"in the final fight, {buff.Status}";
				}
			}
		}

		if (slot < 0 && damage && InCombat && (finalFight || _damageItemsInFight < 1)
			&& DamageItem(crucible, finalFight, out var damageSlot, out var target, out var damageWhy))
		{
			slot = damageSlot;
			on = target;
			why = damageWhy;
		}

		var targets = TargetSystem.Instance();
		if (slot < 0 || targets == null)
		{
			return;
		}

		var used = crucible->Inventory[slot].ItemId;
		var previous = targets->SoftTarget;
		try
		{
			targets->SoftTarget = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)on.Address;
			crucible->UseItem((uint)slot, 0);
		}
		finally
		{
			targets->SoftTarget = previous;
		}

		var item = CrucibleItems[used];
		if (item.Kind == CrucibleItemKind.Damage)
		{
			_damageItemsInFight++;
		}
		else if (item.Kind == CrucibleItemKind.ResistAilment)
		{
			_ = _serumCovers.Add(covers);
		}

		_crucibleItemUsed = (slot, used, DateTime.Now, covers);
		DefenseTrace.Line($"crucible item: {item.Name} {why}");
	}

	// A known tankbuster aimed at him (list, learned table, marker, BossModReborn naming him), or a measured aimed cast at him
	// that calls for a defence (A540).
	private static bool HitAnnouncedOn(IBattleChara player)
	{
		return DataCenter.IsTankbusterAimedAt(player) || TankbusterHelp.AimedHitCallsForDefence(player, out _);
	}

	private static bool EnemyAttacks(IBattleChara chara)
	{
		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile != null && !hostile.IsDead && hostile.InCombat() && hostile.TargetObjectId == chara.GameObjectId)
			{
				return true;
			}
		}

		return false;
	}

	// The attack type (Action.AttackType row) of a cast aimed at the character, 0 when none is seen.
	private static uint AttackTypeAt(IBattleChara chara)
	{
		var casters = DataCenter.AreaCasters;
		for (var i = 0; i < casters.Count; i++)
		{
			var caster = casters[i];
			if (caster != null && caster.IsCasting && caster.CastTargetObjectId == chara.GameObjectId
				&& Service.GetSheet<Lumina.Excel.Sheets.Action>()?.GetRowOrDefault(caster.CastActionId) is { } action)
			{
				return action.AttackType.RowId;
			}
		}

		return 0;
	}

	// 6 and 7: none while one of the protections stands on the character; against a physical hit the item that nullifies
	// physical attacks, against a magical one the one that reflects magic, else - or when that one is missing - the one that
	// mitigates every kind.
	private static unsafe int Protection(InstanceContentCrucibleOfTheUnbroken* crucible, IBattleChara chara, out string why)
	{
		why = string.Empty;
		foreach (var (_, item) in CrucibleItems)
		{
			if (item.Kind is CrucibleItemKind.Mitigate or CrucibleItemKind.NullifyPhysical or CrucibleItemKind.ReflectMagic
				&& item.Status.Length > 0 && PlayerHasStatusNamed(chara, item.Status))
			{
				return -1;
			}
		}

		var attackType = AttackTypeAt(chara);
		var slot = AttackTypeKind.IsPhysical(attackType)
			? FirstOf(crucible, CrucibleItemKind.NullifyPhysical)
			: AttackTypeKind.IsMagic(attackType)
				? FirstOf(crucible, CrucibleItemKind.ReflectMagic)
				: -1;
		if (slot < 0)
		{
			slot = MostOf(crucible, CrucibleItemKind.Mitigate);
		}

		why = $"against an announced hit (attack type {attackType})";
		return slot;
	}

	// 8: the serum that names a dangerous status of the fight not yet covered in it, else the Beast Remedy Kit while no
	// Auto-remedy stands and a dangerous status is left that it was not seen to leave uncured.
	private static unsafe int AgainstAilments(InstanceContentCrucibleOfTheUnbroken* crucible, IBattleChara player, out uint covers,
		out string why)
	{
		covers = 0;
		why = string.Empty;
		var uncovered = false;
		foreach (var ailment in CrucibleBoard.Ailments)
		{
			if (_serumCovers.Contains(ailment))
			{
				continue;
			}

			var name = StatusName(ailment);
			for (var slot = 0; slot < crucible->Inventory.Length; slot++)
			{
				if (TryItem(crucible, slot, CrucibleItemKind.ResistAilment, out var serum) && serum.Status.Length > 0
					&& name.Contains(serum.Status, StringComparison.OrdinalIgnoreCase))
				{
					covers = ailment;
					why = $"against {name} in this fight";
					return slot;
				}
			}

			uncovered |= !_notCuredByRemedy.Contains(ailment);
		}

		if (!uncovered || PlayerHasStatusNamed(player, "Auto-remedy"))
		{
			return -1;
		}

		why = "against the dangerous statuses of this fight, no Auto-remedy stands";
		return FirstOf(crucible, CrucibleItemKind.AutoRemedy);
	}

	// The self-correction of 8: a dangerous status of the fight that stands on the player for longer than a GCD while
	// Auto-remedy stands is one the kit does not cure.
	private static void FollowRemedy(IBattleChara player)
	{
		if (!InCombat || !PlayerHasStatusNamed(player, "Auto-remedy"))
		{
			_ailmentUnderRemedySince.Clear();
			return;
		}

		foreach (var ailment in CrucibleBoard.Ailments)
		{
			if (!HasStatusId(player, ailment))
			{
				_ = _ailmentUnderRemedySince.Remove(ailment);
				continue;
			}

			if (!_ailmentUnderRemedySince.TryGetValue(ailment, out var since))
			{
				_ailmentUnderRemedySince[ailment] = DateTime.Now;
			}
			else if ((DateTime.Now - since).TotalSeconds > DataCenter.DefaultGCDTotal && _notCuredByRemedy.Add(ailment))
			{
				DefenseTrace.Line($"crucible item: {StatusName(ailment)} stayed under Auto-remedy - the Beast Remedy Kit is not "
					+ "used against it again");
			}
		}
	}

	// 9: the elements of the casts in progress aimed at him, and with an area hit reaching him of all casts in progress; the
	// absorber of such an element with the highest chance, while its status does not stand on him.
	private static unsafe int AgainstElement(InstanceContentCrucibleOfTheUnbroken* crucible, IBattleChara player, out string why)
	{
		why = string.Empty;
		var areaReaches = DataCenter.AreaHitReachesPlayer;
		var elements = new HashSet<uint>();
		var casters = DataCenter.AreaCasters;
		for (var i = 0; i < casters.Count; i++)
		{
			var caster = casters[i];
			if (caster != null && caster.IsCasting && (areaReaches || caster.CastTargetObjectId == player.GameObjectId))
			{
				elements.UnionWith(CrucibleBoard.ElementsOf(caster.CastActionId));
			}
		}

		var best = -1;
		var chance = 0f;
		for (var slot = 0; slot < crucible->Inventory.Length; slot++)
		{
			if (TryItem(crucible, slot, CrucibleItemKind.AbsorbElement, out var item) && elements.Contains(item.Element)
				&& !PlayerHasStatusNamed(player, item.Status) && item.Value > chance)
			{
				best = slot;
				chance = item.Value;
				why = $"against a {CrucibleBoard.ElementName(item.Element)} hit on you";
			}
		}

		return best;
	}

	// 10: the item that cures the ailment the familiar carries, by the English name of its status.
	private static unsafe int FamiliarCure(InstanceContentCrucibleOfTheUnbroken* crucible, IBattleChara familiar, out string why)
	{
		why = string.Empty;
		for (var slot = 0; slot < crucible->Inventory.Length; slot++)
		{
			if (TryItem(crucible, slot, CrucibleItemKind.CureFamiliar, out var item) && item.Status.Length > 0
				&& PlayerHasStatusNamed(familiar, item.Status))
			{
				why = $"for {familiar.Name}, against {item.Status}";
				return slot;
			}
		}

		return -1;
	}

	// 13: the damage item of most worth on the player's target - its potency times the enemies within its radius of the target,
	// each counting one plus the measured weakness bonus where the item's element is its weakness; at equal worth the one
	// without knockback. Outside the final fight only when it hits more than the target. An item whose element is the weakness
	// only of enemies still to come waits for them (K2).
	private static unsafe bool DamageItem(InstanceContentCrucibleOfTheUnbroken* crucible, bool finalFight, out int slot,
		out IBattleChara target, out string why)
	{
		slot = -1;
		why = string.Empty;
		target = HostileTarget!;
		if (target == null || target.IsDead || !target.InCombat())
		{
			return false;
		}

		var bonus = CrucibleBoard.WeaknessBonus;
		var bestWorth = 0f;
		var bestHits = 0;
		var bestKnockback = false;
		for (var i = 0; i < crucible->Inventory.Length; i++)
		{
			if (!TryItem(crucible, i, CrucibleItemKind.Damage, out var item) || OnlyForEnemiesToCome(item.Element))
			{
				continue;
			}

			var hits = 0;
			var weight = 0f;
			foreach (var hostile in DataCenter.AllHostileTargets)
			{
				if (hostile == null || hostile.IsDead || !hostile.InCombat()
					|| Vector3.Distance(hostile.Position, target.Position) > item.Radius + hostile.HitboxRadius)
				{
					continue;
				}

				hits++;
				weight += 1f + (item.Element != 0 && CrucibleBoard.WeaknessOf(hostile.NameId) == item.Element ? bonus : 0f);
			}

			var worth = item.Value * weight;
			if ((finalFight || hits > 1) && (worth > bestWorth || (worth == bestWorth && bestKnockback && !item.Knockback)))
			{
				slot = i;
				bestWorth = worth;
				bestHits = hits;
				bestKnockback = item.Knockback;
			}
		}

		if (slot < 0)
		{
			return false;
		}

		why = $"on {target.Name}, {bestHits} enemies within reach{(finalFight ? ", final fight" : string.Empty)}";
		return true;
	}

	// K2 for the damage items: the element is the weakness of an enemy still to come and of none here.
	private static bool OnlyForEnemiesToCome(uint element)
	{
		if (element == 0 || CrucibleBoard.Fight == 0)
		{
			return false;
		}

		var coming = false;
		foreach (var enemy in CrucibleBoard.Enemies)
		{
			if (enemy.Weakness != element)
			{
				continue;
			}

			if (CrucibleBoard.IsHere(enemy.NameId))
			{
				return false;
			}

			coming |= CrucibleBoard.IsToCome(enemy.NameId);
		}

		return coming;
	}

	// The healing item that lifts the player to the line with the least, a potion before ash at the same heal; while none
	// does, the largest.
	private static unsafe int BestHeal(InstanceContentCrucibleOfTheUnbroken* crucible, float hp, float line, bool familiar = false)
	{
		var best = -1;
		CrucibleItem bestItem = default;
		for (var slot = 0; slot < crucible->Inventory.Length; slot++)
		{
			// For the familiar only what names it: Crucible Ash heals you and the allies near you, and the familiar stands at
			// the enemy (A577).
			if (!TryItem(crucible, slot, CrucibleItemKind.Heal, out var item) || (familiar && item.Allies))
			{
				continue;
			}

			var lifts = hp + item.Value >= line;
			var bestLifts = best >= 0 && hp + bestItem.Value >= line;
			if (best < 0
				|| (lifts && !bestLifts)
				|| (lifts && bestLifts && (item.Value < bestItem.Value || (item.Value == bestItem.Value && bestItem.Allies && !item.Allies)))
				|| (!lifts && !bestLifts && item.Value > bestItem.Value))
			{
				best = slot;
				bestItem = item;
			}
		}

		return best;
	}

	// A577: the healing items held, any of them - the last one stays for the player.
	private static unsafe int HealItemsHeld(InstanceContentCrucibleOfTheUnbroken* crucible)
	{
		var count = 0;
		for (var slot = 0; slot < crucible->Inventory.Length; slot++)
		{
			if (TryItem(crucible, slot, CrucibleItemKind.Heal, out _))
			{
				count++;
			}
		}

		return count;
	}

	private static unsafe int FirstOf(InstanceContentCrucibleOfTheUnbroken* crucible, CrucibleItemKind kind)
	{
		for (var slot = 0; slot < crucible->Inventory.Length; slot++)
		{
			if (TryItem(crucible, slot, kind, out _))
			{
				return slot;
			}
		}

		return -1;
	}

	private static unsafe int MostOf(InstanceContentCrucibleOfTheUnbroken* crucible, CrucibleItemKind kind)
	{
		var best = -1;
		var value = 0f;
		for (var slot = 0; slot < crucible->Inventory.Length; slot++)
		{
			if (TryItem(crucible, slot, kind, out var item) && (best < 0 || item.Value > value))
			{
				best = slot;
				value = item.Value;
			}
		}

		return best;
	}

	private static unsafe bool TryItem(InstanceContentCrucibleOfTheUnbroken* crucible, int slot, CrucibleItemKind kind, out CrucibleItem item)
	{
		item = default;
		var id = crucible->Inventory[slot].ItemId;
		return id != 0 && !_crucibleSlotsRefused.Contains(slot) && CrucibleItems.TryGetValue(id, out item) && item.Kind == kind;
	}

	private static bool BossFighting()
	{
		foreach (var hostile in DataCenter.AllHostileTargets)
		{
			if (hostile != null && !hostile.IsDead && hostile.InCombat() && hostile.IsBossFromIcon())
			{
				return true;
			}
		}

		return false;
	}

	// Reraise is assumed to stand until a fall, since its text names no duration. Seen to go while the player lives, it is
	// kept for the fights against a boss from then on.
	private static void FollowReraise(IBattleChara player)
	{
		var stands = PlayerHasStatusNamed(player, "Reraise");
		_fellUnderReraise |= _reraiseStood && player.IsDead;
		if (_reraiseStood && !stands && !player.IsDead)
		{
			if (!_fellUnderReraise && !_reraiseLostWithoutFall)
			{
				_reraiseLostWithoutFall = true;
				DefenseTrace.Line("crucible item: Reraise went without a fall - from now on a Reraiser only against a boss");
			}

			_fellUnderReraise = false;
		}

		_reraiseStood = stands || (_reraiseStood && player.IsDead);
	}
}
