// The locks of stage 4 (concept 13, "Stufe 4", 4b and 4c) against an oracle that knows only the history of observations:
// random runs of resistances, landed sleeps, tries, sleeps that did not stop a cast, stopped casts, landings of a status
// that damages every action with and without it, patches, unreadable versions, and saves and loads. The code under test is
// LearnedLocks.cs as the plugin has it; the oracle restates concept 13 in its own words. Assumptions of the test, not
// claims about the game: three kinds, two actions, versions only move forward.
using RotationSolver.Basic.Configuration;

static class LockHarness
{
	sealed class KindHistory
	{
		public int Count;              // resistances in the file
		public bool Refuted;
		public int RefutedAt;          // the file's count when refuted
		public string? Confirmed;      // version of the latest own resistance, "" when unreadable
		public List<string> Tried = [];
	}

	sealed class LimitHistory
	{
		public int? File;              // the file's limit, the lowest ever stored
		public bool Refuted;
		public int Floor;
		public int? Learned;           // the limit learned by own resistances since the latest refutation
		public string? LearnedIn;
		public List<string> Tried = [];
	}

	sealed class ActionHistory
	{
		public bool Unstoppable;
		public bool EverStopped;
		public string? Confirmed;
		public List<string> Tried = [];
	}

	sealed class PenaltyHistory
	{
		public bool Learned;
		public bool PreStage4;
		public bool Refuted;
		public string? Version;
		public int With, Without, EarlierWith;
		public bool Started;
	}

	static readonly uint[] Kinds = [1, 2, 3];
	static readonly uint[] Actions = [10, 11];

	public static int Run(int runs)
	{
		var failures = new Dictionary<string, int>();
		var first = new Dictionary<string, int>();
		for (var seed = 0; seed < runs; seed++)
		{
			foreach (var failure in OneRun(seed))
			{
				failures[failure] = failures.GetValueOrDefault(failure) + 1;
				first.TryAdd(failure, seed);
			}
		}

		if (failures.Count == 0)
		{
			Console.WriteLine($"{runs} lock runs: KIND, LIMIT, ACTION, ONE TRY, UNREADABLE, PENALTY, ROUND TRIP hold on the C# locks");
			return 0;
		}

		foreach (var (f, n) in failures) Console.WriteLine($"{f}: {n} lock runs, first seed {first[f]}");
		return 1;
	}

	static IEnumerable<string> OneRun(int seed)
	{
		var rng = new Random(seed);
		var found = new HashSet<string>();
		var resistant = new Dictionary<uint, int>();
		var diminishing = new Dictionary<uint, int>();
		var marks = new SleepLockMarks();
		var sightings = new Dictionary<uint, PenaltySighting>();
		var kinds = Kinds.ToDictionary(k => k, _ => new KindHistory());
		var limits = Kinds.ToDictionary(k => k, _ => new LimitHistory());
		var actions = Actions.ToDictionary(a => a, _ => new ActionHistory());
		var penalties = Actions.ToDictionary(a => a, _ => new PenaltyHistory());
		var version = 1;
		string? game = "v1";

		// What the files held before stage 4: locks without marks.
		foreach (var k in Kinds)
		{
			if (rng.NextDouble() < 0.5) { resistant[k] = 1; kinds[k].Count = 1; }
			if (rng.NextDouble() < 0.4) { var l = rng.Next(1, 4); diminishing[k] = l; limits[k].File = l; }
		}

		foreach (var a in Actions)
		{
			if (rng.NextDouble() < 0.4) actions[a].Unstoppable = true;
			if (rng.NextDouble() < 0.5) { penalties[a].Learned = true; penalties[a].PreStage4 = true; }
		}

		for (var step = 0; step < 120; step++)
		{
			var k = Kinds[rng.Next(Kinds.Length)];
			var a = Actions[rng.Next(Actions.Length)];
			switch (rng.Next(13))
			{
				case 0:
					// A patch, or a moment the version cannot be read; versions only move forward.
					if (rng.NextDouble() < 0.3) game = null;
					else { if (game != null || rng.NextDouble() < 0.5) version++; game = $"v{version}"; }
					break;
				case 1:
					// Own sleep resisted at the first sleep in a fight: the kind is immune, counted and confirmed.
					resistant[k] = resistant.GetValueOrDefault(k) + 1;
					SleepLockRules.ConfirmKind(resistant, marks, k, game);
					kinds[k].Count++; kinds[k].Refuted = false; kinds[k].Confirmed = game ?? string.Empty;
					break;
				case 2:
					// A resistance written by another client or an older RSR: the file counts it, no mark.
					resistant[k] = resistant.GetValueOrDefault(k) + 1;
					kinds[k].Count++;
					break;
				case 3:
				{
					// A sleep landed on the kind, from any source, after some sleeps in this fight.
					var before = rng.Next(0, 4);
					var kindStood = OracleKind(kinds[k], game) != LockState.None;
					var limitBefore = OracleLimit(limits, k, game, out _);
					SleepLockRules.RefuteKind(resistant, marks, k, game);
					SleepLockRules.RefuteLimit(diminishing, marks, k, before, game);
					if (kindStood) { kinds[k].Refuted = true; kinds[k].RefutedAt = kinds[k].Count; }
					if (before > 0 && limitBefore is { } lim && before >= lim)
					{
						limits[k].Refuted = true; limits[k].Floor = before; limits[k].Learned = null;
					}

					break;
				}
				case 4:
				{
					// Own sleep resisted after some sleeps in this fight: the limit, lowest in the file and in the version.
					var slept = rng.Next(1, 5);
					diminishing[k] = diminishing.TryGetValue(k, out var was) ? Math.Min(was, slept) : slept;
					SleepLockRules.ConfirmLimit(marks, k, slept, game);
					var h = limits[k];
					h.File = h.File is { } f ? Math.Min(f, slept) : slept;
					h.Learned = !h.Refuted && h.Learned is { } l && h.LearnedIn == (game ?? string.Empty) ? Math.Min(l, slept) : slept;
					h.LearnedIn = game ?? string.Empty;
					h.Refuted = false;
					break;
				}
				case 5:
				case 6:
				{
					// A sleep pressed on the locks the rule would try: each taken-over one spends its try.
					var keys = new List<SleepLockKey>();
					if (SleepLockRules.KindState(resistant, marks, k, game) == LockState.TakenOver) keys.Add(new(SleepLockType.Kind, k));
					if (SleepLockRules.LimitFor(diminishing, marks, k, game, out var ls) != null && ls == LockState.TakenOver) keys.Add(new(SleepLockType.Limit, k));
					if (SleepLockRules.ActionState(actions[a].Unstoppable, marks, a, game) == LockState.TakenOver) keys.Add(new(SleepLockType.Action, a));
					SleepLockRules.Try(marks, keys, game);
					if (game != null)
					{
						foreach (var key in keys)
						{
							(key.Type == SleepLockType.Kind ? kinds[k].Tried : key.Type == SleepLockType.Limit ? limits[k].Tried : actions[a].Tried).Add(game);
						}
					}

					break;
				}
				case 7:
					// A cast went off although a sleep landed in time: unstoppable unless a sleep ever stopped it.
					if (!actions[a].EverStopped)
					{
						actions[a].Unstoppable = true;
						SleepLockRules.ConfirmAction(marks, a, game);
						actions[a].Confirmed = game ?? string.Empty;
					}

					break;
				case 8:
					// A sleep stopped the cast: stoppable for good.
					actions[a].EverStopped = true; actions[a].Unstoppable = false;
					break;
				case 9:
				{
					// A landing laid the status on a party member.
					var p = penalties[a];
					var learned = p.Learned;
					PenaltySightingRules.SeenWith(sightings, a, learned, game);
					p.Learned = true; p.Refuted = false;
					if (game != null)
					{
						Roll(p, game, learned);
						p.With++;
					}

					break;
				}
				case 10:
				case 11:
				{
					// A learned action hit its target without laying the status.
					var p = penalties[a];
					if (!p.Learned) break;
					PenaltySightingRules.SeenWithout(sightings, a, game);
					if (game != null && !p.Refuted)
					{
						Roll(p, game, true);
						p.Without++;
						if (p.With == 0 && p.Without >= 2 && p.Without > p.EarlierWith) p.Refuted = true;
					}

					break;
				}
				case 12:
				{
					// A save and a load: the marks and sightings as the plugin writes and reads them.
					var before = Snapshot(resistant, diminishing, marks, actions, game);
					marks = JsonConvert.DeserializeObject<SleepLockMarks>(JsonConvert.SerializeObject(marks))!.Whole();
					sightings = PenaltySightingRules.Whole(JsonConvert.DeserializeObject<Dictionary<uint, PenaltySighting>>(
						JsonConvert.SerializeObject(PenaltySightingRules.Copy(sightings))));
					if (Snapshot(resistant, diminishing, marks, actions, game) != before) found.Add("ROUND TRIP");
					break;
				}
			}

			// Every lock against the oracle, after every step.
			foreach (var kind in Kinds)
			{
				var state = SleepLockRules.KindState(resistant, marks, kind, game);
				if (state != OracleKind(kinds[kind], game)) found.Add("KIND");
				var limit = SleepLockRules.LimitFor(diminishing, marks, kind, game, out var limitState);
				var expected = OracleLimit(limits, kind, game, out var expectedState);
				if (limit != expected || (limit != null && limitState != expectedState)) found.Add("LIMIT");
				if (game == null && (state == LockState.TakenOver || limitState == LockState.TakenOver)) found.Add("UNREADABLE");
				if (game != null && kinds[kind].Tried.Contains(game) && state == LockState.TakenOver) found.Add("ONE TRY");
			}

			foreach (var action in Actions)
			{
				var state = SleepLockRules.ActionState(actions[action].Unstoppable, marks, action, game);
				if (state != OracleAction(actions[action], game)) found.Add("ACTION");
				if (PenaltySightingRules.IsRefuted(sightings, action) != penalties[action].Refuted) found.Add("PENALTY");
			}
		}

		return found;
	}

	// The counts of an earlier version move to the earlier sightings; an action learned before stage 4 counts one.
	static void Roll(PenaltyHistory p, string game, bool learned)
	{
		if (!p.Started)
		{
			p.Started = true; p.Version = game; p.EarlierWith = learned ? 1 : 0; p.With = 0; p.Without = 0;
		}
		else if (p.Version != game)
		{
			p.EarlierWith += p.With; p.With = 0; p.Without = 0; p.Version = game;
		}
	}

	static LockState Standing(bool seenHere, List<string> tried, string? game)
		=> seenHere || game == null || tried.Contains(game) ? LockState.Holds : LockState.TakenOver;

	static LockState OracleKind(KindHistory h, string? game)
	{
		if (h.Count == 0) return LockState.None;
		if (h.Refuted && h.Count <= h.RefutedAt) return LockState.None;
		return Standing(!h.Refuted && game != null && h.Confirmed == game, h.Tried, game);
	}

	static LockState OracleAction(ActionHistory h, string? game)
		=> !h.Unstoppable ? LockState.None : Standing(game != null && h.Confirmed == game, h.Tried, game);

	// A kind's own limit: learned since its latest refutation, otherwise the file's while never refuted; refuted, none
	// above the floor. Otherwise the lowest own limit of the other kinds above the floor.
	static int? OracleLimit(Dictionary<uint, LimitHistory> limits, uint kind, string? game, out LockState state)
	{
		var h = limits[kind];
		if (Own(h, game) is { } own)
		{
			state = Standing(own.Here, h.Tried, game);
			return own.Limit;
		}

		var floor = h.Refuted ? h.Floor : 0;
		int? lowest = null;
		var here = false;
		foreach (var other in Kinds)
		{
			if (other == kind || Own(limits[other], game) is not { } o || o.Limit <= floor) continue;
			if (lowest == null || o.Limit < lowest) { lowest = o.Limit; here = o.Here; }
			else if (o.Limit == lowest) here |= o.Here;
		}

		state = lowest == null ? LockState.None : Standing(here, h.Tried, game);
		return lowest;

		static (int Limit, bool Here)? Own(LimitHistory h, string? game)
		{
			if (h.Refuted) return null;
			if (h.Learned is { } l) return (l, game != null && h.LearnedIn == game);
			return h.File is { } f ? (f, false) : null;
		}
	}

	static string Snapshot(Dictionary<uint, int> resistant, Dictionary<uint, int> diminishing, SleepLockMarks marks,
		Dictionary<uint, ActionHistory> actions, string? game)
	{
		var text = new System.Text.StringBuilder();
		foreach (var k in Kinds)
		{
			text.Append(SleepLockRules.KindState(resistant, marks, k, game)).Append(SleepLockRules.LimitFor(diminishing, marks, k, game, out var s))
				.Append(s).Append(';');
		}

		foreach (var a in Actions)
		{
			text.Append(SleepLockRules.ActionState(actions[a].Unstoppable, marks, a, game)).Append(';');
		}

		return text.ToString();
	}
}
