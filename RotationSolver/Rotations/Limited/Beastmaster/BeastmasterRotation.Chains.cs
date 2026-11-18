using System.Globalization;
using System.Text.RegularExpressions;

namespace RotationSolver.Rotations.Limited.Beastmaster;

/// <summary>
/// Concept 01, "Ketten: Rally und Rallying Cheer tragen die Kette weiter" (A886): what a chain of instinctual skills can
/// draw on - the TP a skill needs, the TP Rally and Rallying Cheer give, and how long the chain still stands.
/// </summary>
public partial class BeastmasterRotation
{
	// "Standard TP Gauge Increase: 40 | Each stack of Mastered Instinct increases TP Gauge by 70" (Rally, 44905);
	// "Standard Familiar TP Gauge Increase: 30 | Each stack of Natural Instinct increases Familiar TP Gauge by 70"
	// (Rallying Cheer, 44904). Read from the running version's effect text: the two differ in their standard figure.
	private static readonly Regex StandardRestore = new(@"Standard (?:Familiar )?TP Gauge Increase: (?<value>\d+)", RegexOptions.Compiled);
	private static readonly Regex StackRestore = new(@"increases (?:Familiar )?TP Gauge by (?<value>\d+)", RegexOptions.Compiled);
	private static readonly Dictionary<(ActionID Action, int Level), (int Standard, int Each)> _restores = [];

	/// <summary>
	/// TP that <paramref name="action"/> (Rally or Rallying Cheer) gives with <paramref name="stacks"/> stacks, as its
	/// effect text states it at the synced level; the upstream formula where the text gives no figure.
	/// </summary>
	internal static int RestoredByStacks(ActionID action, int stacks)
	{
		var key = (action, DataCenter.PlayerSyncedLevel());
		if (!_restores.TryGetValue(key, out var restore))
		{
			var text = ActionTooltip.Text((uint)action);
			var standard = StandardRestore.Match(text);
			var each = StackRestore.Match(text);
			restore = standard.Success && each.Success
				? (int.Parse(standard.Groups["value"].Value, CultureInfo.InvariantCulture), int.Parse(each.Groups["value"].Value, CultureInfo.InvariantCulture))
				: (TPRestoredByStacks(0), TPRestoredByStacks(1) - TPRestoredByStacks(0));
			_restores[key] = restore;
		}

		return restore.Standard + (stacks * restore.Each);
	}

	/// <summary>The least TP an axe spends, from its cost in the action sheet (Avalanche Axe).</summary>
	internal static int AxeTPNeed => _axeTPNeed ??= CostOf(ActionID.AvalancheAxePvE);

	/// <summary>The least familiar TP Trick spends, from its cost in the action sheet.</summary>
	internal static int TrickTPNeed => _trickTPNeed ??= CostOf(ActionID.TrickPvE);

	/// <summary>
	/// A full gauge: the TP the finishers spend (Brutal Rage, "TP Gauge Cost: 250"), where the axes reach their highest
	/// potency (1,000) and Trick its (1,250) - a refill beyond it is lost.
	/// </summary>
	internal static int GaugeFullTP => _gaugeFullTP ??= CostOf(ActionID.BrutalRagePvE);

	private static int? _gaugeFullTP;
	private static int? _axeTPNeed;
	private static int? _trickTPNeed;

	private static int CostOf(ActionID action)
	{
		return Service.GetSheet<Lumina.Excel.Sheets.Action>()?.GetRowOrDefault((uint)action) is { } row ? row.PrimaryCostValue : 0;
	}

	/// <summary>
	/// Seconds the chain still stands: the heart of the last instinctual skill, Sunstrider or Moonstalker - the next
	/// instinctual skill in that time completes another combo. 0 without a chain.
	/// </summary>
	internal static float ChainTimeLeft => StatusHelper.PlayerStatusTime(false, StatusID.VolantHeart, StatusID.RampantHeart,
		StatusID.DurantHeart, StatusID.EldritchHeart, StatusID.Sunstrider, StatusID.Moonstalker);
}
