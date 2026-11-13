using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using Lumina.Excel.Sheets;
using RotationSolver.Basic.Configuration;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace RotationSolver.Basic.Helpers;

public static partial class ObjectHelper
{
	/// <summary>
	/// Whether this member stands in the heal chain's critical class (concept 07): unprotected by an
	/// invulnerability, and at or below <c>HealthForDyingTanks</c> in effective health carried forward
	/// to the moment a heal begun now would land. One definition for every reader - the heal target
	/// order, the defense holds and Lux Solaris - so they cannot disagree about who is in danger.
	/// Unprotected as the heal decision reads it (<see cref="IsUnprotectedForHealing"/>).
	/// </summary>
	internal static bool IsInCriticalClass(this IBattleChara battleChara)
	{
		return battleChara.IsInCriticalClass(battleChara.IsUnprotectedForHealing());
	}

	/// <summary>
	/// <see cref="IsInCriticalClass(IBattleChara)"/> for a caller that has already read whether the
	/// member is protected, so the status list is not walked twice and the answer cannot change
	/// between the two reads.
	/// </summary>
	internal static bool IsInCriticalClass(this IBattleChara battleChara, bool unprotected)
	{
		return unprotected
			&& battleChara.GetForecastEffectiveHpPercent() <= Service.Config.HealthForDyingTanks * 100f;
	}

	/// <summary>
	/// <see cref="GetEffectiveHpPercent"/> carried forward to the moment a heal begun now would land.
	/// </summary>
	internal static int GetForecastEffectiveHpPercent(this IBattleChara battleChara)
	{
		return (int)(battleChara.GetEffectiveHpPercent() * battleChara.GetForecastSurvivingShare());
	}

	/// <summary>
	/// <see cref="GetPlayerHealthRatio"/> carried forward the same way, for the paths that ask about
	/// the player without holding an object reference.
	/// </summary>
	internal static float GetForecastPlayerHealthRatio(bool instant = false)
	{
		return Player.Object == null
			? GetPlayerHealthRatio()
			: GetPlayerHealthRatio() * Player.Object.GetForecastSurvivingShare(instant);
	}

	/// <summary>
	/// Scores the forecast made for this character earlier against what its health actually did, and
	/// files a fresh forecast for the next round. Called once per history sample.
	/// </summary>
	internal static void ScoreTtkForecast(this IBattleChara battleChara)
	{
		if (battleChara == null)
		{
			return;
		}

		var id = battleChara.GameObjectId;
		var now = DateTime.Now;
		var ratio = battleChara.GetHealthRatio();
		var ttk = battleChara.GetTTK();

		if (_ttkForecast.TryGetValue(id, out var last))
		{
			var elapsed = (float)(now - last.At).TotalSeconds;
			var actualFall = last.Ratio - ratio;

			// Only a fall says anything about a forecast of falling. A rise means a heal landed.
			// The forecast also has to have been a number, and enough time has to have passed for
			// the difference to be more than sampling noise.
			if (!float.IsNaN(last.Ttk) && last.Ttk > 0f && elapsed > 0f && actualFall > 0f)
			{
				var expectedFall = elapsed * (last.Ratio / last.Ttk);
				if (expectedFall > 0f)
				{
					var sample = actualFall / expectedFall;
					var previous = _ttkBias.TryGetValue(id, out var b) ? b : 1f;

					// Smoothed, so a single spike does not take over. Bounded because the ratio can
					// run away when a forecast was very long and a burst lands.
					var smoothed = (previous * 3f + sample) / 4f;
					_ttkBias[id] = Math.Clamp(smoothed, 0.25f, 4f);
				}
			}
		}

		_ttkForecast[id] = (now, ttk, ratio);
	}

	/// <summary>
	/// Drops the scoring state of characters that have not been seen for a while.
	/// </summary>
	/// <remarks>
	/// Both dictionaries are keyed by object id and would otherwise grow for every member of every
	/// party and every enemy of a session (A463). Pruning against the current party instead would be cheaper but wrong: a
	/// member who walks out of the object table for a few seconds would come back with the learned
	/// bias reset to 1, which is the one moment - a pull spread over a wide room - where the
	/// correction is worth the most. Age is the honest criterion, and it is the same window the
	/// health history itself keeps.
	/// </remarks>
	internal static void ForgetStaleTtkForecasts()
	{
		// Runs once per history sample over a table about the size of the party; a size below which the
		// scan was skipped only kept stale entries longer (A340).
		var cutoff = DateTime.Now - TtkForecastLifetime;
		foreach ((var id, var entry) in _ttkForecast)
		{
			if (entry.At < cutoff)
			{
				_ = _ttkForecast.TryRemove(id, out _);
				_ = _ttkBias.TryRemove(id, out _);
			}
		}
	}
}
