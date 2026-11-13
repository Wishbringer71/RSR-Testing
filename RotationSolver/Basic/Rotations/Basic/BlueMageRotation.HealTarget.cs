using CombatRole = RotationSolver.Basic.Data.CombatRole;

namespace RotationSolver.Basic.Rotations.Basic;

public partial class BlueMageRotation
{
	/// <summary>
	/// Whether the player's own health, carried forward by its measured trend over <paramref name="seconds"/>, falls into
	/// the heal chain's critical class (concept 07). False while health is not falling on balance.
	/// </summary>
	protected static bool PlayerFallsWithin(float seconds)
	{
		var player = Player;
		if (player == null)
		{
			return false;
		}

		var ttk = player.GetCorrectedTTK();
		if (float.IsNaN(ttk) || ttk <= 0f)
		{
			return false;
		}

		var left = player.GetEffectiveHpPercent() * Math.Max(0f, 1f - (seconds / ttk));
		return left <= HealthForDyingTanks * 100f;
	}

	/// <summary>
	/// Moon Flute's Waxing and Waning Nocturne and the damage it adds, from its effect text; false when the text states them
	/// not.
	/// </summary>
	protected bool TryMoonFluteText(out float damageUp, out float waxing, out float waning)
	{
		waxing = 0f;
		waning = 0f;
		return ActionTooltip.TryStatedDamageUp(MoonFlutePvE.ID, out damageUp)
			& ActionTooltip.TryStatedDuration(MoonFlutePvE.ID, out waxing)
			& ActionTooltip.TrySecondStatedDuration(MoonFlutePvE.ID, out waning);
	}

	private readonly Dictionary<byte, (float Potency, int Gcds)> _windowGroups = [];
}
