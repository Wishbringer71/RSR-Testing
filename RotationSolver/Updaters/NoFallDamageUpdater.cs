// The signature and the detour in this file are taken from Henchman (https://github.com/Knightmore/Henchman,
// Henchman/Tweaks/HacksUI.cs, "No Fall Damage", unchanged since 5813feb of 15.07.2026), which is licensed under
// the GNU Affero General Public License v3. This file is distributed under that licence; the rest of the
// project stays under its own (GPL-3.0 section 13 permits the combination).

using Dalamud.Hooking;
using ECommons.DalamudServices;
using ECommons.Logging;

namespace RotationSolver.Updaters;

/// <summary>
/// "No fall damage": the game's fall damage function returns zero while the setting is on.
/// </summary>
internal static class NoFallDamageUpdater
{
	// Resolves the call target of the E8 instruction: the function Henchman names "No Fall Damage", taking the
	// actor and a flag word and returning the damage.
	private const string FallDamageSignature = "E8 ?? ?? ?? ?? 48 ?? ?? ?? ?? 48 ?? ?? C6 05 ?? ?? ?? ?? ?? 48 ?? ?? ??";

	private delegate int FallDamageDelegate(nint actor, uint flags);

	private static Hook<FallDamageDelegate>? _fallDamageHook;
	private static bool _unavailableReported;

	public static void Enable()
	{
		try
		{
			_fallDamageHook = Svc.Hook.HookFromSignature<FallDamageDelegate>(FallDamageSignature, FallDamageDetour);
			PluginLog.Debug("[NoFallDamage] Fall damage hook created");
		}
		catch (Exception ex)
		{
			PluginLog.Error($"[NoFallDamage] Fall damage function not found in this game version: {ex.Message}");
		}
	}

	/// <summary>
	/// Follows the setting, once per framework cycle. Without the hook the setting switches itself off, so the
	/// checkbox shows that it cannot work in this game version.
	/// </summary>
	public static void Update()
	{
		bool wanted = Service.Config.NoFallDamage;
		if (_fallDamageHook == null)
		{
			if (wanted)
			{
				Service.Config.NoFallDamage.Value = false;
				if (!_unavailableReported)
				{
					PluginLog.Error("[NoFallDamage] Switched off: the fall damage function was not found in this game version");
					_unavailableReported = true;
				}
			}

			return;
		}

		if (wanted && !_fallDamageHook.IsEnabled)
		{
			_fallDamageHook.Enable();
		}
		else if (!wanted && _fallDamageHook.IsEnabled)
		{
			_fallDamageHook.Disable();
		}
	}

	public static void Disable()
	{
		try
		{
			_fallDamageHook?.Disable();
			_fallDamageHook?.Dispose();
			_fallDamageHook = null;
		}
		catch (Exception ex)
		{
			PluginLog.Error($"[NoFallDamage] Failed to dispose the fall damage hook: {ex}");
		}
	}

	private static int FallDamageDetour(nint actor, uint flags) => 0;
}
