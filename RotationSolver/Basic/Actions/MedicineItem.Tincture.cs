namespace RotationSolver.Basic.Actions;

internal partial class MedicineItem
{
	// The column of ItemAction.Data that holds a tincture's seconds: status 49, its value, its duration (A780).
	private const int EffectSecondsColumn = 2;

	/// <summary>How long the tincture's Medicated lasts, from the item's action (30 s for the gemdraughts).</summary>
	internal float EffectSeconds => _item.ItemAction.Value.Data[EffectSecondsColumn];
}
