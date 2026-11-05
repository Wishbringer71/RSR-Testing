using ECommons.DalamudServices;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// How much of the player's own healing reaches a member who carries a status that lowers HP recovery, measured in play,
/// and the deficit healing can still close on him (concept 07, "Heilung, die nur geschwächt ankommt", A544).
/// </summary>
/// <remarks>
/// <para>The status sheet names over thirty such statuses ("HP recovery is reduced", "reducing HP recovery", "nullified"),
/// most without a figure. Their figure is measured on the player's own area heals: what reached a bearer against the
/// smallest of what the same heal brought, in the same set, to the members carrying none of them. Unmeasured, the full heal
/// is assumed - healing as before.</para>
///
/// <para>Read only where the area heal decides whether the party needs one: a member healing barely reaches is no reason
/// to cast it. A single heal, the emergency heal and the critical class keep the true deficit - a member healing reaches
/// less needs more of it, not later.</para>
/// </remarks>
internal static class HealReduction
{
	private static HashSet<uint>? _reducing;
	private static readonly Dictionary<uint, (double Sum, int Count)> _measured = [];
	private static readonly HashSet<uint> _traced = [];
	private static readonly List<uint> _amounts = [];
	private static readonly List<(IBattleChara Member, uint Amount)> _bearers = [];

	/// <summary>The statuses whose text says HP recovery is lowered or nullified, read once from the status sheet.</summary>
	internal static HashSet<uint> ReducingStatuses => _reducing ??= BuildReducing();

	private static HashSet<uint> BuildReducing()
	{
		var result = new HashSet<uint>();
		// The English sheet whatever the client's language: the phrases below are English, and a German client read its own
		// texts, in which none of them stands - no status was found, and the area heals counted every bearer in full (A547).
		var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Status>(Dalamud.Game.ClientLanguage.English);
		if (sheet == null)
		{
			return result;
		}

		foreach (var row in sheet)
		{
			var text = row.Description.ExtractText();
			if (text.Length == 0)
			{
				continue;
			}

			var lower = text.ToLowerInvariant();
			if (lower.Contains("hp recovery") && !lower.Contains("increased")
				&& (lower.Contains("reduc") || lower.Contains("nullif")))
			{
				_ = result.Add(row.RowId);
			}
		}

		return result;
	}

	/// <summary>
	/// The share of the player's healing that reaches <paramref name="member"/>: the smallest measured share over the
	/// lowering statuses he carries, one when none is measured or he carries none.
	/// </summary>
	internal static float Factor(IBattleChara member)
	{
		if (member == null || _measured.Count == 0)
		{
			return 1f;
		}

		var factor = 1f;
		foreach (var status in member.StatusList)
		{
			if (status.StatusId != 0 && _measured.TryGetValue(status.StatusId, out var m) && m.Count > 0)
			{
				factor = Math.Min(factor, (float)(m.Sum / m.Count));
			}
		}

		return factor;
	}

	/// <summary>
	/// The health ratio <paramref name="member"/> counts with for an area heal: the deficit healing can close on him,
	/// <c>1 - (1 - ratio) * factor</c>; the ratio itself while his factor is one.
	/// </summary>
	internal static float Restorable(IBattleChara member, float ratio)
	{
		var factor = Factor(member);
		return factor >= 1f ? ratio : 1f - ((1f - ratio) * factor);
	}

	/// <summary>
	/// The player's own heal reached several members: the share that reached each bearer of a lowering status against the
	/// smallest heal on the members carrying none, into that status's figure. Effect handler, game thread.
	/// </summary>
	internal static void RecordOwnHeal(IReadOnlyList<(ulong Target, uint Amount)> heals)
	{
		var reducing = ReducingStatuses;
		_amounts.Clear();
		_bearers.Clear();
		foreach (var (target, amount) in heals)
		{
			if (Svc.Objects.SearchById(target) is not IBattleChara member)
			{
				continue;
			}

			var bears = false;
			foreach (var status in member.StatusList)
			{
				if (status.StatusId != 0 && reducing.Contains(status.StatusId))
				{
					bears = true;
					break;
				}
			}

			if (bears)
			{
				_bearers.Add((member, amount));
			}
			else if (amount > 0)
			{
				_amounts.Add(amount);
			}
		}

		if (_bearers.Count == 0 || _amounts.Count == 0)
		{
			return;
		}

		// The smallest heal on a member without such a status: a critical heal there would make the bearers' share look
		// smaller than it is, the smallest makes it look larger - the side on which area heals go as before.
		var baseline = _amounts[0];
		foreach (var amount in _amounts)
		{
			baseline = Math.Min(baseline, amount);
		}

		foreach (var (member, amount) in _bearers)
		{
			var share = Math.Min(1.0, amount / (double)baseline);
			foreach (var status in member.StatusList)
			{
				if (status.StatusId == 0 || !reducing.Contains(status.StatusId))
				{
					continue;
				}

				var m = _measured.TryGetValue(status.StatusId, out var known) ? known : (0.0, 0);
				m = (m.Item1 + share, m.Item2 + 1);
				_measured[status.StatusId] = m;
				Trace(status.StatusId, (float)(m.Item1 / m.Item2), member);
			}
		}
	}

	// A line when a status is first measured in the session.
	private static void Trace(uint status, float factor, IBattleChara member)
	{
		if (!_traced.Add(status))
		{
			return;
		}

		var name = Service.GetSheet<Lumina.Excel.Sheets.Status>()?.GetRowOrDefault(status) is { } row
			? row.Name.ExtractText()
			: string.Empty;
		DefenseTrace.Line($"heal reduction: {name} #{status} on {member.Name.TextValue} lets {factor:P0} of own heals through"
			+ " - area heals count only the deficit healing can close on its bearers");
	}
}
