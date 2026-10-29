using RotationSolver.Basic.Configuration;

namespace RotationSolver.Basic.Helpers;

/// <summary>
/// How often one enemy action hits the player in a row, counted the way Liturgy of the Bell counts its triggers: a hit
/// that damages him and another party member, at most one a second, within the bell's duration from the first hit.
/// Learned per action over every session (concept 08, "Glockenspiel", A349).
/// </summary>
/// <remarks>
/// Six kinds of party attacks hit him three to ten times in a row in the owner's fights of 18.-23.10.2026, none with a
/// marker; most are instant actions, which the area table does not measure at all. The bell is the one tool that gains
/// from every hit of such a series, and its question is exactly this count - so the window and the spacing are the
/// bell's own: its stated duration (<see cref="DefensiveValues.DurationOf"/>) and "the effect of this action can only be
/// triggered once per second". A series on the player alone is left out: the bell heals the party, and a run of hits
/// that only he takes would heal nobody who needs it.
/// </remarks>
internal static class HitSeries
{
	private sealed class Series
	{
		public DateTime First;
		public DateTime LastCounted;
		public int Counted;
	}

	private static readonly Dictionary<uint, Series> _open = [];

	// "The effect of this action can only be triggered once per second" (Liturgy of the Bell, job guide 7.5).
	private const double TriggerSpacingSeconds = 1;

	private static float Window => DefensiveValues.DurationOf((uint)ActionID.LiturgyOfTheBellPvE);

	/// <summary>
	/// One hit of <paramref name="actionId"/> on the player: counted when it damaged him and at least one other party
	/// member. Called by the effect handler on the game thread.
	/// </summary>
	internal static void RecordHit(uint actionId, bool damagedPlayer, bool damagedOthers)
	{
		var window = Window;
		if (!damagedPlayer || !damagedOthers || window <= 0f)
		{
			return;
		}

		var now = DateTime.Now;
		if (_open.TryGetValue(actionId, out var series) && (now - series.First).TotalSeconds <= window)
		{
			if ((now - series.LastCounted).TotalSeconds >= TriggerSpacingSeconds)
			{
				series.Counted++;
				series.LastCounted = now;
				Learn(actionId, series.Counted);
			}

			return;
		}

		// A new series; the ones whose window has passed are done and go.
		List<uint>? done = null;
		foreach (var (id, open) in _open)
		{
			if ((now - open.First).TotalSeconds > window)
			{
				(done ??= []).Add(id);
			}
		}

		if (done != null)
		{
			foreach (var id in done)
			{
				_ = _open.Remove(id);
			}
		}

		_open[actionId] = new Series { First = now, LastCounted = now, Counted = 1 };
	}

	// Only a series of more than one hit is stored, and only an increase is written.
	private static void Learn(uint actionId, int counted)
	{
		lock (OtherConfiguration.HitSeriesGate)
		{
			if (OtherConfiguration.HitSeriesOnPlayer.TryGetValue(actionId, out var known) && known >= counted)
			{
				return;
			}

			OtherConfiguration.HitSeriesOnPlayer[actionId] = counted;
		}

		_ = OtherConfiguration.SaveHitSeriesOnPlayer();
	}

	/// <summary>
	/// The triggers a Liturgy of the Bell laid now would still collect from a learned series on the player - the most
	/// of them; 0 when none runs or is announced:
	/// <list type="bullet">
	/// <item>a series running: for each action that hit him within the bell's duration, what it has been seen to reach
	/// less what has landed so far;</item>
	/// <item>a series announced: an enemy cast of an action learned to hit in series, reaching him and landing within
	/// the next GCD - what it has been seen to reach. A cast of an action whose series already runs is part of that
	/// series and counted there.</item>
	/// </list>
	/// </summary>
	/// <remarks>
	/// The announced first hit (A365): Chort's towers in the Klyteum hit the party four times, three seconds apart, each
	/// from a cast of its own (Ersticken des Fleisches, owner's log of 25.10.2026). A bell laid at the first hit collects
	/// from the three after it; laid just before, it collects from all four - one stack heals 400 with the first hit
	/// instead of 200 when the bell ends, and a weave window missed right after the first hit no longer costs the
	/// second as well (three seconds apart). The cast is read within the next GCD - a
	/// weave window comes in it, and the later the bell goes, the more of its duration is left for the rest of the
	/// series. Only learned series are read: a first sight of several casts of one action in a row does not show
	/// whether they are to be taken together or dodged one by one, and a bell before a dodged run heals nobody.
	/// </remarks>
	internal static int TriggersLeft()
	{
		var window = Window;
		if (window <= 0f)
		{
			return 0;
		}

		var now = DateTime.Now;
		var left = 0;
		lock (OtherConfiguration.HitSeriesGate)
		{
			var learnedSeries = OtherConfiguration.HitSeriesOnPlayer;
			if (learnedSeries.Count == 0)
			{
				return 0;
			}

			foreach (var (id, series) in _open)
			{
				if ((now - series.First).TotalSeconds <= window && learnedSeries.TryGetValue(id, out var learned))
				{
					left = Math.Max(left, learned - series.Counted);
				}
			}

			var casters = DataCenter.AreaCasters ?? [];
			for (var i = 0; i < casters.Count; i++)
			{
				var caster = casters[i];
				if (caster == null || !caster.IsValid() || !caster.IsCasting
					|| !learnedSeries.TryGetValue(caster.CastActionId, out var learned)
					|| (_open.TryGetValue(caster.CastActionId, out var running) && (now - running.First).TotalSeconds <= window))
				{
					continue;
				}

				var landsIn = caster.TotalCastTime - caster.CurrentCastTime;
				if (landsIn > 0f && landsIn <= DataCenter.DefaultGCDTotal && DataCenter.CastReachesPlayer(caster))
				{
					left = Math.Max(left, learned);
				}
			}
		}

		return left;
	}
}
