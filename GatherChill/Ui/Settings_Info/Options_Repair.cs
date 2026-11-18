using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using GatherChill.Enums;
using GatherChill.Gui;
using GatherChill.Utilities.Traveling;
using System;
using System.Collections.Generic;
using System.Text;

namespace GatherChill.Ui.Settings_Info;

public static partial class SettingsUi
{
    private static readonly string RepairOption = "Repair & Spiritbond Options";

    private static readonly List<MenderDestination> MenderOptions = MenderDestination.Unique();

    private static SettingEntry RepairMethod = new()
    {
        Label = "Repair Method",
        Category = RepairOption,
        Keywords = new[] { "Repair", "Gear", "Durability", "Mender", "Self", "NPC", "Auto" },
        Draw = () =>
        {
            var names = new[] { "Disabled", "Self Repair (Dark Matter)", "Mender (Gil)" };
            var current = (int)C.Repair_Mode;

            ImGui.SetNextItemWidth(250 * ImGuiHelpers.GlobalScale);
            if (ImGui.Combo("Repair Method", ref current, names, names.Length))
            {
                C.Repair_Mode = (RepairMode)current;
                C.Save();
            }

            ImGui.SameLine();
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                "Self Repair uses Dark Matter from your inventory while gathering.\n" +
                "Mender travels to a town mender and pays gil to repair all equipped gear.\n" +
                "Both happen before heading to a node, once gear drops to the threshold below.",
                false);
        }
    };

    private static SettingEntry MenderAetheryte = new()
    {
        Label = "Mender Destination",
        Category = RepairOption,
        Keywords = new[] { "Repair", "Mender", "Aetheryte", "Teleport", "Town", "Destination" },
        Draw = () =>
        {
            using var disabled = ImRaii.Disabled(C.Repair_Mode != RepairMode.Mender);

            var labels = new List<string> { "Auto (same zone first, then any attuned mender)" };
            labels.AddRange(MenderOptions.Select(x => x.Label));

            var index = 0;
            if (C.Mender_Aetheryte != 0)
            {
                var match = MenderOptions.FindIndex(x => x.Aetheryte == C.Mender_Aetheryte);
                if (match >= 0)
                    index = match + 1;
            }

            ImGui.SetNextItemWidth(300 * ImGuiHelpers.GlobalScale);
            if (ImGui.Combo("Mender Destination", ref index, labels.ToArray(), labels.Count))
            {
                C.Mender_Aetheryte = index == 0 ? 0 : MenderOptions[index - 1].Aetheryte;
                C.Save();
            }

            ImGui.SameLine();
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                "Where to repair when using a mender.\n" +
                "Auto prefers a mender aetheryte in the zone you're already in, then the first attuned one.\n" +
                "Only attuned aetherytes can be used.",
                false);
        }
    };

    private static SettingEntry DarkMatter = new()
    {
        Label = "Minimum Dark Matter",
        Category = RepairOption,
        Keywords = new[] { "Repair", "Gear", "Dark Matter", "Minimum" },
        Draw = () =>
        {
            using var disabled = ImRaii.Disabled(C.Repair_Mode != RepairMode.Self);

            var v = C.Minimum_DarkMatter;
            ImGui.SetNextItemWidth(150 * ImGuiHelpers.GlobalScale);
            if (ImGui.InputInt("Minimum Dark Matter", ref v, 1, 10))
            {
                C.Minimum_DarkMatter = v;
                C.SaveDebounced();
            }

            ImGui.SameLine();
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle, "Minimum amount of dark matter to allow repairing", false);
        }
    };

    private static SettingEntry RepairAmount = new()
    {
        Label = "Repair Threshold",
        Category = RepairOption,
        Keywords = new[] { "Repair", "Gear", "Durability", "Threshold", "Minimum", "Amount" },
        Draw = () =>
        {
            using var disabled = ImRaii.Disabled(C.Repair_Mode == RepairMode.None);

            var v = C.Repair_Amount;
            ImGui.SetNextItemWidth(150 * ImGuiHelpers.GlobalScale);
            if (ImGui.InputInt("Repair at %", ref v, 1, 10))
            {
                C.Repair_Amount = v;
                C.SaveDebounced();
            }

            ImGui.SameLine();
            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle, "Repair once any equipped item drops to this condition %", false);
        }
    };

    private static SettingEntry Spiritbond_Extract = new()
    {
        Label = "Extract Materia",
        Category = RepairOption,
        Keywords = new[] { "Extract", "Materia", "Spiritbond" },
        Draw = () =>
        {
            var v = C.Extract_Spiritbond;
            if (ImGui.Checkbox("Extract Spiritbonded Gear", ref v))
            {
                C.Extract_Spiritbond = v;
                C.Save();
            }
        }
    };
}
