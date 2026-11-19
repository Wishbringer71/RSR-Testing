using ECommons.SplatoonAPI;
using System.Collections;
using System.Reflection;

namespace RotationSolver.Engine.Helpers;

/// <summary>
/// Concept 13, "Flächenanzeige: eine richtige Anzeige je Wirken" (A921): Splatoon's projection settings. Splatoon offers no
/// interface for them, so they are reached through the plugin object ECommons' <see cref="Splatoon"/> connects to. All of it
/// fails closed: settings that cannot be read count as "projection off, nothing blocked", and nothing is written.
/// Runs on the framework thread, like Splatoon's own projection, so the lists never change under its loop.
/// </summary>
internal static class SplatoonProjection
{
	private const BindingFlags Members = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

	// Splatoon writes a ring's radii into the omen's path as "sircle_" and two pairs of digits (Projection.DetermineDonutRange).
	private const string RingKey = "sircle_";
	private const string RingDigits = "00";

	private static int _readForVersion = -1;
	private static FieldInfo? _config, _enable, _actions, _duties, _whitelist;
	private static FieldInfo? _entryAction, _entryTerritory, _entryDataId, _entryNameId, _entryModelId;
	private static Type? _entryType;
	private static float _raidwideSize = float.MaxValue;
	private static bool _tracedUnreadable;
	private static readonly Dictionary<uint, (float Inner, float Outer)?> _misreadRings = [];

	/// <summary>Circles of this reach or more Splatoon's projection leaves out as raidwides; <see cref="float.MaxValue"/> unread.</summary>
	internal static float RaidwideSize => Bind() ? _raidwideSize : float.MaxValue;

	/// <summary>Whether Splatoon's projection is on and draws in <paramref name="territory"/>.</summary>
	internal static bool DrawsIn(uint territory)
	{
		if (Config() is not { } config || _enable?.GetValue(config) is not bool enabled || !enabled)
		{
			return false;
		}

		var whitelist = _whitelist?.GetValue(config) is bool w && w;
		var listed = _duties?.GetValue(config) is ICollection<uint> duties && duties.Contains(territory);
		return listed == whitelist;
	}

	/// <summary>Whether the action stands in Splatoon's projection blacklist, his one list of what he does not want drawn.</summary>
	internal static bool Blacklisted(uint actionId)
	{
		if (Config() is not { } config || _actions?.GetValue(config) is not IList list || _entryAction == null)
		{
			return false;
		}

		for (var i = 0; i < list.Count; i++)
		{
			if (list[i] is { } entry && _entryAction.GetValue(entry) is uint id && id == actionId)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// The ring Splatoon draws for <paramref name="act"/> when it contradicts the game: Splatoon reads the omen's digits as
	/// yalms, the game scales the omen to the action's reach (BossModReborn's module code for six rings, A921). Null when
	/// Splatoon draws no ring for it or draws it with the action's reach.
	/// </summary>
	internal static (float Inner, float Outer)? MisreadRing(Lumina.Excel.Sheets.Action act)
	{
		if (_misreadRings.TryGetValue(act.RowId, out var known))
		{
			return known;
		}

		(float Inner, float Outer)? ring = null;
		if ((CastType)act.CastType == CastType.Donut && act.Omen.ValueNullable?.Path.ExtractText() is { Length: > 0 } path)
		{
			var width = RingDigits.Length;
			for (var at = path.IndexOf(RingKey, StringComparison.Ordinal); at >= 0; at = path.IndexOf(RingKey, at + 1, StringComparison.Ordinal))
			{
				var start = at + RingKey.Length;
				if (start + width + width <= path.Length
					&& int.TryParse(path.AsSpan(start, width), System.Globalization.NumberStyles.None, null, out var outer)
					&& int.TryParse(path.AsSpan(start + width, width), System.Globalization.NumberStyles.None, null, out var inner))
				{
					// Splatoon draws the band between them only while it has a width.
					if (outer > inner && outer != act.EffectRange)
					{
						ring = (inner, outer);
					}

					break;
				}
			}
		}

		_misreadRings[act.RowId] = ring;
		return ring;
	}

	/// <summary>
	/// Enters the cast's action into Splatoon's projection blacklist, as Splatoon's own button does. False when it stands
	/// there already or the settings cannot be written.
	/// </summary>
	internal static unsafe bool Block(IBattleChara caster, uint actionId, uint territory)
	{
		if (Config() is not { } config || _actions?.GetValue(config) is not IList list || _entryType == null || Blacklisted(actionId))
		{
			return false;
		}

		try
		{
			var entry = Activator.CreateInstance(_entryType);
			if (entry == null)
			{
				return false;
			}

			_entryAction?.SetValue(entry, actionId);
			_entryTerritory?.SetValue(entry, territory);
			_entryDataId?.SetValue(entry, caster.BaseId);
			_entryNameId?.SetValue(entry, caster.NameId);
			if (caster.Address != nint.Zero)
			{
				_entryModelId?.SetValue(entry, (uint)((FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)caster.Address)->ModelContainer.ModelCharaId);
			}

			_ = list.Add(entry);
			return true;
		}
		catch (Exception ex)
		{
			TraceUnreadable(ex.Message);
			return false;
		}
	}

	private static object? Config()
		=> Bind() && _config != null ? _config.GetValue(Splatoon.Instance) : null;

	// Reads the members once per connection to Splatoon; a reconnect may bring another build.
	private static bool Bind()
	{
		var plugin = Splatoon.Instance;
		if (plugin == null)
		{
			return false;
		}

		if (_readForVersion == Splatoon.Version)
		{
			return _config != null;
		}

		_readForVersion = Splatoon.Version;
		_config = null;
		try
		{
			var config = plugin.GetType().GetField("Config", Members);
			var configType = config?.FieldType;
			_enable = configType?.GetField("EnableProjection", Members);
			_actions = configType?.GetField("ProjectionBlacklistedActions", Members);
			_duties = configType?.GetField("ProjectionBlacklistedDuties", Members);
			_whitelist = configType?.GetField("IsProjectionBlacklistWhitelist", Members);
			_entryType = _actions?.FieldType.IsGenericType == true ? _actions.FieldType.GetGenericArguments()[0] : null;
			_entryAction = _entryType?.GetField("Action", Members);
			_entryTerritory = _entryType?.GetField("Territory", Members);
			_entryDataId = _entryType?.GetField("DataId", Members);
			_entryNameId = _entryType?.GetField("NameId", Members);
			_entryModelId = _entryType?.GetField("ModelId", Members);
			_raidwideSize = plugin.GetType().Assembly.GetType("Splatoon.Services.Projection")
				?.GetField("RaidwideSize", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) is float size
				? size
				: float.MaxValue;

			if (config == null || _enable == null || _actions == null || _entryAction == null)
			{
				TraceUnreadable("a setting is missing");
				return false;
			}

			_config = config;
			return true;
		}
		catch (Exception ex)
		{
			TraceUnreadable(ex.Message);
			return false;
		}
	}

	private static void TraceUnreadable(string why)
	{
		if (_tracedUnreadable)
		{
			return;
		}

		_tracedUnreadable = true;
		DefenseTrace.Line($"area display: Splatoon's projection settings are not readable ({why}) - none is blocked, none is left to it");
	}
}
