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
		if (CrucibleBoard.Fight == 0)
		{
			return false;
		}

		var hits = ElementsOf(profile);
		var coming = false;
		foreach (var enemy in CrucibleBoard.Enemies)
		{
			if (enemy.Weakness == 0 || !hits.Contains(enemy.Weakness))
			{
				continue;
			}

			if (CrucibleBoard.IsHere(enemy.NameId))
			{
				return false;
			}

			coming |= !CrucibleBoard.WasThereAtStart(enemy.NameId);
		}

		if (coming && _crucibleHeldTraced.Add(profile.Pet))
		{
			DefenseTrace.Line($"crucible: {profile.Pet} held for the enemies to come - it hits their weakness, none of those here");
		}

		return coming;
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
