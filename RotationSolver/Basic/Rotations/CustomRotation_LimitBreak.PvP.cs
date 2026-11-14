using ECommons.DalamudServices;

namespace RotationSolver.Basic.Rotations;

public partial class CustomRotation
{
	/// <summary>
	/// The job's own Limit Break in PvP, once the game allows it (a full gauge), with the condition of its kind (concept 25,
	/// "PvP: Limit Break", A756). Until A756 RSR never pressed it: every job's base names it (`LimitBreakPvP`), nothing read it.
	/// - A finishing blow on one target waits until it kills: its stated potency (one potency is one HP in PvP) covers the
	///   target's HP and barrier, or the target is below the share its text names, or carries Kuzushi for Zantetsuken.
	/// - A shield for the party goes when the player stands in the critical class.
	/// - One aimed at an enemy goes on a target: it hits at least that one.
	/// - A circle around the player goes with an enemy inside it; a line or cone from the player needs the enemy count of
	///   its setting ("AoE Count"), as it hits only what stands in its direction.
	/// - One that only grants the player a state goes with an enemy within the rotation's reach.
	/// </summary>
	private bool PvPLimitBreak(out IAction? act)
	{
		return PvPLimitBreakFor(out act) && act is IBaseAction limitBreak
			&& (!Rushes.IsMatch(ActionTooltip.Text(limitBreak.ID))
				|| GapCloserAllowed(limitBreak.Target.Target, pressed: (ActionID)limitBreak.ID));
	}

	// A Limit Break that carries the player onto its target ("Rushes target", "Swiftly charges forward", "a jumping ...
	// attack") goes within the reach he allows damaging gap closers, as every other leap (concept 25, "PvP: Bewegung").
	private static readonly System.Text.RegularExpressions.Regex Rushes = new(@"Rushes target|charges forward|jumping (?:physical )?attack",
		System.Text.RegularExpressions.RegexOptions.Compiled);

	private bool PvPLimitBreakFor(out IAction? act)
	{
		act = null;
		var limitBreak = LimitBreakPvP;
		// Only the Limit Break itself, not the action its button turns into after it (Terminal Trigger, Sky Shatter, Oracle
		// ...): those end the Limit Break's effect early, the rotations cast them, and most fire on their own at its end
		// (concept 25, "PvP: Limit Break", A760).
		if (!DataCenter.IsPvP || !DataCenter.InCombat || limitBreak == null || Player == null || !IsGaugeLimitBreak(limitBreak))
		{
			return false;
		}

		switch (limitBreak.ID)
		{
			case PvPLimitBreakRow.MarksmansSpite:
			case PvPLimitBreakRow.Meteodrive:
				// The potency in damage as the player's own plain hits in this area showed it; one HP a potency before the first
				// (concept 25, "PvP: Limit Break", A759).
				var perPotency = OwnHitWatch.DamagePerPotency;
				return ActionTooltip.TryStatedTotalPotency(limitBreak.ID, out var potency)
					&& limitBreak.CanUse(out act, skipAoeCheck: true, targetOverride: TargetType.LowHP)
					&& limitBreak.Target.Target is { } target
					&& target.GetEffectiveHp() <= potency * (float.IsNaN(perPotency) ? 1f : perPotency);

			case PvPLimitBreakRow.SeitonTenchu:
				return ActionTooltip.TryStatedHealthBelow(limitBreak.ID, out var below)
					&& limitBreak.CanUse(out act, skipAoeCheck: true, targetOverride: TargetType.LowHPPercent)
					&& limitBreak.Target.Target is { } low && low.GetHealthRatio() < below;

			case PvPLimitBreakRow.Zantetsuken:
				// The job's base gives it Kuzushi as the status its target needs (SamuraiRotation.PvP.cs, A761).
				return limitBreak.CanUse(out act, skipAoeCheck: true);

			case PvPLimitBreakRow.Phalanx:
			case PvPLimitBreakRow.Mesotes:
				return Player.IsInCriticalClass() && limitBreak.CanUse(out act, skipAoeCheck: true);

			case PvPLimitBreakRow.SkyHigh:
				// Sky High lands as Sky Shatter around the player; its own row has no radius.
				return NumberOfHostilesInRangeOf(EffectRangeOf(PvPLimitBreakRow.SkyShatter)) > 0 && limitBreak.CanUse(out act);
		}

		var row = limitBreak.Action;
		if (row.CanTargetHostile && row.Range > 0)
		{
			return limitBreak.CanUse(out act, skipAoeCheck: true);
		}

		if (row.TargetArea || (CastType)row.CastType is CastType.StraightLine or CastType.Cone)
		{
			return limitBreak.CanUse(out act);
		}

		return (row.EffectRange > 0 ? NumberOfHostilesInRangeOf(row.EffectRange) > 0 : HasHostilesInMaxRange)
			&& limitBreak.CanUse(out act, skipAoeCheck: true);
	}

	/// <summary>
	/// The job's shielding Limit Break where Guard is due (concept 25, "PvP: Wechselwirkungen und Synergien", A757):
	/// Phalanx (10 s impervious, the party 33 % less damage), Mesotes (damage from outside its zone negated) and Sky High
	/// (4 s out of reach, then a 24,000 barrier with Sky Shatter) hold longer or cover more than Guard's 4 s on the player
	/// alone; so does Eventide, whose Undead Redemption keeps the HP above 1 against most attacks for 10 s and heals with every
	/// weaponskill (A761, a reading of its text). Guard afterwards would end them, and `GuardWastedStatuses` keeps it off under
	/// Hallowed Ground, Sky High and Undead Redemption.
	/// </summary>
	private bool PvPShieldLimitBreak(out IAction? act)
	{
		act = null;
		var limitBreak = LimitBreakPvP;
		return DataCenter.IsPvP && limitBreak != null && IsGaugeLimitBreak(limitBreak)
			&& limitBreak.ID is PvPLimitBreakRow.Phalanx or PvPLimitBreakRow.Mesotes or PvPLimitBreakRow.SkyHigh
				or PvPLimitBreakRow.Eventide
			&& limitBreak.CanUse(out act, skipAoeCheck: true);
	}

	/// <summary>
	/// Whether the Limit Break button holds a Limit Break of the gauge: its own action, or one it turns into that its text
	/// names by the full gauge - Summon Phoenix after Summon Bahamut. Not a follow-up such as Sky Shatter or Terminal
	/// Trigger, whose text names the Limit Break's effect instead (concept 25, "PvP: Limit Break", A760).
	/// </summary>
	private static bool IsGaugeLimitBreak(IBaseAction limitBreak)
	{
		return limitBreak.AdjustedID == limitBreak.ID
			|| ActionTooltip.Text(limitBreak.AdjustedID).Contains("limit gauge is full", StringComparison.Ordinal);
	}

	private static float EffectRangeOf(uint row)
	{
		var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>();
		return sheet != null && sheet.TryGetRow(row, out var action) ? action.EffectRange : 0f;
	}

	/// <summary>
	/// Rows of the Action sheet for the PvP Limit Breaks with a condition of their own. The generated `ActionID` names none
	/// of them: each job's base creates its Limit Break from the row number, as `(ActionID)29415`.
	/// </summary>
	private static class PvPLimitBreakRow
	{
		public const uint MarksmansSpite = 29415;
		public const uint Meteodrive = 29485;
		public const uint SeitonTenchu = 29515;
		public const uint Zantetsuken = 29537;
		public const uint Phalanx = 29069;
		public const uint Mesotes = 29266;
		public const uint SkyHigh = 29497;
		public const uint SkyShatter = 29498;
		public const uint Eventide = 29097;
	}
}
