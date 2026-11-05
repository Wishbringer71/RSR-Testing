namespace RotationSolver.Basic.Rotations.Basic;

/// <summary>
/// K2 of the Crucible (concept 01, "Crucible of the Unbroken", stage 1, A548): a familiar that hits only the weakness of
/// enemies still to come waits for them, and comes first once they appear. The fights, weaknesses and the fight in progress
/// come from <see cref="CrucibleBoard"/>.
/// </summary>
public partial class BeastmasterRotation
{
	// The weaknesses each familiar's Trick and Release hit, by familiar; what K2 has traced in this fight.
	private static readonly Dictionary<BeastmasterPet, HashSet<uint>> _familiarElements = [];
	private static readonly HashSet<BeastmasterPet> _crucibleHeldTraced = [];
	private static uint _crucibleHeldFight;

	/// <summary>The weaknesses (XBMElement rows) the Trick and the Tempered Release of <paramref name="profile"/> hit.</summary>
	internal static HashSet<uint> ElementsOf(FamiliarProfile profile)
	{
		if (_familiarElements.TryGetValue(profile.Pet, out var known))
		{
			return known;
		}

		var hits = new HashSet<uint>(CrucibleBoard.ElementsOf(profile.TrickId));
		hits.UnionWith(CrucibleBoard.ElementsOf(profile.ReleaseId));
		_familiarElements[profile.Pet] = hits;
		return hits;
	}

	/// <summary>Follows the Crucible every frame (stage 1, A548).</summary>
	internal static void FollowCrucible()
	{
		CrucibleBoard.Follow();
		if (_crucibleHeldFight != CrucibleBoard.Fight)
		{
			_crucibleHeldFight = CrucibleBoard.Fight;
			_crucibleHeldTraced.Clear();
		}
	}

	/// <summary>
	/// K2 (A548): <paramref name="profile"/> hits the weakness of an enemy of this fight that was not there when it began and
	/// is not there yet, and the weakness of none that is here.
	/// </summary>
	internal static bool HitsOnlyEnemiesToCome(FamiliarProfile profile)
	{
		// A581: before combat tells the fight, the battle the horn screen named, with the enemies standing now.
		var told = CrucibleBoard.Fight != 0;
		var fight = told ? CrucibleBoard.Fight : CrucibleBoard.Upcoming;
		if (fight == 0)
		{
			return false;
		}

		var hits = ElementsOf(profile);
		var coming = false;
		foreach (var enemy in told ? CrucibleBoard.Enemies : CrucibleBoard.EnemiesOf(fight))
		{
			if (enemy.Weakness == 0 || !hits.Contains(enemy.Weakness))
			{
				continue;
			}

			if (told ? CrucibleBoard.IsHere(enemy.NameId) : CrucibleBoard.PresentNow(enemy.NameId))
			{
				return false;
			}

			coming |= told ? CrucibleBoard.IsToCome(enemy.NameId) : DataCenter.HostileListLive;
		}

		if (coming && _crucibleHeldTraced.Add(profile.Pet))
		{
			DefenseTrace.Line($"crucible: {profile.Pet} held for the enemies to come - it hits their weakness, none of those here");
		}

		return coming;
	}

	private static readonly System.Text.RegularExpressions.Regex ComboPotency = new(@"Combo Potency: (?<value>[\d,]+)",
		System.Text.RegularExpressions.RegexOptions.CultureInvariant);
	private static uint _waveTracedFight;

	/// <summary>
	/// A558: on a board, whether Quelling Wave deals more than the axe combo's mean step on <paramref name="target"/> - its
	/// stated potency times one plus the measured weakness bonus when its element is the target's weakness, against the mean
	/// of Smash Axe's potency and the combo potencies of Axeblade Bite and Shieldsplitter, each with the bonus where it hits
	/// the weakness. It preserves the combo, so it pushes the combo one GCD later; false before the bonus is measured.
	/// </summary>
	internal static bool WaveBeatsCombo(IBattleChara? target)
	{
		if (!IsInCrucible || target == null || CrucibleBoard.Fight == 0)
		{
			return false;
		}

		var weakness = CrucibleBoard.WeaknessOf(target.NameId);
		var bonus = CrucibleBoard.WeaknessBonus;
		if (weakness == 0 || bonus <= 0f || !CrucibleBoard.ElementsOf((uint)ActionID.QuellingWavePvE).Contains(weakness)
			|| !ActionTooltip.TryStatedPotency((uint)ActionID.QuellingWavePvE, out var wave)
			|| !ActionTooltip.TryStatedPotency((uint)ActionID.SmashAxePvE, out var first)
			|| !ActionTooltip.TryStated((uint)ActionID.AxebladeBitePvE, ComboPotency, out var second)
			|| !ActionTooltip.TryStated((uint)ActionID.ShieldsplitterPvE, ComboPotency, out var third))
		{
			return false;
		}

		var waveWorth = wave * (1f + bonus);
		float[] steps = [Worth(ActionID.SmashAxePvE, first), Worth(ActionID.AxebladeBitePvE, second),
			Worth(ActionID.ShieldsplitterPvE, third)];
		var comboWorth = 0f;
		foreach (var step in steps)
		{
			comboWorth += step;
		}

		comboWorth /= steps.Length;
		var beats = waveWorth > comboWorth;
		if (beats && _waveTracedFight != CrucibleBoard.Fight)
		{
			_waveTracedFight = CrucibleBoard.Fight;
			DefenseTrace.Line($"crucible: Quelling Wave before the axes on {target.Name} ({CrucibleBoard.ElementName(weakness)} "
				+ $"weakness, bonus {bonus:P0}): {waveWorth:F0} against the combo's mean step {comboWorth:F0}");
		}

		return beats;

		float Worth(ActionID axe, float potency)
			=> potency * (1f + (CrucibleBoard.ElementsOf((uint)axe).Contains(weakness) ? bonus : 0f));
	}

	/// <summary>
	/// K2 (A548): <paramref name="profile"/> hits the weakness of an enemy here now that was not there when the fight began.
	/// </summary>
	internal static bool HitsEnemyThatCame(FamiliarProfile profile)
	{
		if (CrucibleBoard.Fight == 0)
		{
			return false;
		}

		var hits = ElementsOf(profile);
		foreach (var enemy in CrucibleBoard.Enemies)
		{
			if (enemy.Weakness != 0 && hits.Contains(enemy.Weakness) && CrucibleBoard.IsHere(enemy.NameId)
				&& !CrucibleBoard.WasThereAtStart(enemy.NameId))
			{
				return true;
			}
		}

		return false;
	}
}
