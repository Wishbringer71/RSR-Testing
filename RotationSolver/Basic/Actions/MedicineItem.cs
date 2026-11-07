using Lumina.Excel.Sheets;

namespace RotationSolver.Basic.Actions;

/// <summary>
/// The type of the medicine
/// </summary>
public enum MedicineType : byte
{
	/// <summary>
	/// 
	/// </summary>
	None,

	/// <summary>
	/// 
	/// </summary>
	Strength,

	/// <summary>
	/// 
	/// </summary>
	Dexterity,

	/// <summary>
	/// 
	/// </summary>
	Vitality,

	/// <summary>
	/// 
	/// </summary>
	Intelligence,

	/// <summary>
	/// 
	/// </summary>
	Mind,
}

internal class MedicineItem : BaseItem
{
	public MedicineType Type { get; }

	public MedicineItem(Item item) : base(item)
	{
		Type = _item.Unknown4 switch
		{
			10120 => MedicineType.Strength,
			10140 => MedicineType.Dexterity,
			10160 => MedicineType.Vitality,
			10180 => MedicineType.Intelligence,
			10200 => MedicineType.Mind,
			_ => MedicineType.None,
		};
	}

	// Switched for every job as an exclusion, like the potions (A645): whether a job drinks tinctures and where is its
	// "Tincture usage"; which grade is a question of the item. The per-job switch, off by default since upstream
	// 3e9b5b164, kept the tincture out of every burst on each job where it had not been switched on by hand.
	public override bool SwitchForAllJobs => true;

	// A tincture is used like an action and would end a running Flamethrower channel (A380).
	protected override bool CanUseThis => !DataCenter.FlamethrowerChannelRunning && CustomRotation.TinctureAllowedHere;
}
