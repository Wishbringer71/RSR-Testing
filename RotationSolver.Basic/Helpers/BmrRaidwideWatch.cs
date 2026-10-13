using System.Collections.Concurrent;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// Whether BossModReborn's raidwide predictions land, module by module, as the fight shows it.
/// </summary>
/// <remarks>
/// <para>The owner's rule for every safety measure: it pays where a hit is announced or likely, and each one
/// names how it measures that likelihood (concept 08). A raidwide prediction named none - every one inside the
/// mitigation window opened the area defence. His trace of 04-10 October measured what that costs: of 175
/// predictions that opened it on their own, 27 hit him at the predicted time; 144 defensive actions went to
/// predictions that nothing followed, and a Temperance spent on one was gone when a 27 % raidwide came 25 s
/// later (A272).</para>
///
/// <para>Each prediction that comes inside the window is held against the fight: did an enemy action reach more
/// than one party member, or the player and someone else, within the same window around the predicted moment?
/// The window is the user's own <c>BMRRaidwideMitWindow</c>: a mitigation placed that early covers a hit that
/// late, and a prediction further off than that would have placed it wrong anyway.</para>
///
/// <para>The verdict follows the one the predicted-mitigation hold already keeps for itself
/// (<see cref="DataCenter.ProactiveHoldIsEarningItsKeep"/>): it starts trusting, because a rule that never acts
/// cannot learn, and it trusts a module as long as its predictions landed at least as often as they missed - a
/// simple majority, the break-even of spending a cooldown on a prediction. Below it, the module's raidwide
/// predictions are set aside until one lands again; the scoring goes on, so it recovers by itself. The score is
/// kept per module rather than per fight: a module is one encounter, so the pattern carried over is the same
/// boss's, which is what the hold's per-fight reset protects against.</para>
///
/// <para>Recognition only. Whether a module below parity is set aside is the decision of
/// <c>SetAsideMissingBmrRaidwides</c>, off by default: replayed over the same trace, setting aside below parity
/// also took the defence ahead of eleven raidwides of 15 % and more that only BossModReborn had announced, and
/// that trade is the owner's. The score and its trace line run either way, with the BossModReborn endpoint the
/// prediction came from. What it cannot see: a raidwide whose damage comes from an actor that is neither a
/// targetable enemy nor a combatant counts as missed.</para>
/// </remarks>
internal static class BmrRaidwideWatch
{
	private sealed class Score
	{
		public int Landed;
		public int Missed;
	}

	private sealed class Prediction
	{
		public required string Module { get; init; }
		public required string Source { get; set; }
		public DateTime Activation { get; set; }
	}

	private static readonly ConcurrentDictionary<string, Score> _scores = new();
	private static readonly List<Prediction> _open = [];
	private static readonly List<DateTime> _partyHits = [];
	private static readonly object _lock = new();

	/// <summary>
	/// Records an enemy action that reached more than one party member, or the player and someone else.
	/// Called by the effect handler.
	/// </summary>
	internal static void RecordPartyHit()
	{
		lock (_lock)
		{
			_partyHits.Add(DateTime.Now);
		}
	}

	/// <summary>
	/// Whether the raidwide predictions of <paramref name="module"/> are to be acted on.
	/// </summary>
	/// <param name="module">The active module's name; null or empty for a prediction without one.</param>
	/// <returns>True until the module's predictions have missed more often than they landed.</returns>
	internal static bool ModuleLands(string? module)
		=> !_scores.TryGetValue(module ?? string.Empty, out var score) || score.Landed >= score.Missed;

	/// <summary>The module's record as landed and missed, for the trace.</summary>
	/// <param name="module">The active module's name.</param>
	/// <returns>Landed and missed predictions.</returns>
	internal static (int Landed, int Missed) Record(string? module)
		=> _scores.TryGetValue(module ?? string.Empty, out var score) ? (score.Landed, score.Missed) : (0, 0);

	/// <summary>
	/// Opens a watch for each raidwide prediction that enters the mitigation window and settles the ones whose
	/// window has passed. Called once per framework cycle, on the game thread.
	/// </summary>
	internal static void Update()
	{
		var window = TimeSpan.FromSeconds(Service.Config.BMRRaidwideMitWindow);
		var now = DateTime.Now;

		lock (_lock)
		{
			for (var i = _open.Count - 1; i >= 0; i--)
			{
				if (now > _open[i].Activation + window)
				{
					Settle(_open[i], window);
					_open.RemoveAt(i);
				}
			}

			if (!DataCenter.InCombat)
			{
				// A prediction the fight ended before is inconclusive, not missed.
				_open.Clear();
				_partyHits.Clear();
				return;
			}

			var raw = DataCenter.BMRNextRaidwideInRaw;
			if (raw is > 0f and < float.MaxValue && raw <= Service.Config.BMRRaidwideMitWindow)
			{
				var module = DataCenter.BMRActiveModuleName ?? string.Empty;
				var activation = now.AddSeconds(raw);
				Prediction? same = null;
				foreach (var open in _open)
				{
					if (open.Module == module && (activation - open.Activation).Duration() <= window)
					{
						same = open;
						break;
					}
				}

				var source = DataCenter.BMRNextRaidwideSource;
				if (same != null)
				{
					// The same event, its moment refined as it comes closer.
					same.Activation = activation;
					same.Source = source;
				}
				else
				{
					_open.Add(new Prediction { Module = module, Source = source, Activation = activation });
				}
			}

			// Hits older than any open window can settle nothing any more.
			var keepFrom = now - window - window;
			_partyHits.RemoveAll(t => t < keepFrom);
		}
	}

	private static void Settle(Prediction prediction, TimeSpan window)
	{
		var landed = false;
		foreach (var hit in _partyHits)
		{
			if ((hit - prediction.Activation).Duration() <= window)
			{
				landed = true;
				break;
			}
		}

		var score = _scores.GetOrAdd(prediction.Module, _ => new Score());
		if (landed)
		{
			score.Landed++;
		}
		else
		{
			score.Missed++;
		}

		var verdict = score.Landed >= score.Missed
			? "acted on"
			: Service.Config.SetAsideMissingBmrRaidwides ? "set aside" : "below parity, acted on (setting off)";
		DefenseTrace.Line($"BMR raidwide predicted for {prediction.Activation:HH:mm:ss.f}"
			+ $" ({(prediction.Module.Length == 0 ? "no module" : prediction.Module)}, {prediction.Source}):"
			+ $" {(landed ? "landed" : "nothing landed")}; landed {score.Landed} of {score.Landed + score.Missed} - {verdict}");
	}
}
