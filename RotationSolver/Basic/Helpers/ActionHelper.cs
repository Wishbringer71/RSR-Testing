using Action = Lumina.Excel.Sheets.Action;

namespace RotationSolver.Basic.Helpers;

internal static class ActionHelper
{
	internal const byte GCDCooldownGroup = 58;

	internal const byte AbilityCooldownGroup = 19;

	internal const byte OGCDCooldownGroup = 71;

	// The Action sheet states cast times in tenths of a second (Cast100ms).
	private const float CastUnitsPerSecond = 10f;

	/// <summary>
	/// The action's cast time in seconds, from the sheet: no new cast of it lands sooner, so a further landing within it
	/// belongs to the same use - the series of the area and of the tankbuster measurement (A349, A454).
	/// </summary>
	internal static float CastSeconds(this Action action) => action.Cast100ms / CastUnitsPerSecond;

	/// <summary>
	/// Gets the action category of the specified action.
	/// </summary>
	/// <param name="action">The action to get the category for.</param>
	/// <returns>The action category.</returns>
	public static ActionCate GetActionCate(this Action action)
	{
		return (ActionCate)(action.ActionCategory.IsValid ? action.ActionCategory.Value.RowId : 0);
	}

	internal static bool IsGeneralGCD(this Action action)
	{
		return action.CooldownGroup == GCDCooldownGroup;
	}

	internal static bool IsRealGCD(this Action action)
	{
		return action.CooldownGroup == GCDCooldownGroup || action.AdditionalCooldownGroup == GCDCooldownGroup;
	}

	internal static byte GetCoolDownGroup(this Action action)
	{
		var group = action.CooldownGroup == GCDCooldownGroup ? action.AdditionalCooldownGroup : action.CooldownGroup;
		return group == 0 ? GCDCooldownGroup : group;
	}

	internal static bool IsInJob(this Action action)
	{
		return action.ClassJobCategory.ValueNullable?.DoesJobMatchCategory(DataCenter.Job) ?? true;
	}

	internal static bool CanUseGCD => DataCenter.DefaultGCDRemain <= DataCenter.CalculatedActionAhead;
}