// What the revision store takes from the plugin, outside the game: the log, the trace, the file path, the JSON settings
// and the file reader and writer of OtherConfiguration, the last two as the plugin has them (ReadFile without the
// in-game warning). Harness.Game stands in for the game version.
using Newtonsoft.Json.Converters;

namespace ECommons.Logging
{
	public static class PluginLog { public static void Warning(string s) => Harness.Log.Add("WARN " + s); }
}

namespace RotationSolver.Basic.Helpers
{
	public static class DefenseTrace { public static void Line(string s) => Harness.Log.Add(s); }
}

public static class Harness
{
	public static string? Game;
	public static string Dir = "";
	public static List<string> Log = [];
}

namespace RotationSolver.Basic.Configuration
{
	internal partial class OtherConfiguration
	{
		internal static Dictionary<uint, float> HostileCastingAreaPotential = [];
		public static Dictionary<uint, TankbusterReading> TankbusterPotential = [];
		public static readonly object TankbusterPotentialGate = new();

		private static readonly RevisedStore<float> AreaRevision = new("HostileCastingAreaPotential", 1, Math.Max,
			IsValidAreaShare, share => share, () => HostileCastingAreaPotential, null, "area table");

		private static readonly RevisedStore<TankbusterReading> TankbusterRevision = new("TankbusterPotential", 1,
			TankbusterReading.Higher, TankbusterReading.IsValid, reading => reading.Unmitigated > 0f ? reading.Unmitigated : null,
			() => TankbusterPotential, TankbusterPotentialGate, "tankbuster table");

		internal static bool IsWithinWholeHealth(float share) => share <= 1f;

		internal static Task SaveList(string name) => Task.CompletedTask;

		private static string GetFilePath(string name) => Path.Combine(Harness.Dir, $"{name}.json");

		private static readonly JsonSerializerSettings SerializerSettings = new() { TypeNameHandling = TypeNameHandling.None };

		private static readonly JsonSerializerSettings DeserializerSettings = new()
		{
			TypeNameHandling = TypeNameHandling.None,
			Converters = [new StringEnumConverter()],
		};

		private static T? ReadFile<T>(string path) where T : class
		{
			if (!File.Exists(path)) return null;
			try
			{
				return JsonConvert.DeserializeObject<T>(File.ReadAllText(path), DeserializerSettings);
			}
			catch (Exception ex)
			{
				Harness.Log.Add($"Failed to load {path}: {ex.Message}");
				try { File.Move(path, path + ".corrupt", true); } catch { }
				return null;
			}
		}

		private static bool WriteFile(string path, string json)
		{
			var tempPath = path + ".tmp";
			File.WriteAllText(tempPath, json);
			File.Move(tempPath, path, true);
			return true;
		}
	}
}
