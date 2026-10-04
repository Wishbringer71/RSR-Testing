using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.ImGuiMethods;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using RotationSolver.Basic.Configuration;
using RotationSolver.Data;
using GAction = Lumina.Excel.Sheets.Action;
using Status = Lumina.Excel.Sheets.Status;

namespace RotationSolver.UI;

public partial class MainWindow
{
	private static readonly Lazy<Status[]> _allDispelStatus = new(() =>
	{
		var sheet = Service.GetSheet<Status>();
		var list = new List<Status>();
		foreach (var s in sheet)
		{
			if (s.CanDispel)
			{
				list.Add(s);
			}
		}
		return [.. list];
	});

	internal static Status[] AllDispelStatus => _allDispelStatus.Value;

	private static readonly Lazy<Status[]> _allStatus = new(() =>
	{
		var sheet = Service.GetSheet<Status>();
		if (sheet == null)
		{
			return [];
		}

		var list = new List<Status>();
		foreach (var s in sheet)
		{
			if (!string.IsNullOrEmpty(s.Name.ToString()) && s.Icon != 0)
			{
				list.Add(s);
			}
		}
		return [.. list];
	});

	internal static Status[] AllStatus => _allStatus.Value;

	private static readonly Lazy<GAction[]> _allActions = new(() =>
	{
		var sheet = Service.GetSheet<GAction>();
		var list = new List<GAction>();
		foreach (var a in sheet)
		{
			if (!string.IsNullOrEmpty(a.ToString()) && !a.IsPvP && !a.IsPlayerAction
				&& a.Cast100ms > 0)
			{
				list.Add(a);
			}
		}
		var result = new GAction[list.Count];
		for (var i = 0; i < list.Count; i++)
		{
			result[i] = list[i];
		}
		return result;
	});

	internal static GAction[] AllActions => _allActions.Value;

	private const int BadStatusCategory = 2;
	private static readonly Lazy<Status[]> _badStatus = new(() =>
	{
		var sheet = Service.GetSheet<Status>();
		var list = new List<Status>();
		foreach (var s in sheet)
		{
			if (s.StatusCategory == BadStatusCategory && s.Icon != 0)
			{
				list.Add(s);
			}
		}
		return [.. list];
	});

	internal static Status[] BadStatus => _badStatus.Value;

	private static void DrawList()
	{
		DrawPageIntro(UiString.ConfigWindow_List_Description.GetDescription());
		_idsHeader?.Draw();
	}

	private static readonly CollapsingHeaderGroup _idsHeader = BuildHeaderGroup(
		new Dictionary<Func<string>, Action>
		{
			{ () => UiString.ConfigWindow_List_Statuses.GetDescription(), DrawListStatuses },
			{ () => Service.Config.UseDefenseAbility ? UiString.ConfigWindow_List_Actions.GetDescription() : string.Empty, DrawListActions },
			{ () => UiString.ConfigWindow_List_Territories.GetDescription(), DrawListTerritories },
		},
		(UiString.ConfigWindow_List_Statuses, FontAwesomeIcon.Star),
		(UiString.ConfigWindow_List_Actions, FontAwesomeIcon.Bolt),
		(UiString.ConfigWindow_List_Territories, FontAwesomeIcon.MapMarkedAlt));

	private static void DrawListStatuses()
	{
		ImGui.SetNextItemWidth(ImGui.GetWindowWidth());
		_ = ImGui.InputTextWithHint("##Searching the action", UiString.ConfigWindow_List_StatusNameOrId.GetDescription(), ref _statusSearching, 50);

		using var table = ImRaii.Table("Rotation Solver List Statuses", 4, ImGuiTableFlags.BordersInner | ImGuiTableFlags.Resizable | ImGuiTableFlags.SizingStretchSame);
		if (table)
		{
			ImGui.TableSetupScrollFreeze(0, 1);
			ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

			_ = ImGui.TableNextColumn();
			if (ImGui.Button("Reset and Update Invuln Status List"))
			{
				OtherConfiguration.ResetInvincibleStatus();
			}
			ImGui.TableHeader(UiString.ConfigWindow_List_Invincibility.GetDescription());

			_ = ImGui.TableNextColumn();
			if (ImGui.Button("Reset and Update Priority Status List"))
			{
				OtherConfiguration.ResetPriorityStatus();
			}
			ImGui.TableHeader(UiString.ConfigWindow_List_Priority.GetDescription());

			_ = ImGui.TableNextColumn();
			if (ImGui.Button("Reset and Update Dispell Debuff List"))
			{
				OtherConfiguration.ResetDangerousStatus();
			}
			ImGui.TableHeader(UiString.ConfigWindow_List_DangerousStatus.GetDescription());

			_ = ImGui.TableNextColumn();
			if (ImGui.Button("Reset and Update No Casting Status List"))
			{
				OtherConfiguration.ResetNoCastingStatus();
			}
			ImGui.TableHeader(UiString.ConfigWindow_List_NoCastingStatus.GetDescription());

			ImGui.TableNextRow();

			_ = ImGui.TableNextColumn();
			ImGui.TextWrapped(UiString.ConfigWindow_List_InvincibilityDesc.GetDescription());
			DrawStatusList(nameof(OtherConfiguration.InvincibleStatus), OtherConfiguration.InvincibleStatus, AllStatus);

			_ = ImGui.TableNextColumn();
			ImGui.TextWrapped(UiString.ConfigWindow_List_PriorityDesc.GetDescription());
			DrawStatusList(nameof(OtherConfiguration.PriorityStatus), OtherConfiguration.PriorityStatus, AllStatus);

			_ = ImGui.TableNextColumn();
			ImGui.TextWrapped(UiString.ConfigWindow_List_DangerousStatusDesc.GetDescription());
			DrawStatusList(nameof(OtherConfiguration.DangerousStatus), OtherConfiguration.DangerousStatus, AllDispelStatus);

			_ = ImGui.TableNextColumn();
			ImGui.TextWrapped(UiString.ConfigWindow_List_NoCastingStatusDesc.GetDescription());
			DrawStatusList(nameof(OtherConfiguration.NoCastingStatus), OtherConfiguration.NoCastingStatus, BadStatus);
		}
	}

	private static void FromClipBoardButton(HashSet<uint> items)
	{
		const string CopyErrorMessage = "Failed to copy the values to the clipboard.";
		const string PasteErrorMessage = "Failed to copy the values from the clipboard.";

		if (ImGui.Button(UiString.ConfigWindow_Actions_Copy.GetDescription()))
		{
			try
			{
				ImGui.SetClipboardText(JsonConvert.SerializeObject(items));
			}
			catch (Exception ex)
			{
				PluginLog.Warning($"{CopyErrorMessage}: {ex.Message}");
			}
		}

		ImGui.SameLine();

		if (ImGui.Button(UiString.ActionSequencer_FromClipboard.GetDescription()))
		{
			try
			{
				var clipboardText = ImGui.GetClipboardText();
				if (clipboardText != null)
				{
					foreach (var aId in JsonConvert.DeserializeObject<uint[]>(clipboardText) ?? [])
					{
						_ = items.Add(aId);
					}
				}
			}
			catch (Exception ex)
			{
				PluginLog.Warning($"{PasteErrorMessage}: {ex.Message}");
			}
			finally
			{
				_ = OtherConfiguration.Save();
				ImGui.CloseCurrentPopup();
			}
		}
	}

	private static string _statusSearching = string.Empty;
	private static void DrawStatusList(string name, HashSet<uint> statuses, Status[] allStatus)
	{
		const float IconWidth = 24f;
		const float IconHeight = 32f;
		const uint DefaultNotLoadId = 0;

		ImGui.PushID(name);
		FromClipBoardButton(statuses);

		uint removeStatusId = 0;
		var notLoadId = DefaultNotLoadId;

		var popupId = $"Rotation Solver Popup{name}";

		StatusPopUp(popupId, allStatus, ref _statusSearching, status =>
		{
			_ = statuses.Add(status.RowId);
			_ = OtherConfiguration.Save();
		}, notLoadId);

		var count = Math.Max(1, (int)MathF.Floor(ImGui.GetColumnWidth() / ((IconWidth * Scale) + ImGui.GetStyle().ItemSpacing.X)));
		var index = 0;

		if (index++ % count != 0)
		{
			ImGui.SameLine();
		}
		if (ImGui.Button("+", new Vector2(IconWidth, IconHeight) * Scale))
		{
			if (!ImGui.IsPopupOpen(popupId))
			{
				ImGui.OpenPopup(popupId);
			}
		}
		ImguiTooltips.HoveredTooltip(UiString.ConfigWindow_List_AddStatus.GetDescription());

		var statusSheet = Service.GetSheet<Status>();
		foreach (var statusId in statuses)
		{
			if (!statusSheet.TryGetRow(statusId, out var status) || status.RowId == 0)
			{
				continue;
			}

			void Delete()
			{
				removeStatusId = status.RowId;
			}

			var key = $"Status{status.RowId}";

			ImGuiHelper.DrawHotKeysPopup(key, string.Empty, (UiString.ConfigWindow_List_Remove.GetDescription(), Delete, ImGuiHelper.DeleteHint));

			if (IconSet.GetTexture(status.Icon, out var texture, notLoadId) && texture?.Handle != null)
			{
				if (index++ % count != 0)
				{
					ImGui.SameLine();
				}
				_ = ImGuiHelper.NoPaddingNoColorImageButton(texture, new Vector2(IconWidth, IconHeight) * Scale, $"Status{status.RowId}");

				ImGuiHelper.ExecuteHotKeysPopup(key, string.Empty, $"{status.Name} ({status.RowId})", false,
					(Delete, new[] { VirtualKey.DELETE }));
			}
		}

		if (removeStatusId != 0)
		{
			_ = statuses.Remove(removeStatusId);
			_ = OtherConfiguration.Save();
		}
		ImGui.PopID();
	}

	private static Status[]? _statusPopupSource;
	private static string? _statusPopupSearch;
	private static readonly List<(Status status, float score)> _statusPopupResults = [];

	internal static void StatusPopUp(string popupId, Status[] allStatus, ref string searching, Action<Status> clicked, uint notLoadId = 0, float size = 32)
	{
		const float InputWidth = 200f;
		const float ChildHeight = 400f;
		const int InputTextLength = 128;

		using var popup = ImRaii.Popup(popupId);
		if (popup)
		{
			ImGui.SetNextItemWidth(InputWidth * Scale);
			_ = ImGui.InputTextWithHint("##Searching the status", "Enter status name/number", ref searching, InputTextLength);

			ImGui.Spacing();

			using var child = ImRaii.Child("Rotation Solver Reborn Add Status", new Vector2(-1, ChildHeight * Scale));
			if (child)
			{
				var count = Math.Max(1, (int)MathF.Floor(ImGui.GetWindowWidth() / ((size * 3 / 4 * Scale) + ImGui.GetStyle().ItemSpacing.X)));
				var index = 0;

				if (string.IsNullOrWhiteSpace(searching))
				{
					return;
				}

				if (!ReferenceEquals(allStatus, _statusPopupSource) || !string.Equals(searching, _statusPopupSearch, StringComparison.Ordinal))
				{
					_statusPopupSource = allStatus;
					_statusPopupSearch = searching;
					_statusPopupResults.Clear();

					var keys = SearchableCollection.SplitQuery(searching);
					for (var i = 0; i < allStatus.Length; i++)
					{
						var s = allStatus[i];
						var sim = SearchableCollection.Similarity($"{s.Name} {s.RowId}", keys);
						if (sim > 0)
						{
							_statusPopupResults.Add((s, sim));
						}
					}

					_statusPopupResults.Sort((a, b) => b.score.CompareTo(a.score));
				}

				var filtered = _statusPopupResults;
				if (filtered.Count == 0)
				{
					ImGui.TextColored(ImGuiColors.DalamudRed, "No matching statuses found.");
					return;
				}

				foreach (var tuple in filtered)
				{
					var status = tuple.status;
					if (status.Icon != 215049 && IconSet.GetTexture(status.Icon, out var texture, notLoadId) && texture?.Handle != null)
					{
						if (index++ % count != 0)
						{
							ImGui.SameLine();
						}
						if (ImGuiHelper.NoPaddingNoColorImageButton(texture, new Vector2(size * 3 / 4, size) * Scale, $"Adding{status.RowId}"))
						{
							clicked?.Invoke(status);
							ImGui.CloseCurrentPopup();
						}
						ImguiTooltips.HoveredTooltip($"{status.Name} ({status.RowId})");
					}
				}
			}
		}
	}

	private static void DrawListActions()
	{
		ImGui.SetNextItemWidth(ImGui.GetWindowWidth());
		_ = ImGui.InputTextWithHint("##Searching the action", UiString.ConfigWindow_List_ActionNameOrId.GetDescription(), ref _actionSearching, 50);

		using var table = ImRaii.Table("Rotation Solver List Actions", 4, ImGuiTableFlags.BordersInner | ImGuiTableFlags.Resizable | ImGuiTableFlags.SizingStretchSame);
		if (table)
		{
			ImGui.TableSetupScrollFreeze(0, 1);
			ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

			_ = ImGui.TableNextColumn();
			if (ImGui.Button("Reset and Update Tankbuster List"))
			{
				OtherConfiguration.ResetHostileCastingTank();
			}
			ImGui.TableHeader(UiString.ConfigWindow_List_HostileCastingTank.GetDescription());

			_ = ImGui.TableNextColumn();
			if (ImGui.Button("Reset and Update AOE List"))
			{
				OtherConfiguration.ResetHostileCastingArea();
			}
			ImGui.TableHeader(UiString.ConfigWindow_List_HostileCastingArea.GetDescription());

			_ = ImGui.TableNextColumn();
			if (ImGui.Button("Reset and Update Knockback List"))
			{
				OtherConfiguration.ResetHostileCastingKnockback();
			}
			ImGui.TableHeader(UiString.ConfigWindow_List_HostileCastingKnockback.GetDescription());

			_ = ImGui.TableNextColumn();
			if (ImGui.Button("Reset and Stop Casting List"))
			{
				OtherConfiguration.ResetHostileCastingStop();
			}
			ImGui.TableHeader(UiString.ConfigWindow_List_HostileCastingStop.GetDescription());

			ImGui.TableNextRow();

			_ = ImGui.TableNextColumn();
			ImGui.TextWrapped(UiString.ConfigWindow_List_HostileCastingTankDesc.GetDescription());
			DrawActionsList(nameof(OtherConfiguration.HostileCastingTank), OtherConfiguration.HostileCastingTank);

			_ = ImGui.TableNextColumn();
			_allSearchable.DrawItems(Configs.List);
			ImGui.TextWrapped(UiString.ConfigWindow_List_HostileCastingAreaDesc.GetDescription());
			DrawActionsList(nameof(OtherConfiguration.HostileCastingArea), OtherConfiguration.HostileCastingArea,
				OtherConfiguration.HostileCastingAreaPotential);

			// How much of the list has been rated, and how hard those actions were seen to hit. This
			// is the probe for a change that deliberately alters no behaviour: without it, whether
			// the store fills up at all would be as unknown after shipping as before. In repeated
			// content - an extreme trial, a savage fight being progged - "rated" should reach the
			// number of area actions in that fight after a single clear.
			//
			// Separate from the list above on purpose: DrawActionsList serves four lists through one
			// signature, and leaving it alone is what makes this store free to introduce.
			var rated = OtherConfiguration.HostileCastingAreaPotential;
			ImGui.Text($"Damage potential recorded: {rated.Count} of {OtherConfiguration.HostileCastingArea.Count}");

			// What the store itself last did with the file. The count above is the table in memory,
			// which looks the same whether the readings reached the disk or not - and whether a login
			// found a file, found none, or found one it could not read. This line is written by the
			// load and by every save, and a save reads the file back before it reports success.
			ImGui.TextColored(
				OtherConfiguration.AreaPotentialStoreState.Contains("FAILED") || OtherConfiguration.AreaPotentialStoreState.Contains("MISMATCH") || OtherConfiguration.AreaPotentialStoreState.Contains("NOT SAVED")
					? ImGuiColors.DalamudRed
					: ImGuiColors.DalamudGrey,
				"Store: " + OtherConfiguration.AreaPotentialStoreState);
			ImGui.TextColored(ImGuiColors.DalamudGrey, "Last hit: " + DataCenter.AreaMeasurementLastOutcome);
			ImGui.TextColored(ImGuiColors.DalamudGrey, "Casts this session: " + DataCenter.AreaMeasurementTallyText);
			ImGui.TextColored(DataCenter.EffectHandlerErrors > 0 ? ImGuiColors.DalamudRed : ImGuiColors.DalamudGrey,
				"Effect handler: " + $"{DataCenter.EffectSetsReceived} sets, {DataCenter.EnemyEffectSets} from enemies, "
				+ $"{DataCenter.EnemyHitsOnPlayer} hit you, {DataCenter.EffectHandlerErrors} errors");
			if (DataCenter.EffectHandlerFirstError.Length > 0)
			{
				ImGui.TextColored(ImGuiColors.DalamudRed, "First error: " + DataCenter.EffectHandlerFirstError);
			}
			if (OtherConfiguration.HostileCastingArea.Count == 0)
			{
				// Only listed actions are measured, so an empty list measures nothing, whatever the
				// fight. A failed download at first start leaves exactly that behind.
				ImGui.TextColored(ImGuiColors.DalamudRed,
					"The AoE list is empty, so nothing can be measured - press \"Reset and Update AOE List\".");
			}
			if (rated.Count > 0)
			{
				var highest = 0f;
				foreach (var share in rated.Values)
				{
					if (share > highest)
					{
						highest = share;
					}
				}
				ImGui.Text($"Hardest hit seen: {highest * 100f:F0}% of a member's maximum HP");

				// Whether the arithmetic has ever fired, which the two numbers above cannot say: they
				// report that the store fills up, and between "a value exists" and "a cooldown was
				// saved" lies the whole calculation. The premortem found exactly that gap once already
				// - an earlier draft compared against 0.15 and could never have fired - and it was
				// caught by thinking it through rather than by measuring. This line measures it.
				//
				// Zero is two different answers, so the setting is named alongside: switched off the
				// rule cannot fire, switched on it means every rated action in the content played so
				// far was worth its mitigation.
				if (!Service.Config.SkipMitigationForSmallAreaCasts)
				{
					ImGui.TextColored(ImGuiColors.DalamudYellow,
						"Mitigation is never withheld: \"Skip party mitigation for small area casts\" is off.");
				}
				else
				{
					ImGui.Text("Mitigation withheld as too small, this session: "
						+ $"{DataCenter.AreaMitigationSkipped.Count} of {rated.Count} rated action(s)");
				}

				// The two rules below change behaviour only when switched on, and in the fight their
				// working looks exactly like their absence - healing that arrives early looks like
				// ordinary healing, and a mitigation that goes out looks like any other. Each gets a
				// line that says whether it has fired at all, so switching one on is a test and not
				// a guess. Both lines name the setting when it is off, because zero then has two
				// possible meanings.
				if (!Service.Config.HealAheadOfAnnouncedHit)
				{
					ImGui.TextColored(ImGuiColors.DalamudGrey,
						"No healing ahead of area casts: \"Heal ahead of an announced area cast\" is off.");
				}
				else
				{
					ImGui.Text("Healed ahead of an announced cast, this session: "
						+ $"{DataCenter.HealedAheadOfAreaCast.Count} action(s)");
				}

				if (!Service.Config.HoldProactiveMitigationForSmallCast)
				{
					ImGui.TextColored(ImGuiColors.DalamudGrey,
						"Predicted mitigations follow the timing alone: \"Hold a predicted mitigation "
						+ "while a small cast is running\" is off.");
				}
				else
				{
					// The rule scores itself and acts on that score, so this line reports a verdict
					// rather than asking for one to be drawn from it.
					var (vindicated, wasted) = DataCenter.ProactiveHoldRecord;
					ImGui.Text("Predicted mitigation held for a small cast, this fight: "
						+ $"{DataCenter.ProactiveMitigationHeld.Count} action(s), "
						+ $"{vindicated} followed by a big hit, {wasted} not");
					if (vindicated + wasted > 0 && !DataCenter.ProactiveHoldIsEarningItsKeep)
					{
						ImGui.TextColored(ImGuiColors.DalamudYellow,
							"  Holding is wrong more often than right in this fight - the rule has "
							+ "stood itself down until that turns around.");
					}
				}

				if (!Service.Config.MitigateBigAreaCastsEvenIfInterruptible)
				{
					ImGui.TextColored(ImGuiColors.DalamudGrey,
						"Interruptible casts are never mitigated: \"Mitigate a big area cast even when "
						+ "it is interruptible\" is off.");
				}
				else
				{
					ImGui.Text("Mitigated although interruptible, this session: "
						+ $"{DataCenter.MitigatedInterruptibleCast.Count} action(s)");
				}
			}

			_ = ImGui.TableNextColumn();
			_allSearchable.DrawItems(Configs.List2);
			ImGui.TextWrapped(UiString.ConfigWindow_List_HostileCastingKnockbackDesc.GetDescription());
			DrawActionsList(nameof(OtherConfiguration.HostileCastingKnockback), OtherConfiguration.HostileCastingKnockback);

			_ = ImGui.TableNextColumn();
			_allSearchable.DrawItems(Configs.List3);
			ImGui.TextWrapped(UiString.ConfigWindow_List_HostileCastingStopDesc.GetDescription());
			DrawActionsList(nameof(OtherConfiguration.HostileCastingStop), OtherConfiguration.HostileCastingStop);
		}
	}

	private static string _actionSearching = string.Empty;
	private static string _actionPopupSearching = string.Empty;
	private static string _lastActionPopupSearching = string.Empty;
	private static readonly List<(GAction action, float sim)> _cachedPopupFiltered = [];

	/// <summary>
	/// Draws one of the learned action lists. <paramref name="potential"/> is optional and only the
	/// area list passes it: where a measured damage share exists for an entry, the entry says so.
	/// Four lists share this one signature, so the parameter is additive and the other three callers
	/// are unchanged.
	/// </summary>
	private static void DrawActionsList(string name, HashSet<uint> actions, Dictionary<uint, float>? potential = null)
	{
		actions ??= [];
		if (name == null)
		{
			return;
		}
		ImGui.PushID(name);
		uint removeId = 0;
		var popupId = $"Rotation Solver Reborn Action Popup{name}";

		if (ImGui.Button($"{UiString.ConfigWindow_List_AddAction.GetDescription()}##{name}"))
		{
			if (!ImGui.IsPopupOpen(popupId))
			{
				ImGui.OpenPopup(popupId);
			}
		}

		ImGui.SameLine();
		FromClipBoardButton(actions);

		ImGui.Spacing();

		List<GAction> actionList = [];
		var actionSheet = Service.GetSheet<GAction>();
		foreach (var a in actions)
		{
			if (actionSheet.TryGetRow(a, out var act))
			{
				actionList.Add(act);
			}
		}

		if (!string.IsNullOrEmpty(_actionSearching))
		{
			var scored = new List<(GAction action, float score)>(actionList.Count);
			var searchKeys = SearchableCollection.SplitQuery(_actionSearching);
			foreach (var action in actionList)
			{
				var sim = SearchableCollection.Similarity($"{action.Name} {action.RowId}", searchKeys);
				scored.Add((action, sim));
			}
			scored.Sort((a, b) => b.score.CompareTo(a.score));
			actionList.Clear();
			foreach ((var action, var score) in scored)
			{
				actionList.Add(action);
			}
		}

		for (var idx = 0; idx < actionList.Count; idx++)
		{
			var action = actionList[idx];
			void Reset() => removeId = action.RowId;
			var key = $"Action{action.RowId}";

			ImGuiHelper.DrawHotKeysPopup(key, string.Empty, (UiString.ConfigWindow_List_Remove.GetDescription(), Reset, ImGuiHelper.DeleteHint));

			// The measurement per entry, not just the total. A raidwide sitting at three percent is a
			// reading taken while the party was well shielded, and only the name beside the number
			// makes that visible - the alternative is a single "hardest hit" figure that cannot say
			// which action is rated wrong. The second half says whether this entry has actually
			// withheld a mitigation, which is what turns a stored number into an observed effect.
			var label = $"{action.Name} ({action.RowId})";
			if (potential != null && potential.TryGetValue(action.RowId, out var measured) && measured > 0f)
			{
				label += $"  -  {measured * 100f:F0}% measured";
				if (DataCenter.AreaMitigationSkipped.ContainsKey(action.RowId))
				{
					label += ", mitigation withheld";
				}
				if (DataCenter.HealedAheadOfAreaCast.ContainsKey(action.RowId))
				{
					label += ", healed ahead";
				}
				if (DataCenter.MitigatedInterruptibleCast.ContainsKey(action.RowId))
				{
					label += ", mitigated although interruptible";
				}
				if (DataCenter.ProactiveMitigationHeld.ContainsKey(action.RowId))
				{
					label += ", predicted mitigation held";
				}
			}
			_ = ImGui.Selectable(label);

			ImGuiHelper.ExecuteHotKeysPopup(key, string.Empty, string.Empty, false, (Reset, new[] { VirtualKey.DELETE }));
		}

		if (removeId != 0)
		{
			_ = actions.Remove(removeId);
			_ = OtherConfiguration.Save();
		}

		ActionPopup(popupId, actions);

		ImGui.PopID();
	}

	private static void ActionPopup(string popupId, HashSet<uint> actions)
	{
		const float InputWidth = 200f;
		const float ChildHeight = 400f;
		const int MaxDisplayCount = 20;

		using var popup = ImRaii.Popup(popupId);
		if (popup)
		{
			ImGui.SetNextItemWidth(InputWidth * Scale);
			_ = ImGui.InputTextWithHint("##Searching the action pop up", UiString.ConfigWindow_List_ActionNameOrId.GetDescription(), ref _actionPopupSearching, 50);

			ImGui.Spacing();

			using var child = ImRaii.Child("Rotation Solver Add action", new Vector2(-1, ChildHeight * Scale));
			if (child)
			{
				if (string.IsNullOrWhiteSpace(_actionPopupSearching))
				{
					ImGui.TextColored(ImGuiColors.DalamudYellow, "Enter a search term to filter actions.");
					if (!string.IsNullOrEmpty(_lastActionPopupSearching))
					{
						_lastActionPopupSearching = string.Empty;
						_cachedPopupFiltered.Clear();
					}
				}
				else
				{
					if (!string.Equals(_actionPopupSearching, _lastActionPopupSearching, StringComparison.Ordinal))
					{
						_lastActionPopupSearching = _actionPopupSearching;
						_cachedPopupFiltered.Clear();

						var searchLower = _actionPopupSearching.Trim().ToLowerInvariant();
						var useSimilarity = searchLower.Length >= 3;
						var searchKeys = SearchableCollection.SplitQuery(_actionPopupSearching);

						for (var i = 0; i < AllActions.Length; i++)
						{
							var a = AllActions[i];

							if (actions.Contains(a.RowId))
							{
								continue;
							}

							var nameLower = a.Name.ToString().ToLowerInvariant();
							var idStr = a.RowId.ToString();

							if (nameLower.Contains(searchLower) || idStr == searchLower)
							{
								_cachedPopupFiltered.Add((a, 1000f));
							}
							else if (useSimilarity)
							{
								var sim = SearchableCollection.Similarity($"{a.Name} {a.RowId}", searchKeys);
								if (sim > 0f)
								{
									_cachedPopupFiltered.Add((a, sim));
								}
							}
						}

						if (_cachedPopupFiltered.Count > 1)
						{
							_cachedPopupFiltered.Sort((x, y) => y.sim.CompareTo(x.sim));
						}
					}

					var shown = 0;
					for (var i = 0; i < _cachedPopupFiltered.Count && shown < MaxDisplayCount; i++)
					{
						var action = _cachedPopupFiltered[i].action;
						var selected = ImGui.Selectable($"{action.Name} ({action.RowId})");
						if (ImGui.IsItemHovered())
						{
							ImguiTooltips.ShowTooltip($"{action.Name} ({action.RowId})");
							if (selected)
							{
								_ = actions.Add(action.RowId);
								_ = OtherConfiguration.Save();
								ImGui.CloseCurrentPopup();
							}
						}
						shown++;
					}

					if (shown == 0)
					{
						ImGui.TextColored(ImGuiColors.DalamudRed, "No matching actions found.");
					}
				}
			}
		}
	}

	private static void DrawListTerritories()
	{
		if (Svc.ClientState == null)
		{
			return;
		}

		var territoryId = Svc.ClientState.TerritoryType;

		using var table = ImRaii.Table("Rotation Solver List Territories", 4,
			ImGuiTableFlags.BordersInner | ImGuiTableFlags.Resizable | ImGuiTableFlags.SizingStretchSame);
		if (table)
		{
			ImGui.TableSetupScrollFreeze(0, 1);
			ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

			_ = ImGui.TableNextColumn();
			ImGui.TableHeader(UiString.ConfigWindow_List_NoHostile.GetDescription());
			_ = ImGui.TableNextColumn();
			ImGui.TableHeader(UiString.ConfigWindow_List_NoProvoke.GetDescription());
			_ = ImGui.TableNextColumn();
			ImGui.TableHeader(UiString.ConfigWindow_List_BeneficialPositions.GetDescription());

			ImGui.TableNextRow();

			_ = ImGui.TableNextColumn();
			ImGui.TextWrapped(UiString.ConfigWindow_List_NoHostileDesc.GetDescription());
			var width = ImGui.GetColumnWidth() - ImGuiEx.CalcIconSize(FontAwesomeIcon.Ban).X - ImGui.GetStyle().ItemSpacing.X - (10 * Scale);

			if (!OtherConfiguration.NoHostileNames.TryGetValue(territoryId, out var libs))
			{
				OtherConfiguration.NoHostileNames[territoryId] = libs = [];
			}

			var hasEmpty = false;
			for (var i = 0; i < libs.Length; i++)
			{
				if (string.IsNullOrEmpty(libs[i]))
				{
					hasEmpty = true;
					break;
				}
			}
			if (!hasEmpty)
			{
				var newArr = new string[libs.Length + 1];
				for (var i = 0; i < libs.Length; i++)
				{
					newArr[i] = libs[i];
				}

				newArr[^1] = string.Empty;
				OtherConfiguration.NoHostileNames[territoryId] = libs = newArr;
			}

			var removeIndex = -1;
			for (var i = 0; i < libs.Length; i++)
			{
				ImGui.SetNextItemWidth(width);
				if (ImGui.InputTextWithHint($"##Rotation Solver Territory Target Name {i}",
					UiString.ConfigWindow_List_NoHostilesName.GetDescription(), ref libs[i], 1024))
				{
					OtherConfiguration.NoHostileNames[territoryId] = libs;
					_ = OtherConfiguration.SaveNoHostileNames();
				}
				ImGui.SameLine();
				if (ImGuiEx.IconButton(FontAwesomeIcon.Ban, $"##Rotation Solver Remove Territory Target Name {i}"))
				{
					removeIndex = i;
				}
			}
			if (removeIndex > -1)
			{
				var list = new List<string>(libs.Length - 1);
				for (var i = 0; i < libs.Length; i++)
				{
					if (i == removeIndex)
					{
						continue;
					}

					list.Add(libs[i]);
				}
				OtherConfiguration.NoHostileNames[territoryId] = [.. list];
				_ = OtherConfiguration.SaveNoHostileNames();
			}

			_ = ImGui.TableNextColumn();
			ImGui.TextWrapped(UiString.ConfigWindow_List_NoProvokeDesc.GetDescription());

			width = ImGui.GetColumnWidth() - ImGuiEx.CalcIconSize(FontAwesomeIcon.Ban).X
				- ImGui.GetStyle().ItemSpacing.X - (10 * Scale);

			if (!OtherConfiguration.NoProvokeNames.TryGetValue(territoryId, out libs))
			{
				OtherConfiguration.NoProvokeNames[territoryId] = libs = [];
			}

			hasEmpty = false;
			for (var i = 0; i < libs.Length; i++)
			{
				if (string.IsNullOrEmpty(libs[i]))
				{ hasEmpty = true; break; }
			}
			if (!hasEmpty)
			{
				var newArr = new string[libs.Length + 1];
				for (var i = 0; i < libs.Length; i++)
				{
					newArr[i] = libs[i];
				}

				newArr[^1] = string.Empty;
				OtherConfiguration.NoProvokeNames[territoryId] = libs = newArr;
			}

			removeIndex = -1;
			for (var i = 0; i < libs.Length; i++)
			{
				ImGui.SetNextItemWidth(width);
				if (ImGui.InputTextWithHint($"##Rotation Solver Reborn Territory Provoke Name {i}",
					UiString.ConfigWindow_List_NoProvokeName.GetDescription(), ref libs[i], 1024))
				{
					OtherConfiguration.NoProvokeNames[territoryId] = libs;
					_ = OtherConfiguration.SaveNoProvokeNames();
				}
				ImGui.SameLine();
				if (ImGuiEx.IconButton(FontAwesomeIcon.Ban, $"##Rotation Solver Reborn Remove Territory Provoke Name {i}"))
				{
					removeIndex = i;
				}
			}
			if (removeIndex > -1)
			{
				var list = new List<string>(libs.Length - 1);
				for (var i = 0; i < libs.Length; i++)
				{
					if (i == removeIndex)
					{
						continue;
					}

					list.Add(libs[i]);
				}
				OtherConfiguration.NoProvokeNames[territoryId] = [.. list];
				_ = OtherConfiguration.SaveNoProvokeNames();
			}

			_ = ImGui.TableNextColumn();
			if (!OtherConfiguration.BeneficialPositions.TryGetValue(territoryId, out var pts))
			{
				OtherConfiguration.BeneficialPositions[territoryId] = pts = [];
			}

			if (ImGui.Button(UiString.ConfigWindow_List_AddPosition.GetDescription()) && Player.Object != null && Player.Available)
			{
				unsafe
				{
					var point = Player.Object.Position;
					var pointMathed = point + (Vector3.UnitY * 5);
					var direction = Vector3.UnitY;
					var directionPtr = &direction;
					var pointPtr = &pointMathed;
					var unknown = stackalloc int[] { 0x4000, 0, 0x4000, 0 };
					RaycastHit hit = default;

					var newPts = new Vector3[pts.Length + 1];
					for (var i = 0; i < pts.Length; i++)
					{
						newPts[i] = pts[i];
					}

					if (Framework.Instance()->BGCollisionModule
						->RaycastMaterialFilter(&hit, pointPtr, directionPtr, 20, 1, unknown))
					{
						newPts[^1] = hit.Point;
					}
					else
					{
						newPts[^1] = point;
					}
					OtherConfiguration.BeneficialPositions[territoryId] = newPts;
					_ = OtherConfiguration.SaveBeneficialPositions();
				}
			}

			var removePosIndex = -1;
			for (var i = 0; i < pts.Length; i++)
			{
				void Reset() => removePosIndex = i;
				var key = "Beneficial Positions" + i.ToString();
				ImGuiHelper.DrawHotKeysPopup(key, string.Empty,
					(UiString.ConfigWindow_List_Remove.GetDescription(), Reset, ["Delete"]));
				_ = ImGui.Selectable(pts[i].ToString());

				ImGuiHelper.ExecuteHotKeysPopup(key, string.Empty, string.Empty, false,
					(Reset, [VirtualKey.DELETE]));
			}
			if (removePosIndex > -1)
			{
				var list = new List<Vector3>(pts.Length - 1);
				for (var i = 0; i < pts.Length; i++)
				{
					if (i == removePosIndex)
					{
						continue;
					}

					list.Add(pts[i]);
				}
				OtherConfiguration.BeneficialPositions[territoryId] = [.. list];
				_ = OtherConfiguration.SaveBeneficialPositions();
			}
		}
	}
}
