using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Config;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using RotationSolver.Basic.Configuration;
using RotationSolver.Basic.Rotations.Duties;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using Action = Lumina.Excel.Sheets.Action;
using Buddy = FFXIVClientStructs.FFXIV.Client.Game.UI.Buddy;
using CharacterManager = FFXIVClientStructs.FFXIV.Client.Game.Character.CharacterManager;
using CombatRole = RotationSolver.Basic.Data.CombatRole;

namespace RotationSolver.Basic;

internal static partial class DataCenter
{
	// A779: the length of the last combat and the territory it was fought in, so a rotation can tell whether the pull
	// before this one here reached a later burst (set by ActionUpdater when combat ends).
	internal static float LastPullSeconds { get; set; }

	internal static uint LastPullTerritoryId { get; set; }

	/// <summary>
	/// The party's first living tank, or null if there is none.
	/// </summary>
	public static IBattleChara? PartyTank
	{
		get
		{
			foreach (var member in PartyMembers)
			{
				if (member.IsJobCategory(JobRole.Tank) && !member.IsDead)
				{
					return member;
				}
			}
			return null;
		}
	}

	/// <summary>
	/// Whether <see cref="AllHostileTargets"/> and the other target lists were built in this frame: while RSR is on or an
	/// automatic start is set (<c>MajorUpdater</c>). Off, the lists keep the objects of the last frame they were built in,
	/// and those are not valid any more (concept 03, "Dritte Invariante"). A rotation service that reads them asks this
	/// first (A576).
	/// </summary>
	public static bool HostileListLive { get; internal set; }

	/// <summary>
	/// The most cast time left on any untargetable enemy-side figure, whatever it casts, in seconds - zero when none
	/// casts. Rebuilt with <see cref="AreaCasters"/>; read by the tankbuster marker watch, which waits for every enemy
	/// cast that runs while a marker stands, the helpers' included (A334).
	/// </summary>
	internal static float LongestHelperCastRemaining { get; set; }

	/// <summary>
	/// Whether an untargetable enemy casting this action is read as an enemy: the action is on the area,
	/// tankbuster or knockback list, or the tankbuster table has measured it. The list itself is the evidence of
	/// hostility - an id is there only once it hit the party, or by the shipped list (A277, A303).
	/// </summary>
	internal static bool IsListedHostileCast(uint actionId)
		=> OtherConfiguration.HostileCastingArea.Contains(actionId)
			|| OtherConfiguration.HostileCastingTank.Contains(actionId)
			|| OtherConfiguration.HostileCastingKnockback.Contains(actionId)
			|| TankbusterForecast.IsLearned(actionId);

	/// <summary>
	/// Whether <paramref name="h"/> counts as an enemy for the cast recognition: targetable and hostile, or an
	/// untargetable enemy casting a listed action (<see cref="IsListedHostileCast"/>; the area list since A277, the
	/// tankbuster and knockback lists and the measured tankbusters since A303).
	/// </summary>
	/// <param name="h">The caster.</param>
	/// <returns>True for an enemy whose area cast is read.</returns>
	internal static bool IsAreaCastEnemy(IBattleChara h) => h.IsEnemy() || AreaCastHelperIds.Contains(h.GameObjectId);

	/// <summary>
	/// Whom each enemy last struck, and when, from the effect handler. The provoke delay ends with an enemy's first hit on
	/// the party member it turned to (A500).
	/// </summary>
	internal static readonly ConcurrentDictionary<ulong, (ulong Target, DateTime At)> LastStrikeByEnemy = new();

	/// <summary>
	/// The dead another player is casting a raise on right now, by object id: the caster whose raise lands
	/// first and when it lands. Rebuilt each target update (<c>TargetUpdater.UpdateCastsByOthers</c>, A293).
	/// <see cref="TargetFilter.GetDeath"/> leaves them out, and the player's own raise on such a body is broken
	/// off when the other lands first (<c>CancelCastUpdater</c>, A294).
	/// </summary>
	internal static Dictionary<ulong, (string Caster, DateTime LandsAt)> RaisedByOthers { get; set; } = [];

	/// <summary>
	/// Yalms per second by which the distance between the player and <paramref name="target"/> shrinks, from this read and
	/// the last one; 0 on a new target or after more than a GCD without a read, negative while it grows (concept 03, "C4",
	/// A605).
	/// </summary>
	internal static float ClosingSpeed(IBattleChara target)
	{
		var now = DateTime.Now;
		var distance = target.DistanceToPlayer();
		var since = (float)(now - _closingLastRead).TotalSeconds;
		if (target.GameObjectId != _closingTargetId || since > DefaultGCDTotal)
		{
			_closingTargetId = target.GameObjectId;
			_closingSpeed = 0f;
		}
		else if (since > 0f)
		{
			_closingSpeed = (_closingLastDistance - distance) / since;
		}

		_closingLastDistance = distance;
		_closingLastRead = now;
		return _closingSpeed;
	}

	/// <summary>
	/// Whether an enemy with a boss icon (<see cref="ObjectHelper.IsBossFromIcon"/>) is alive and fighting. One reading for
	/// the Crucible's items and the deep dungeon's pomanders (A625).
	/// </summary>
	internal static bool BossInCombat
	{
		get
		{
			foreach (var hostile in AllHostileTargets)
			{
				if (hostile != null && !hostile.IsDead && hostile.InCombat() && hostile.IsBossFromIcon())
				{
					return true;
				}
			}

			return false;
		}
	}

	private static bool _packTTKKnown;

	/// <summary>
	/// No tank and no healer of the party is alive beside the player: nobody holds the enemies off him and nobody heals
	/// him but himself. A blue mage beside him counts in the role of his Aetheric Mimicry (concept 03, "Zielart ohne Tank
	/// und Heiler", A525, A526).
	/// </summary>
	public static bool NoTankNoHealerBeside => !AnyLiving(JobRole.Tank, true) && !AnyLiving(JobRole.Healer, true);

	/// <summary>
	/// Concept 03, "Zielart ohne Tank und Heiler" (A525, A526, A541): the job the precept names and none beside him who
	/// holds or heals. The enemy to attack first is then the one <see cref="EnemyDanger.Compare"/> puts first - danger per
	/// second against its HP, the fewest HP while nothing is measured. A line whenever that turns.
	/// </summary>
	internal static bool DangerFirstAlone
	{
		get
		{
			var holds = (CurrentRotation is BeastmasterRotation or BlueMageRotation) && NoTankNoHealerBeside;
			if (holds != _dangerFirstAlone)
			{
				_dangerFirstAlone = holds;
				DefenseTrace.Line(holds
					? "targeting: the most dangerous per HP first - no tank and no healer alive beside you"
					: "targeting: your setting again");
			}

			return holds;
		}
	}

	// A blue mage counts in the role of his Aetheric Mimicry through IsJobCategory (A526, A559).
	private static bool AnyLiving(JobRole role, bool excludeSelf)
	{
		var playerId = Player.Object?.GameObjectId ?? 0;
		foreach (var member in PartyMembers)
		{
			if (member != null && !member.IsDead && !(excludeSelf && member.GameObjectId == playerId) && member.IsJobCategory(role))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// No healer is alive and an enemy attacks the player, with "Without a living healer, spend your defence to stay
	/// alive longer" on: the single-target defence stands open for it (<c>StateUpdater.ShouldAddDefenseSingle</c>,
	/// A361). Unless a tankbuster opens it as well (<see cref="TankbusterOpensDefence"/>), a mitigation on the player
	/// goes only once it is back before the next tankbuster on him (<see cref="TankbusterForecast.NextOnPlayerIn"/>).
	/// </summary>
	public static bool UnhealedUnderAttack { get; set; }

	/// <summary>
	/// A tankbuster, or the player, opens the single-target defence this frame: his command, a listed tankbuster cast
	/// at him, BossModReborn's imminent one, or for a tank a known tankbuster aimed at either tank. What the defence
	/// spends then is for that hit; the pull rule and an attack on an unhealed player are the other sources.
	/// </summary>
	/// <remarks>
	/// Known, not the unlisted reading "an enemy casting at the one it attacks" that also opens a tank's defence: that
	/// matches every uninterruptible trash cast in a pull, and here it lifted the pull's hold on the big mitigation and
	/// the hold for the next tankbuster without a healer, so the old 60 s / 30 s stagger came back with the first such
	/// cast (A395).
	/// </remarks>
	internal static bool TankbusterOpensDefence =>
		CommandStatus.HasFlag(AutoStatus.DefenseSingle) || IsHostileCastingTankBusterAtMe || BMRTankbusterImminent
		|| (Role == JobRole.Tank && _knownTankbusterAtATank.Value);

	/// <summary>
	/// Is someone alive who could raise, in the set the current raise settings draw targets from?
	///
	/// Two callers ask this with opposite intent, which is why the player is a parameter rather
	/// than a fixed rule. A Phoenix Down asks "can nobody do this properly" and must count the
	/// player, because a living raiser uses their spell instead of an item. The only-healer hard
	/// cast modes ask "is there anyone else", and there the player is the one deciding.
	///
	/// The reference set follows <see cref="Configs.RaiseType"/>, because that is what decides
	/// where a corpse may come from. Under the alliance modes the other alliances are real parties
	/// with their own raisers: while one of them is alive the raise is theirs to take. AllOutOfDuty
	/// raises the alliance too and counts it the same way. Strangers in the open world stay out - they
	/// are not a raise reserve, and counting them would suppress raising almost always; since upstream
	/// 7.5.7.5 they are kept apart from <see cref="AllianceMembers"/>, which before held them under
	/// that mode and was left out for it (A332).
	/// </summary>
	/// <param name="excludeSelf">Skip the player, for callers asking whether anyone *else* can.</param>
	public static bool AnyLivingRaiser(bool excludeSelf)
	{
		return AnyLivingRaiser(excludeSelf, healersOnly: false);
	}

	/// <inheritdoc cref="AnyLivingRaiser(bool)"/>
	/// <param name="excludeSelf">Skip the player, for callers asking whether anyone *else* can.</param>
	/// <param name="healersOnly">
	/// Count only the healer jobs, leaving Summoner and Red Mage out.
	///
	/// The two "only healer" hard cast modes ask for this, because their option text does: "Raise
	/// while Swiftcast is on cooldown and other <i>healers</i> are dead". The wider question - can
	/// anybody raise - would hold the hard cast back on the strength of a living Summoner, that is
	/// on an assumption about what another player is going to do. The user's instruction is the
	/// text: those modes wait for healers and for nobody else.
	///
	/// The feather keeps the wide set, because his instruction there is the opposite one and says
	/// raiser: a Phoenix Down goes out when nobody left can raise at all.
	///
	/// The level check applies either way - a healer below <see cref="RaiseLevel"/> has no raise to
	/// wait for.
	/// </param>
	public static bool AnyLivingRaiser(bool excludeSelf, bool healersOnly)
	{
		if (HasLivingRaiser(PartyMembers, excludeSelf, healersOnly))
		{
			return true;
		}

		return Service.Config.RaiseType is RaiseType.PartyAndAllianceSupports
				or RaiseType.PartyAndAllianceHealers
				or RaiseType.All
				or RaiseType.AllOutOfDuty
			&& HasLivingRaiser(AllianceMembers, excludeSelf, healersOnly);
	}

	private static bool HasLivingRaiser(IEnumerable<IBattleChara>? members, bool excludeSelf, bool healersOnly)
	{
		if (members == null)
		{
			return false;
		}

		foreach (var member in members)
		{
			if (member == null || member.IsDead)
			{
				continue;
			}

			if (excludeSelf && member.IsPlayer())
			{
				continue;
			}

			if (member.IsJobCategory(JobRole.Healer)
				|| (!healersOnly && member.IsJobs(ECommons.ExcelServices.Job.SMN)))
			{
				if (member.Level >= RaiseLevel)
				{
					return true;
				}

				continue;
			}

			if (!healersOnly && member.IsJobs(ECommons.ExcelServices.Job.RDM) && member.Level >= VerraiseLevel)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// The level at which a healer or a Summoner has their raise, and the one at which a Red Mage has
	/// Verraise.
	///
	/// <see cref="CanRaise"/> has always applied these to the player. <see cref="HasLivingRaiser"/> did
	/// not apply them to anyone else, so a Red Mage below 64 - every level-synced run through the older
	/// content - counted as a living raiser and held back the feather that was the only way anyone was
	/// getting up. Sharing the two numbers is what keeps the question from being answered differently
	/// for the player than for the party.
	///
	/// For another party member the level comes from ICharacter.Level, which is what the client shows
	/// for them. That it reports the synced level inside a synced duty is what WrathCombo assumes too
	/// (its InFATE counts the player in a FATE only while LocalPlayer.Level is at most the FATE's
	/// maximum) - another tool, not a game source. Were it the true level, an under-levelled raiser would
	/// still count as living, which is the state before this check and no worse.
	/// </summary>
	private static byte RaiseLevel => _raiseLevel ??= LevelOf(ActionID.RaisePvE);

	/// <inheritdoc cref="RaiseLevel"/>
	private static byte VerraiseLevel => _verraiseLevel ??= LevelOf(ActionID.VerraisePvE);

	private static byte? _raiseLevel;

	private static byte? _verraiseLevel;

	// The level an action is learned at, from the action sheet (Raise 12, Verraise 64 today). The healers' raises all
	// share Raise's level; a sheet that cannot be read answers 1, so the level check then lets every member count, the
	// state before this check (A340).
	private static byte LevelOf(ActionID action)
		=> Service.GetSheet<Lumina.Excel.Sheets.Action>().TryGetRow((uint)action, out var row) && row.ClassJobLevel > 0
			? row.ClassJobLevel
			: (byte)1;

	private static float _lastGcdTotal;

	/// <summary>How long a party-wide reading counts as the same frame's, in milliseconds.</summary>
	internal static long FrameCacheMs => PartyHpStatsTtlMs;

	/// <summary>
	/// The smallest party composition in the game: one tank, one healer, one melee, one ranged
	/// (ContentMemberType row 2). The area-heal statistics weigh the lowest this many members of a
	/// larger party, so a full party is judged on the four who need it most.
	/// </summary>
	internal const int LightPartySize = 4;

	/// <summary>
	/// The lowest forecast health in the party, from the same members and the same moment as
	/// <see cref="ComputeForecastAreaStats"/>: what the downtime heal's threshold reads (A358).
	/// </summary>
	internal static float ForecastMinHp(bool instant)
	{
		ComputeForecastAreaStats(instant, out _, out _, out _, out _);
		return _forecastMinCache[instant ? 1 : 0];
	}

	/// <summary>
	/// The area-heal statistics on forecast health: each member's health carried forward to the moment
	/// a heal begun now would land (<see cref="ObjectHelper.GetForecastSurvivingShare(IBattleChara, bool)"/>).
	/// </summary>
	/// <remarks>
	/// "Heal ahead of incoming damage" states that every healing threshold reads the health a member is
	/// heading for; the area thresholds read the level only. Kept apart from the statistics above
	/// because those have many readers outside the heal chain, foreign rotations among them, tuned to
	/// the level - only the two area thresholds read this. With the setting off the forecast share is
	/// 1 and the figures equal the level ones.
	/// </remarks>
	internal static void ComputeForecastAreaStats(bool instant, out float avgHp, out float stdDevHp, out float lowestAvgHp, out float lowestStdDevHp)
	{
		var kind = instant ? 1 : 0;
		var now = Environment.TickCount64;
		if (_forecastStatsCacheTick[kind] != long.MinValue && now - _forecastStatsCacheTick[kind] < PartyHpStatsTtlMs)
		{
			avgHp = _forecastAvgCache[kind];
			stdDevHp = _forecastStdDevCache[kind];
			lowestAvgHp = _forecastLowestAvgCache[kind];
			lowestStdDevHp = _forecastLowestStdDevCache[kind];
			return;
		}

		// The same members as the level statistics count - valid, with an id, alive - so that with the
		// setting off both give the same figures, as the setting's text promises.
		var count = 0;
		foreach (var member in PartyMembers)
		{
			if (!member.IsValid() || member.GameObjectId == 0 || count >= _forecastHpBuffer.Length)
			{
				continue;
			}

			try
			{
				var hp = GetPartyMemberHPRatio(member);
				if (hp > 0)
				{
					_forecastHpBuffer[count++] = HealReduction.Restorable(member, hp * member.GetForecastSurvivingShare(instant));
				}
			}
			catch (AccessViolationException ex)
			{
				PluginLog.Error($"AccessViolationException in forecast party HP computation: {ex.Message}");
			}
		}

		avgHp = stdDevHp = lowestAvgHp = lowestStdDevHp = 0;
		_forecastMinCache[kind] = 0;
		if (count > 0)
		{
			Array.Sort(_forecastHpBuffer, 0, count);
			_forecastMinCache[kind] = _forecastHpBuffer[0];
			var lowestCount = Math.Min(count, LightPartySize);

			float sum = 0, lowestSum = 0;
			for (var i = 0; i < count; i++)
			{
				sum += _forecastHpBuffer[i];
				if (i < lowestCount)
				{
					lowestSum += _forecastHpBuffer[i];
				}
			}

			avgHp = sum / count;
			lowestAvgHp = lowestSum / lowestCount;

			float variance = 0, lowestVariance = 0;
			for (var i = 0; i < count; i++)
			{
				var diff = _forecastHpBuffer[i] - avgHp;
				variance += diff * diff;
				if (i < lowestCount)
				{
					var lowestDiff = _forecastHpBuffer[i] - lowestAvgHp;
					lowestVariance += lowestDiff * lowestDiff;
				}
			}

			stdDevHp = (float)Math.Sqrt(variance / count);
			lowestStdDevHp = (float)Math.Sqrt(lowestVariance / lowestCount);
		}

		_forecastAvgCache[kind] = avgHp;
		_forecastStdDevCache[kind] = stdDevHp;
		_forecastLowestAvgCache[kind] = lowestAvgHp;
		_forecastLowestStdDevCache[kind] = lowestStdDevHp;
		_forecastStatsCacheTick[kind] = now;
	}

	/// <summary>
	/// Whether an enemy cast certainly reaches the player and a step avoids it: not aimed at him, of a shape measured exactly
	/// here (a circle or a line, and a cone or a ring whose omen names its angle or inner circle - without, they count as a
	/// full circle in <see cref="AreaCastReaches"/>; A731), and leaving a living party member other than him outside -
	/// unlike a raidwide, which reaches everyone. False without such a member to compare (concept 03, "Bewegungssperre beim
	/// Wirken und die KI von BossMod", A652).
	/// </summary>
	internal static bool CastCertainlyDodgeable(IBattleChara h)
	{
		var player = Player.Object;
		if (player == null || h == null || !h.IsValid() || !h.IsCasting || h.CastTargetObjectId == player.GameObjectId
			|| !TryGetCastAction(h, out var act))
		{
			return false;
		}

		// A731: a cone with its angle and a ring with its inner circle in the omen are measured too, and the circle of type 5.
		var (degrees, inner) = OmenShape(act);
		var measured = (CastType)act.CastType is CastType.Circle or CastType.StraightLine or CastType.Unknown5
			|| act.CastType == LineFromCasterCastType
			|| (IsConeCastType(act.CastType) && degrees > 0f)
			|| ((CastType)act.CastType == CastType.Donut && (degrees > 0f || inner > 0f));
		if (!measured || !AreaCastReaches(h, act, player))
		{
			return false;
		}

		foreach (var member in PartyMembers)
		{
			if (member != null && member.GameObjectId != player.GameObjectId && !member.IsDead && !AreaCastReaches(h, act, member))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Whether a listed area cast reaches <paramref name="member"/>, from the shape the game data gives it.
	/// </summary>
	/// <remarks>
	/// <para>A single-target action with a cast range hits the one it is cast at and nobody else (A192);
	/// a self-targeted action with no effect range is the scripted party-wide kind and reaches everyone.
	/// A circle cast at someone is centred on them; a line reaches what lies in its width; a circle
	/// around the caster is measured from the caster's centre. BossModReborn sizes none of its circle
	/// shapes by the caster's hitbox; measured from the hitbox edge, a large boss stretched every
	/// point-blank cast by its hitbox - ten yalms reached eighteen (A218). The member's own hitbox counts
	/// in favour of the hit everywhere.</para>
	/// <para>Ground-placed circles keep the old measure: their centre is only in the native cast data.
	/// A cone takes the angle its omen names and a ring its inner circle (<see cref="OmenShape"/>); without, both count as a
	/// circle. Types 3, 4 and 5 reach past the caster's hitbox (A731).</para>
	/// </remarks>
	private static bool AreaCastReaches(IBattleChara h, Action act, IBattleChara member)
	{
		if (AreaShapeReaches(h, act, member))
		{
			return true;
		}

		// A765: the same attack's hits without a cast of their own, in the rows next to it (concept 13, "Mehrteilige Angriffe").
		foreach (var part in SameAttackHits(act))
		{
			if (AreaShapeReaches(h, part, member))
			{
				return true;
			}
		}

		return false;
	}

	private static readonly ConcurrentDictionary<uint, Action[]> _sameAttackHits = new();

	// A765: the rows that adjoin <act> without a gap, carry its name and have no cast time - the hits of a cast that lands as
	// another action, such as Claw and Tail (#43132 cast as a line, #43131 hitting as a cone behind).
	private static Action[] SameAttackHits(Action act)
	{
		return _sameAttackHits.GetOrAdd(act.RowId, static (id, cast) =>
		{
			var name = cast.Name.ExtractText();
			if (string.IsNullOrEmpty(name))
			{
				return [];
			}

			var sheet = Service.GetSheet<Action>();
			var parts = new List<Action>();
			foreach (var step in new[] { -1, 1 })
			{
				for (var row = (long)id + step; row > 0 && row <= uint.MaxValue; row += step)
				{
					if (sheet.GetRowOrDefault((uint)row) is not { } other || other.Name.ExtractText() != name)
					{
						break;
					}

					// A row without an effect range draws no area; read alone it would reach everyone.
					if (other.Cast100ms == 0 && other.EffectRange > 0)
					{
						parts.Add(other);
					}
				}
			}

			return [.. parts];
		}, act);
	}

	private static bool AreaShapeReaches(IBattleChara h, Action act, IBattleChara member)
	{
		if ((CastType)act.CastType == CastType.Targeted && act.Range > 0)
		{
			return h.CastTargetObjectId == member.GameObjectId;
		}

		// A766: a cone or a line hits where it points, not whom it names; a circle or ring on a target hits its target.
		var (degrees, inner) = OmenShape(act);
		var directional = (act.CastType is (byte)CastType.StraightLine or LineFromCasterCastType && act.XAxisModifier > 0)
			|| ((IsConeCastType(act.CastType) || (CastType)act.CastType == CastType.Donut) && degrees > 0f);
		if (act.EffectRange == 0 || (h.CastTargetObjectId == member.GameObjectId && !directional))
		{
			return true;
		}

		var aimedAt = h.CastTargetObjectId != h.GameObjectId
			? Svc.Objects.SearchById(h.CastTargetObjectId) as IBattleChara
			: null;
		if (aimedAt != null && !aimedAt.IsValid())
		{
			aimedAt = null;
		}

		if ((CastType)act.CastType == CastType.Circle && act.Range > 0 && !act.CanTargetSelf && !act.TargetArea && aimedAt != null)
		{
			return HorizontalDistance(member.Position, aimedAt.Position) - member.HitboxRadius - aimedAt.HitboxRadius <= act.EffectRange;
		}

		// A731: types 3, 4 and 5 reach past the caster's hitbox, 2, 12 and 13 do not (BossModReborn's analysis, "range X+R").
		var casterRadius = (CastType)act.CastType is CastType.Cone or CastType.StraightLine or CastType.Unknown5 ? h.HitboxRadius : 0f;

		if (act.CastType is (byte)CastType.StraightLine or LineFromCasterCastType && act.XAxisModifier > 0)
		{
			var direction = CastDirection(h);
			if (direction.LengthSquared() > 0f)
			{
				direction = Vector3.Normalize(direction);
				var toMember = member.Position - h.Position;
				toMember.Y = 0;
				var along = Vector3.Dot(direction, toMember);
				var across = Vector3.Cross(direction, toMember).Length();
				return along >= -member.HitboxRadius
					&& along <= act.EffectRange + casterRadius + member.HitboxRadius
					&& across <= (act.XAxisModifier / 2f) + member.HitboxRadius;
			}
		}

		if (act.TargetArea)
		{
			// A766: around the place the game keeps for the cast; without one, from the caster with both hitboxes.
			return CastLocation(h) is { } place
				? HorizontalDistance(member.Position, place) - member.HitboxRadius <= act.EffectRange
				: HorizontalDistance(member.Position, h.Position) - member.HitboxRadius - h.HitboxRadius <= act.EffectRange;
		}

		// A731: a cone by the angle its omen names, in the cast's direction (A766); a ring with a sector in its omen counts as
		// that cone without its inner circle.
		if ((IsConeCastType(act.CastType) || (CastType)act.CastType == CastType.Donut) && degrees > 0f)
		{
			return InCone(h.Position, CastDirection(h), act.EffectRange + casterRadius, degrees, member);
		}

		// A731: a ring spares who stands wholly inside its inner circle.
		if ((CastType)act.CastType == CastType.Donut && inner > 0f)
		{
			var centre = act.Range > 0 && !act.CanTargetSelf && aimedAt != null ? aimedAt.Position : h.Position;
			var distance = HorizontalDistance(member.Position, centre);
			return distance - member.HitboxRadius <= act.EffectRange && distance + member.HitboxRadius >= inner;
		}

		return HorizontalDistance(member.Position, h.Position) - member.HitboxRadius <= act.EffectRange + casterRadius;
	}

	// A766 (concept 13, "Wirkrichtung und Ort aus dem Spiel"): the direction the game keeps for the cast, as BossMod reads
	// it (Character.CastRotation); the omen and the hit keep it while the target moves. The caster's facing without one.
	private static unsafe Vector3 CastDirection(IBattleChara h)
	{
		if (h.Address == nint.Zero || !h.IsCasting)
		{
			return h.GetFaceVector();
		}

		var rotation = ((FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)h.Address)->CastRotation;
		return new Vector3(MathF.Sin(rotation), 0f, MathF.Cos(rotation));
	}

	// A766: the place of a ground-targeted cast as the game keeps it (CastInfo.TargetLocation); null while it gives none.
	internal static unsafe Vector3? CastLocation(IBattleChara h)
	{
		if (h.Address == nint.Zero || !h.IsCasting)
		{
			return null;
		}

		var info = ((FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)h.Address)->GetCastInfo();
		if (info == null)
		{
			return null;
		}

		var place = new Vector3(info->TargetLocation.X, info->TargetLocation.Y, info->TargetLocation.Z);
		return place.LengthSquared() > 0f ? place : null;
	}

	// A731: cast type 13 is a cone like type 3 (Cone), which the enum does not name, measured from the caster's centre.
	private const byte ConeFromCentreCastType = 13;

	private static bool IsConeCastType(byte castType) => castType == (byte)CastType.Cone || castType == ConeFromCentreCastType;

	// A731: whether <member> stands in a cone from <origin> towards <direction> - within its reach and its angle, its hitbox
	// in favour of the hit.
	private static bool InCone(Vector3 origin, Vector3 direction, float reach, float degrees, IBattleChara member)
	{
		var toMember = member.Position - origin;
		toMember.Y = 0;
		var distance = toMember.Length();
		if (distance - member.HitboxRadius > reach)
		{
			return false;
		}

		direction.Y = 0;
		if (distance <= member.HitboxRadius || direction.LengthSquared() <= 0f)
		{
			return true;
		}

		var cosine = Math.Clamp(Vector3.Dot(Vector3.Normalize(direction), toMember / distance), -1f, 1f);
		var allowance = MathF.Asin(Math.Min(1f, member.HitboxRadius / distance));
		return MathF.Acos(cosine) <= float.DegreesToRadians(degrees / 2f) + allowance;
	}

	// A731: per action, the cone angle and the ring's inner radius its omen names (0 for none).
	private static readonly ConcurrentDictionary<uint, (float Degrees, float Inner)> _omenShapes = new();

	// Cast type 12 is a rectangle from the caster like type 4 (StraightLine), which the enum does not
	// name: the area list's type-12 entries carry a length and a width (Diffuse Laser 60 by 60, the
	// line stacks 60 by 8).
	private const byte LineFromCasterCastType = 12;

	/// <summary>
	/// A701: the listed knockback that opened the anti-knockback flag in this frame - its caster and action -, for the
	/// trace line of the flag; null while none reaches the player.
	/// </summary>
	internal static (IBattleChara Caster, Action Action)? KnockbackReachingPlayer { get; private set; }

	/// <summary>
	/// Whether the raidwide BossModReborn predicts hits the player, from the entry's own list of whom it hits;
	/// null when that list belongs to another entry or the prediction comes from the timeline, which names
	/// nobody. BossModReborn's "raidwide" also covers towers, baits, spreads and charges on single players
	/// (A273).
	/// </summary>
	public static bool? BMRRaidwideHitsPlayer { get; set; }

	/// <summary>One enemy cast a BossModReborn raidwide prediction came with (A301).</summary>
	internal readonly record struct BmrRaidwideCast(ulong CasterId, uint ActionId);

	/// <summary>
	/// The enemy casts the current BossModReborn raidwide prediction came with: those that began in the update in
	/// which its landing time first appeared. Empty when it appeared at no cast's start.
	/// </summary>
	/// <remarks>
	/// <para>BossModReborn names no action for a prediction. Its time does not tie it to a cast either: against a
	/// running listed cast the predicted landing lay 0.4 to 2.9 s after the cast's end in his traces of 04. to
	/// 16.10.2026, depending on the module. Its start does: a cast-bound raidwide component adds the prediction
	/// when the cast begins and drops it when the cast ends (<c>Components.RaidwideCast</c>, landing
	/// <c>CastFinishAt</c>), so a landing time that appears in the same update as a cast's start came with that
	/// cast.</para>
	///
	/// <para>Every enemy cast that began then is kept, the untargetable helpers included whatever they cast: a
	/// visual cast on the list and the real hit from a helper beside it must not read as the visual cast alone.
	/// Recognition only; whether a cast kept here is worth a mitigation is asked by the reader.</para>
	/// </remarks>
	internal static IReadOnlyList<BmrRaidwideCast> BMRRaidwideCasts => _bmrRaidwideCasts;

	/// <summary>
	/// Called with every reading of <see cref="BMRNextRaidwideIn"/>, after the hostile lists of the same update
	/// are built: keeps <see cref="BMRRaidwideCasts"/> for the prediction it describes.
	/// </summary>
	internal static void NoteBmrRaidwide(float raidwideIn)
	{
		var now = DateTime.Now;
		var frame = Svc.Framework.UpdateDelta;
		var previousRead = _bmrRaidwideReadAt;
		var previousFrame = _bmrRaidwideReadFrame;
		_bmrRaidwideReadAt = now;
		_bmrRaidwideReadFrame = frame;

		if (raidwideIn is <= 0f or float.MaxValue)
		{
			_bmrRaidwideLandsAt = DateTime.MinValue;
			_bmrRaidwideCasts = [];
			return;
		}

		// The same prediction keeps its landing time; BossModReborn's own reading may lag RSR's by a frame.
		var landsAt = now + TimeSpan.FromSeconds(raidwideIn);
		var lag = frame > previousFrame ? frame : previousFrame;
		if (_bmrRaidwideLandsAt != DateTime.MinValue && (landsAt - _bmrRaidwideLandsAt).Duration() <= lag)
		{
			return;
		}

		_bmrRaidwideLandsAt = landsAt;
		_bmrRaidwideCasts = previousRead == DateTime.MinValue ? [] : CastsBegunBetween(previousRead - lag, now);
	}

	private static List<BmrRaidwideCast> CastsBegunBetween(DateTime after, DateTime until)
	{
		List<BmrRaidwideCast> begun = [];
		HashSet<ulong> seen = [];
		void Consider(IBattleChara h)
		{
			if (!h.IsCasting || !seen.Add(h.GameObjectId))
			{
				return;
			}

			var began = until - TimeSpan.FromSeconds(h.CurrentCastTime);
			if (began > after && began <= until)
			{
				begun.Add(new BmrRaidwideCast(h.GameObjectId, h.CastActionId));
			}
		}

		foreach (var h in AllHostileTargets)
		{
			if (h != null && h.HasNativeState())
			{
				Consider(h);
			}
		}

		foreach (var obj in Svc.Objects)
		{
			if (obj is IBattleChara h && h.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.BattleNpc
				&& !h.IsTargetable && h.HasNativeState() && h.IsEnemySideFigure())
			{
				Consider(h);
			}
		}

		return begun;
	}

	/// <summary>
	/// True while the player holds a movement key, as BossMod reads it (<c>Movement.IsMoveRequested</c>). BossMod
	/// then sets <see cref="BMRMaxCastTime"/> to zero, which says nothing about danger (concept 03, "Bewegungssperre beim
	/// Wirken und die KI von BossMod").
	/// </summary>
	public static bool BMRMoveRequested { get; set; }

	/// <summary>True while BossMod's AI steers the player toward a destination (<c>AI.IsNavigating</c>).</summary>
	public static bool BMRIsNavigating { get; set; }

	/// <summary>Where BossMod's AI steers the player (<c>AI.NaviTargetPos</c>); null when it does not.</summary>
	public static Vector3? BMRNaviTargetPos { get; set; }

	/// <summary>The player's movement speed as BossMod's AI reads it (<c>AI.PlayerSpeed</c>); zero without it.</summary>
	public static float BMRPlayerSpeed { get; set; }

	/// <summary>No tank of the party is alive beside the player, a blue mage counting in his mimicry's role: nobody holds the
	/// enemies off him (concept 01, C1 at the pull, A542).</summary>
	public static bool NoTankBeside => !AnyLiving(JobRole.Tank, true);

	private static bool _dangerFirstAlone;
}
