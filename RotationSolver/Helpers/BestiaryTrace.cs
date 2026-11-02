using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Agent;
using Dalamud.Game.Agent.AgentArgTypes;
using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Text.ReadOnly;
using System.Reflection;
using System.Text;

namespace RotationSolver.Helpers;

/// <summary>
/// Writes what the game does while the Master's Bestiary is open to <c>BestiaryTrace.log</c> in the plugin's config
/// folder: every event an agent receives, every click on the Bestiary and the context menu, every context menu with its
/// entries, and the three battlehorn slots as they change (concept 01, "Aufzeichnung des Bestienbuchs", A488).
/// </summary>
/// <remarks>
/// The owner's request of 02.11.2026: record how he assigns familiars to the battlehorns, starting when the Bestiary
/// opens and stopping when it closes. Daedalus assigns them through the Bestiary's own callbacks out of combat; whether
/// the same values and menu run on his German client, and whether the game allows it in combat, only his client can
/// show. The recorder only listens - it fires no callback and changes nothing.
/// </remarks>
internal static class BestiaryTrace
{
	private const string Bestiary = "XBMMonsterNotebook";
	private const string ContextMenu = "ContextMenu";
	private static readonly string[] Watched = [Bestiary, ContextMenu];

	private static readonly object _lock = new();
	private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);
	private static StreamWriter? _writer;
	// The listeners of an open Bestiary are registered: kept apart from the writer, which a failed write closes early.
	private static bool _listening;
	private static string _horns = string.Empty;
	private static bool _inCombat;

	/// <summary>The trace file's full path.</summary>
	public static string FilePath => Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "BestiaryTrace.log");

	/// <summary>Listens for the Bestiary to open and close; nothing is written until it opens.</summary>
	public static void Enable()
	{
		Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, Bestiary, OnBestiaryOpened);
		Svc.AddonLifecycle.RegisterListener(AddonEvent.PostShow, Bestiary, OnBestiaryOpened);
		Svc.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, Bestiary, OnBestiaryClosed);
		Svc.AddonLifecycle.RegisterListener(AddonEvent.PostHide, Bestiary, OnBestiaryClosed);
	}

	public static void Disable()
	{
		Svc.AddonLifecycle.UnregisterListener(OnBestiaryOpened, OnBestiaryClosed);
		Stop("plugin unloaded");
	}

	private static void OnBestiaryOpened(AddonEvent type, AddonArgs args)
	{
		lock (_lock)
		{
			if (_listening)
			{
				return;
			}

			try
			{
				_writer = new StreamWriter(FilePath, true, Utf8) { AutoFlush = true };
			}
			catch (Exception ex)
			{
				_writer = null;
				PluginLog.Warning($"Could not open the bestiary trace: {ex.Message}");
				return;
			}
		}

		_horns = Horns();
		_inCombat = Svc.Condition[ConditionFlag.InCombat];
		Write(string.Empty);
		Write($"==== {DateTime.Now:yyyy-MM-dd HH:mm:ss} bestiary opened ({type}) | version {Version()} | commit {SourceCommit()}"
			+ $" | level {Player.Level} | territory {Svc.ClientState.TerritoryType}");
		Line($"horns {_horns}");

		Svc.AddonLifecycle.RegisterListener(AddonEvent.PreReceiveEvent, Watched, OnAddonClick);
		Svc.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, ContextMenu, OnMenu);
		Svc.AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, ContextMenu, OnMenu);
		Svc.AgentLifecycle.RegisterListener(AgentEvent.PreReceiveEvent, OnAgentEvent);
		Svc.Framework.Update += OnFrame;
		_listening = true;
	}

	private static void OnBestiaryClosed(AddonEvent type, AddonArgs args)
	{
		Stop($"bestiary closed ({type})");
	}

	private static void Stop(string why)
	{
		if (!_listening)
		{
			return;
		}

		_listening = false;
		Svc.Framework.Update -= OnFrame;
		Svc.AgentLifecycle.UnregisterListener(OnAgentEvent);
		Svc.AddonLifecycle.UnregisterListener(OnAddonClick, OnMenu);
		Line($"horns {Horns()}");
		Write($"==== {DateTime.Now:yyyy-MM-dd HH:mm:ss} {why}");

		lock (_lock)
		{
			try
			{
				_writer?.Dispose();
			}
			catch (Exception ex)
			{
				PluginLog.Warning($"Could not close the bestiary trace: {ex.Message}");
			}

			_writer = null;
		}
	}

	// The assignment goes through the server; the slots change about a second after the menu entry is picked.
	private static void OnFrame(Dalamud.Plugin.Services.IFramework framework)
	{
		var combat = Svc.Condition[ConditionFlag.InCombat];
		if (combat != _inCombat)
		{
			_inCombat = combat;
			Line(combat ? "combat started" : "combat ended");
		}

		var horns = Horns();
		if (horns != _horns)
		{
			Line($"horns changed: {_horns} -> {horns}");
			_horns = horns;
		}
	}

	// The callback a click in the Bestiary or the menu sends arrives here as the values the agent receives.
	private static unsafe void OnAgentEvent(AgentEvent type, AgentArgs args)
	{
		if (args is not AgentReceiveEventArgs receive)
		{
			return;
		}

		Line($"agent {args.AgentId} event kind {receive.EventKind}: {Values((AtkValue*)receive.AtkValues, receive.ValueCount)}");
	}

	private static void OnAddonClick(AddonEvent type, AddonArgs args)
	{
		if (args is AddonReceiveEventArgs click)
		{
			Line($"click {args.AddonName}: {click.AtkEventType} param {click.EventParam}");
		}
	}

	private static unsafe void OnMenu(AddonEvent type, AddonArgs args)
	{
		var (count, values) = args switch
		{
			AddonSetupArgs setup => (setup.AtkValueCount, setup.AtkValues),
			AddonRefreshArgs refresh => (refresh.AtkValueCount, refresh.AtkValues),
			_ => (0u, nint.Zero),
		};
		Line($"menu {args.AddonName} {type}: {Values((AtkValue*)values, count)}");
	}

	private static string Horns()
	{
		try
		{
			var pets = BeastmasterRotation.CurrentPets;
			var text = new StringBuilder();
			for (var i = 0; i < pets.Length; i++)
			{
				text.Append(i == 0 ? "[" : ", ").Append(pets[i]).Append(' ').Append((BeastmasterPet)pets[i]);
			}

			return text.Append(']').ToString();
		}
		catch (Exception ex)
		{
			return $"unreadable ({ex.GetType().Name})";
		}
	}

	private static unsafe string Values(AtkValue* values, uint count)
	{
		if (values == null || count == 0)
		{
			return "no values";
		}

		var text = new StringBuilder();
		for (var i = 0; i < count; i++)
		{
			var value = values[i];
			text.Append(i == 0 ? string.Empty : " | ").Append(i).Append(' ');
			_ = value.Type switch
			{
				AtkValueType.Int => text.Append("int ").Append(value.Int),
				AtkValueType.UInt => text.Append("uint ").Append(value.UInt),
				AtkValueType.Bool => text.Append("bool ").Append(value.Bool),
				AtkValueType.Float => text.Append("float ").Append(value.Float),
				AtkValueType.Int64 => text.Append("int64 ").Append(value.Int64),
				AtkValueType.UInt64 => text.Append("uint64 ").Append(value.UInt64),
				AtkValueType.String or AtkValueType.ConstString or AtkValueType.ManagedString => text.Append("text \"")
					.Append(value.String.HasValue ? new ReadOnlySeStringSpan(value.String.AsSpan()).ExtractText() : string.Empty)
					.Append('"'),
				_ => text.Append(value.Type),
			};
		}

		return text.ToString();
	}

	private static void Line(string text)
	{
		Write($"{DateTime.Now:HH:mm:ss.fff} {(_inCombat ? "combat" : "peace")} {text}");
	}

	private static void Write(string text)
	{
		lock (_lock)
		{
			if (_writer == null)
			{
				return;
			}

			try
			{
				_writer.WriteLine(text);
			}
			catch (Exception ex)
			{
				PluginLog.Warning($"Could not write the bestiary trace: {ex.Message}");
				try
				{
					_writer.Dispose();
				}
				catch
				{
					// A writer whose write failed fails to close as well; the handle is released either way.
				}

				_writer = null;
			}
		}
	}

	private static string Version() => typeof(BestiaryTrace).Assembly.GetName().Version?.ToString() ?? "unknown";

	private static string SourceCommit()
	{
		foreach (var attribute in typeof(BestiaryTrace).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
		{
			if (attribute.Key == "SourceCommit")
			{
				return attribute.Value ?? "unknown";
			}
		}

		return "unknown";
	}
}
