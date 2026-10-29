// The driver of run.sh: the runs of learned_store_model.py, played against the revision store's C# code (concept 13,
// "Stufe 1"). The old file is handled as LearnedFile handles it - read with its backup, written as the highest valid value
// merged with the file, copied as the backup after reading back -, and the checks read the revision file the store wrote.
using RotationSolver.Basic.Configuration;

return OtherConfiguration.RoundTrip() ? OtherConfiguration.RunHarness(args.Length > 0 ? int.Parse(args[0]) : 300) : 1;

namespace RotationSolver.Basic.Configuration
{
	internal partial class OtherConfiguration
	{
		const string Name = "HostileCastingAreaPotential";
		static readonly uint[] Ids = [1, 2, 3];

		sealed class Truth
		{
			public Dictionary<uint, List<float>> Values = [];
			public Dictionary<uint, int> Stored = new() { [1] = 0, [2] = 0, [3] = 0 };
			public Dictionary<uint, int> BroughtIn = new() { [1] = 0, [2] = 0, [3] = 0 };
			public Dictionary<(uint, string, int), int> ByRevision = [];
			public bool CountsLost;
			public float? Highest(uint id, IEnumerable<float> extra)
			{
				var all = (Values.TryGetValue(id, out var v) ? v : []).Concat(extra).ToList();
				return all.Count > 0 ? all.Max() : null;
			}
		}

		static string P(string suffix = "") => Path.Combine(Harness.Dir, Name + suffix);

		static Dictionary<uint, float> LoadOld()
		{
			var path = P(".json");
			var existed = File.Exists(path);
			var value = ReadFile<Dictionary<uint, float>>(path);
			if (value == null && existed && !File.Exists(path))
			{
				value = ReadFile<Dictionary<uint, float>>(path + ".bak");
			}
			return value ?? [];
		}

		static void SaveOld(Dictionary<uint, float> snapshot)
		{
			var path = P(".json");
			Dictionary<uint, float>? disk = null;
			if (File.Exists(path))
			{
				try { disk = JsonConvert.DeserializeObject<Dictionary<uint, float>>(File.ReadAllText(path)); }
				catch { return; }
				if (disk == null) return;
			}
			foreach (var (id, v) in disk ?? [])
			{
				if (!IsValidAreaShare(v)) continue;
				snapshot[id] = snapshot.TryGetValue(id, out var m) ? Math.Max(m, v) : v;
			}
			WriteFile(path, JsonConvert.SerializeObject(snapshot, Formatting.Indented, SerializerSettings));
			var back = JsonConvert.DeserializeObject<Dictionary<uint, float>>(File.ReadAllText(path));
			if (back?.Count == snapshot.Count) File.Copy(path, path + ".bak", true);
		}

		static RevisionFile<float>? DiskRev()
		{
			var path = P(".rev.json");
			if (!File.Exists(path)) return null;
			return JsonConvert.DeserializeObject<RevisionFile<float>>(File.ReadAllText(path), DeserializerSettings);
		}

		// The tankbuster table's file through the JSON reader and back, and an invalid value as quarantine text: a field
		// the reader drops, or a value that does not come back, would lose a layer at every load.
		internal static bool RoundTrip()
		{
			var layer = new RevisionLayer<TankbusterReading>
			{
				Game = "2026.10.14.0000.0000/2026.10.14.0000.0000", Method = 1, Count = 3, Clean = 2, Low = 0.4f, High = 1.7f,
				Value = new TankbusterReading { Unmitigated = 1.7f, UnderVulnerability = 2.1f, AimedOnly = true, Span = 4.5f },
			};
			var file = new RevisionFile<TankbusterReading>
			{
				Entries = { [42] = new RevisionEntry<TankbusterReading> { Current = layer, Inherited = layer.Copy(),
					Quarantined = new RevisionQuarantine { Value = JsonConvert.SerializeObject(float.NaN, SerializerSettings), Count = 1 } } },
			};
			var text = JsonConvert.SerializeObject(file, Formatting.Indented, SerializerSettings);
			var back = JsonConvert.DeserializeObject<RevisionFile<TankbusterReading>>(text, DeserializerSettings);
			var again = JsonConvert.SerializeObject(back, Formatting.Indented, SerializerSettings);
			if (text != again)
			{
				Console.WriteLine("ROUND TRIP FAILED:\n" + text + "\n---\n" + again);
				return false;
			}

			Console.WriteLine("round trip ok: a tankbuster entry with both layers and a quarantined NaN comes back as written");
			return true;
		}

		internal static int RunHarness(int runs)
		{
			var failures = new Dictionary<string, int>();
			var firstSeed = new Dictionary<string, int>();
			for (var seed = 0; seed < runs; seed++)
			{
				foreach (var f in Run(seed))
				{
					failures[f] = failures.GetValueOrDefault(f) + 1;
					firstSeed.TryAdd(f, seed);
				}
			}
			if (failures.Count == 0)
			{
				Console.WriteLine($"{runs} runs: READ, LOST, DOUBLED, CURRENT, DELETED hold on the C# store");
				return 0;
			}
			foreach (var (f, n) in failures) Console.WriteLine($"{f}: {n} runs, first seed {firstSeed[f]}");
			return 1;
		}

		static HashSet<string> Run(int seed)
		{
			var rng = new Random(seed);
			Harness.Dir = Path.Combine(Path.GetTempPath(), "revtest", seed.ToString());
			if (Directory.Exists(Harness.Dir)) Directory.Delete(Harness.Dir, true);
			Directory.CreateDirectory(Harness.Dir);
			var found = new HashSet<string>();
			var legacy = new Dictionary<uint, float>();
			foreach (var id in Ids) if (rng.NextDouble() < 0.7) legacy[id] = (float)Math.Round(0.05 + rng.NextDouble() * 0.55, 3);
			if (rng.NextDouble() < 0.3) legacy[Ids[rng.Next(3)]] = rng.NextDouble() < 0.5 ? 9.41f : -0.2f;
			File.WriteAllText(P(".json"), JsonConvert.SerializeObject(legacy));
			var truth = NewTruth(legacy);
			int game = 1, method = 1;
			for (var s = 0; s < 14; s++)
			{
				var between = rng.NextDouble();
				if (between < 0.15) game++;
				else if (between < 0.2) method++;
				else if (between < 0.25 && File.Exists(P(".rev.json")))
				{
					File.WriteAllText(P(".rev.json"), "garbage{");
					if (rng.NextDouble() < 0.3) { File.WriteAllText(P(".rev.json.bak"), "garbage{"); truth.CountsLost = true; }
				}
				else if (between < 0.28 && File.Exists(P(".json")) && File.Exists(P(".json.bak"))
					&& File.ReadAllText(P(".json")) == File.ReadAllText(P(".json.bak")))
				{
					File.WriteAllText(P(".json"), "garbage{");
				}
				else if (between < 0.31)
				{
					File.Delete(P(".rev.json"));
					File.Delete(P(".json"));
					truth = NewTruth([]);
				}
				else if (between < 0.36)
				{
					Dictionary<uint, float> old = [];
					try { if (File.Exists(P(".json"))) old = JsonConvert.DeserializeObject<Dictionary<uint, float>>(File.ReadAllText(P(".json"))) ?? []; }
					catch { old = []; }
					old = old.Where(p => IsValidAreaShare(p.Value)).ToDictionary(p => p.Key, p => p.Value);
					for (var r = 0; r < 6; r++)
					{
						var id = Ids[rng.Next(3)];
						var v = (float)Math.Round(0.05 + rng.NextDouble() * 0.75, 3);
						if (v > old.GetValueOrDefault(id, -1f))
						{
							if (v > (truth.Highest(id, []) ?? -1f)) truth.BroughtIn[id]++;
							old[id] = v;
							truth.Values.TryAdd(id, []);
							truth.Values[id].Add(v);
						}
					}
					File.WriteAllText(P(".json"), JsonConvert.SerializeObject(old));
					continue;
				}

				var readable = rng.NextDouble();
				var knownFrom = readable < 0.8 ? 0 : readable < 0.9 ? rng.Next(1, 7) : 7;
				var finishes = rng.NextDouble() > 0.05;
				var store = new RevisedStore<float>(Name, method, Math.Max, IsValidAreaShare, share => share,
					() => HostileCastingAreaPotential, null, "area table");
				var running = $"{game}";
				Harness.Game = knownFrom == 0 ? running : null;
				HostileCastingAreaPotential = [];
				if (finishes)
				{
					HostileCastingAreaPotential = store.Load(LoadOld());
					Check(truth, [], found, "load", knownFrom == 0 ? running : null, method);
				}
				var session = new List<(uint Id, float Value, string? Running)>();
				for (var r = 0; r < 6; r++)
				{
					Harness.Game = r + 1 >= knownFrom ? running : null;
					var id = Ids[rng.Next(3)];
					float v = rng.NextDouble() < 0.08 ? new[] { float.NaN, 1.7f, -0.1f }[rng.Next(3)] : (float)Math.Round(0.05 + rng.NextDouble() * 0.75, 3);
					var clean = rng.NextDouble() > 0.2;
					store.Measure(id, v, clean);
					if (IsValidAreaShare(v)) session.Add((id, v, Harness.Game));
					if (finishes)
					{
						var layers = store.Snapshot();
						SaveOld(new Dictionary<uint, float>(HostileCastingAreaPotential));
						store.Save(layers);
						foreach (var (sid, sv, srun) in session)
						{
							truth.Values.TryAdd(sid, []);
							truth.Values[sid].Add(sv);
							truth.Stored[sid]++;
							if (srun != null) truth.ByRevision[(sid, srun, method)] = truth.ByRevision.GetValueOrDefault((sid, srun, method)) + 1;
						}
						session.Clear();
						Check(truth, [], found, "reading", Harness.Game, method);
					}
					else
					{
						store.Save(store.Snapshot());
					}
				}
			}
			return found;
		}

		static Truth NewTruth(Dictionary<uint, float> legacy)
		{
			var t = new Truth();
			foreach (var (id, v) in legacy)
			{
				t.BroughtIn[id] = 1;
				if (IsValidAreaShare(v)) t.Values[id] = [v];
			}
			return t;
		}

		static void Check(Truth truth, List<(uint, float, string?)> pending, HashSet<string> found, string where, string? running, int method)
		{
			var disk = DiskRev();
			foreach (var id in Ids)
			{
				var expected = truth.Highest(id, []);
				var seen = HostileCastingAreaPotential.TryGetValue(id, out var s) ? s : (float?)null;
				if (seen is { } sv && !IsValidAreaShare(sv)) found.Add("READ");
				if (expected != seen) found.Add(expected == null ? "DELETED" : "READ");
				if (where != "reading" || disk == null) continue;
				disk.Entries.TryGetValue(id, out var e);
				var counted = (e?.Current?.Count ?? 0) + (e?.Inherited?.Count ?? 0);
				if (!truth.CountsLost && counted < truth.Stored[id]) found.Add("LOST");
				if (counted > truth.Stored[id] + truth.BroughtIn[id]) found.Add("DOUBLED");
				if (running != null && !truth.CountsLost)
				{
					var under = truth.ByRevision.GetValueOrDefault((id, running, method));
					var cur = e?.Current;
					if (under > 0 && (cur == null || cur.Game != running || cur.Method != method || cur.Count != under)) found.Add("CURRENT");
				}
			}
		}
	}
}
