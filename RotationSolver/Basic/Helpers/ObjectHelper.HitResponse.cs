using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using Lumina.Excel.Sheets;
using RotationSolver.Basic.Configuration;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace RotationSolver.Basic.Helpers;

public static partial class ObjectHelper
{
	/// <summary>
	/// How long an off-GCD heal decided on now takes to go out: until the current animation lock and
	/// any cast in progress have ended - nothing is woven over a hard cast (concept 08, A217).
	/// </summary>
	internal static float GetInstantHealLeadTime()
	{
		var player = Player.Object;
		var castLeft = player != null && player.IsCasting
			? Math.Max(0f, player.TotalCastTime - player.CurrentCastTime)
			: 0f;
		return Math.Max(Math.Max(0f, DataCenter.AnimationLock), castLeft);
	}

	/// <summary>
	/// How long a heal decided on now takes to land: what is left of this GCD, plus one GCD for the
	/// cast. Read from the game, so a shortened GCD shortens the look-ahead with it.
	/// </summary>
	internal static float GetHealLeadTime() => _healLead.Value;

	/// <summary>
	/// Is anything actually threatening this character right now?
	/// </summary>
	/// <remarks>
	/// The user's requirement, in his words: an emergency heal on a freshly raised player is right
	/// "falls Gefahr bevorsteht, z.B. grosser heftiger AoE" - but "wenn der Spieler aber keine Aggro
	/// hat, kein AoE ansteht, oder kein sonstiger Schaden ansteht, wuerde doch HoT oder kleinere
	/// Heals bzw. beides reichen".
	///
	/// That is three questions, and the tree can answer all three:
	/// <list type="bullet">
	/// <item>aimed at - an enemy is attacking this member, or casting something that will land on
	/// them (<see cref="DataCenter.TargetedPartyMembers"/>)</item>
	/// <item>an announced area cast - <see cref="DataCenter.IsHostileCastingAOE"/></item>
	/// <item>damage actually arriving - the health trend has a finite time to zero</item>
	/// </list>
	///
	/// Deliberately not a rule about raising. A player who has just been resurrected is only the
	/// most visible case: he holds a few percent, carries no aggro and takes no damage, so every
	/// threshold in the tree reads him as the most urgent member in the party while nothing at all
	/// is happening to him. The same is true of a damage dealer who has just run out of an area
	/// effect. The user's requirement is about danger, not about raising, and so is this.
	///
	/// The third arm is why this is worth the name: aggro and an announced cast are both predictions
	/// that can miss - a boss mechanic that neither casts nor retargets shows up in neither - and
	/// the health trend catches what they miss, one sample after the first hit lands. An emergency
	/// heal is an ability without a cast time, so the cost of finding out that way is a delay, not a
	/// death.
	///
	/// Errs towards "threatened": every arm that cannot answer says yes by staying silent, and the
	/// caller then behaves exactly as it does today.
	/// </remarks>
	/// <summary>
	/// Is anybody in the party going to fall before a heal decided on now could reach them?
	/// </summary>
	/// <remarks>
	/// The user's bound on every rule that yields a GCD: suspending something is only right while
	/// the incoming damage is still manageable. Manageable is not a number of enemies and not a sum
	/// of their output - it is whether the group holds, and that is measured rather than set.
	///
	/// A rule that gives up a GCD costs the party exactly that GCD. So the question is whether
	/// everyone survives it with enough left over to be healed afterwards, which is the same lead
	/// time the forward-looking health uses: the rest of this GCD plus one for the cast.
	///
	/// Nobody falling means NaN everywhere, and NaN is not a short time - it is no death in sight.
	/// So a party being held steady never trips this, however many enemies are standing on it, and
	/// a single member actually going down stops the hold even if the pack is small. That is the
	/// intended shape: the old proxy measured the pack, and a big pack that is being healed through
	/// is precisely where stretching the throttle pays.
	///
	/// It answers for the whole party rather than for the tank alone, which is deliberately
	/// conservative: a damage dealer standing in fire blocks the hold even though the stun would not
	/// have helped him. The cost of that is one GCD of stun stretching, and the user's standing
	/// order puts the group's survival ahead of output.
	/// </remarks>
	internal static bool AnyPartyMemberFallingWithinHealWindow()
	{
		var lead = GetHealLeadTime();
		if (lead <= 0f)
		{
			return false;
		}

		var party = DataCenter.PartyMembers;
		for (var i = 0; i < party.Count; i++)
		{
			var member = party[i];
			if (member == null || member.IsDead)
			{
				continue;
			}

			var ttk = member.GetCorrectedTTK();
			if (!float.IsNaN(ttk) && ttk <= lead)
			{
				return true;
			}
		}
		return false;
	}

	internal static bool IsUnderThreat(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return true;
		}

		if (DataCenter.TargetedPartyMembers.Contains(battleChara.GameObjectId))
		{
			return true;
		}

		if (DataCenter.IsHostileCastingAOE)
		{
			return true;
		}

		// A finite time to zero means the health is falling on balance - net of every mitigation,
		// barrier and foreign heal. NaN means it is not, which is the answer this asks for.
		return !float.IsNaN(battleChara.GetCorrectedTTK());
	}

	/// <summary>
	/// <see cref="GetHealthRatio"/> carried forward to the moment a heal begun now would land. With
	/// "Heal ahead of incoming damage" off it is exactly <see cref="GetHealthRatio"/>. Every healing
	/// threshold reads this, as that setting's text promises.
	/// </summary>
	public static float GetForecastHealthRatio(this IBattleChara battleChara)
	{
		return battleChara.GetHealthRatio() * battleChara.GetForecastSurvivingShare();
	}

	/// <summary>
	/// <see cref="GetForecastHealthRatio(IBattleChara)"/> for a heal of the given kind: an off-GCD heal
	/// (<paramref name="instant"/>) looks ahead only as far as the current animation lock.
	/// </summary>
	public static float GetForecastHealthRatio(this IBattleChara battleChara, bool instant)
	{
		return battleChara.GetHealthRatio() * battleChara.GetForecastSurvivingShare(instant);
	}

	/// <summary>
	/// The health ratio this character is expected to hold <paramref name="seconds"/> from now, at the
	/// measured net rate - every heal, barrier and mitigation already in it - with the forecast error of
	/// the time to death taken out (<see cref="GetCorrectedTTK"/>). Today's ratio while the health is not
	/// falling on balance. For a cooldown whose worth lasts that long, asked independently of "Heal ahead
	/// of incoming damage", whose text promises the moment a heal lands.
	/// </summary>
	internal static float GetHealthRatioIn(this IBattleChara battleChara, float seconds)
	{
		var ratio = battleChara.GetHealthRatio();
		var ttk = battleChara.GetCorrectedTTK();
		if (float.IsNaN(ttk) || ttk <= 0f || seconds <= 0f)
		{
			return ratio;
		}

		return ratio * Math.Clamp(1f - (seconds / ttk), 0f, 1f);
	}

	/// <summary>
	/// <see cref="GetEffectiveHp"/> carried forward to the moment a heal begun now would land.
	/// </summary>
	internal static uint GetForecastEffectiveHp(this IBattleChara battleChara, bool instant = false)
	{
		return (uint)(battleChara.GetEffectiveHp() * battleChara.GetForecastSurvivingShare(instant));
	}
}
