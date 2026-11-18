using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility;
using System.Collections.Generic;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GatherChill.Scheduler.Handlers;
using GatherChill.Utilities.Tools;
using GatherChill.Utilities.Traveling;
using GatherChill.Utilities.Utility;
using Lumina.Excel.Sheets;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace GatherChill.Scheduler.Tasks
{
    /// <summary>
    /// Walks to a mender NPC and pays gil to repair all gear. Used before a node when the
    /// lowest equipped item is at or below the configured repair threshold.
    /// </summary>
    internal static class Task_MenderRepair
    {
        private const string Tag = "Task: Mender Repair";

        private const uint GilRepairButtonNodeId = 16;
        private const uint RepairAutoConfirmNodeId = 13;

        private const float InteractRange = 4f;
        private const float WalkRange = 120f;

        private static readonly TimeSpan RepairTimeout = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan MenderGrace = TimeSpan.FromSeconds(15);
        private static readonly TimeSpan RepairDisabledGrace = TimeSpan.FromSeconds(10);

        private const int MaxRepairClicks = 4;

        private enum Step
        {
            Idle,
            Travel,
            Approach,
            Interact,
        }

        private static Step _step = Step.Idle;
        private static MenderDestination? _destination;
        private static DateTime _deadline = DateTime.MaxValue;
        private static DateTime _interactedAt = DateTime.MinValue;
        private static DateTime _repairRequestedAt = DateTime.MinValue;
        private static DateTime? _arrivedAt;
        private static DateTime? _repairDisabledSince;
        private static int _repairClicks;

        private static HashSet<uint>? _menderIds;

        public static bool Enqueue()
        {
            if (!Utils.NeedsRepair())
            {
                // Only clean up if we were the ones that opened something
                if (_step == Step.Idle && _repairRequestedAt == DateTime.MinValue)
                    return true;

                if (CloseRepairWindows())
                    return false;

                IceLogging.Debug("Repair is done, heading back to the route", Tag);
                Reset();
                return true;
            }

            if (HandleOpenDialogs())
                return false;

            if (_step == Step.Idle)
            {
                _destination = PickDestination();
                if (_destination == null)
                {
                    Fail("Couldn't find an attuned aetheryte with a mender nearby");
                    return true;
                }

                _deadline = DateTime.UtcNow + RepairTimeout;
                _step = Step.Travel;
                IceLogging.Info($"Gear needs repairing. Heading to {_destination.Label}", Tag);
            }

            if (DateTime.UtcNow > _deadline)
            {
                Fail($"Timed out while trying to repair at {_destination?.Name ?? "the mender"}");
                return true;
            }

            return _step switch
            {
                Step.Travel => TravelToMender(),
                Step.Approach => ApproachMender(),
                Step.Interact => InteractWithMender(),
                _ => true,
            };
        }

        private static void Reset()
        {
            _step = Step.Idle;
            _destination = null;
            _deadline = DateTime.MaxValue;
            _interactedAt = DateTime.MinValue;
            _repairRequestedAt = DateTime.MinValue;
            _arrivedAt = null;
            _repairDisabledSince = null;
            _repairClicks = 0;
        }

        private static void Fail(string reason)
        {
            IceLogging.Error(reason, Tag);
            StopWalking();
            Reset();
            SchedulerMain.Fail(reason);
        }

        private static MenderDestination? PickDestination()
        {
            var here = Player.Territory.RowId;

            if (C.Mender_Aetheryte != 0)
            {
                var configured = MenderDestination.For(C.Mender_Aetheryte);
                if (configured != null && MenderDestination.IsAttuned(configured.Aetheryte))
                    return configured;

                IceLogging.Warning($"The configured mender aetheryte [{C.Mender_Aetheryte}] isn't attuned, picking another one", Tag);
            }

            var sameZone = MenderDestination.All.FirstOrDefault(x => x.Territory == here && MenderDestination.IsAttuned(x.Aetheryte));
            if (sameZone != null)
                return sameZone;

            return MenderDestination.All.FirstOrDefault(x => MenderDestination.IsAttuned(x.Aetheryte));
        }

        private static unsafe bool TravelToMender()
        {
            if (_destination == null)
                return true;

            if (Player.Territory.RowId == _destination.Territory)
            {
                IceLogging.Debug($"Arrived at {_destination.Name}, looking for the mender", Tag);
                _step = Step.Approach;
                return false;
            }

            if (Player.IsBusy || Player.IsCasting
                || Svc.Condition[ConditionFlag.BetweenAreas]
                || Svc.Condition[ConditionFlag.BetweenAreas51])
                return false;

            if (Player.Mounted && EzThrottler.Throttle("Mender Repair Dismount", 1000))
                Utils.Dismount();

            if (EzThrottler.Throttle("Mender Repair Teleport", 5000))
            {
                IceLogging.Debug($"Teleporting to {_destination.Name} [{_destination.Aetheryte}]", Tag);

                if (!Telepo.Instance()->Teleport(_destination.Aetheryte, 0))
                    IceLogging.Warning($"The teleport to {_destination.Name} was refused. Waiting before trying again", Tag);
            }

            return false;
        }

        private static bool ApproachMender()
        {
            if (_destination == null)
                return true;

            var mender = FindNearestMender();

            if (mender == null)
            {
                if (Player.DistanceTo(_destination.Position) > InteractRange)
                {
                    _arrivedAt = null;
                    MoveToward(_destination.Position);
                    return false;
                }

                // Standing on the spot but nothing targetable yet. NPCs can take a moment to load.
                _arrivedAt ??= DateTime.UtcNow;

                if (DateTime.UtcNow - _arrivedAt.Value > MenderGrace)
                {
                    Fail($"Reached {_destination.Name} but there's no mender here");
                    return true;
                }

                if (EzThrottler.Throttle("Mender Repair Waiting"))
                    IceLogging.Debug($"At {_destination.Name}, waiting for a mender to show up", Tag);

                return false;
            }

            _arrivedAt = null;

            if (Player.DistanceTo(mender) > InteractRange)
            {
                MoveToward(mender.Position);
                return false;
            }

            StopWalking();

            if (Player.Mounted || Player.IsJumping)
            {
                if (EzThrottler.Throttle("Mender Repair Dismount Close", 1000))
                    Utils.Dismount();

                return false;
            }

            if (EzThrottler.Throttle("Mender Repair Interact", 1000))
            {
                IceLogging.Debug("Interacting with the mender", Tag);
                Utils.TargetgameObject(mender);
                Utils.InteractWithObject(mender);
                _interactedAt = DateTime.UtcNow;
                _step = Step.Interact;
            }

            return false;
        }

        private static bool InteractWithMender()
        {
            if (FindNearestMender() == null)
            {
                IceLogging.Debug("Lost sight of the mender, walking to it again", Tag);
                _step = Step.Approach;
                return false;
            }

            if (EzThrottler.Throttle("Mender Repair Reinteract", 2000))
            {
                var mender = FindNearestMender();
                if (mender != null)
                {
                    Utils.TargetgameObject(mender);
                    Utils.InteractWithObject(mender);
                    _interactedAt = DateTime.UtcNow;
                }
            }

            return false;
        }

        private static void MoveToward(Vector3 target)
        {
            if (!P.navmesh.IsReady())
            {
                if (EzThrottler.Throttle("Mender Repair Nav Wait", 1000))
                    IceLogging.Debug($"Waiting for navmesh to build [{P.navmesh.BuildProgress():P0}]", Tag);

                return;
            }

            if (P.navmesh.IsRunning())
                return;

            if (EzThrottler.Throttle("Mender Repair Nav Move", 1000))
            {
                IceLogging.Debug($"Pathing to the mender, {Player.DistanceTo(target):N1} yalms away", Tag);
                P.navmesh.PathfindAndMoveCloseTo(target, false, InteractRange - 1);
            }
        }

        private static void StopWalking()
        {
            try
            {
                P.navmesh.Stop();
            }
            catch (Exception ex)
            {
                Svc.Log.Debug(ex, "Couldn't stop navmesh");
            }
        }

        private static IGameObject? FindNearestMender()
        {
            var origin = Player.Position;

            return Svc.Objects
                      .Where(x => x.IsTargetable && MenderIds.Contains(x.BaseId))
                      .OrderBy(x => Vector3.Distance(x.Position, origin))
                      .FirstOrDefault();
        }

        private static HashSet<uint> MenderIds
        {
            get
            {
                if (_menderIds != null)
                    return _menderIds;

                _menderIds = new HashSet<uint>();

                foreach (var npc in Svc.Data.GetExcelSheet<ENpcResident>())
                {
                    if (npc.Singular.ExtractText().Contains("mender", StringComparison.OrdinalIgnoreCase))
                        _menderIds.Add(npc.RowId);
                }

                IceLogging.Debug($"Found {_menderIds.Count} mender NPCs in the sheets", Tag);
                return _menderIds;
            }
        }

        #region Dialogs

        private static unsafe bool HandleOpenDialogs()
        {
            // "Repair all" confirmation. The mender path uses RepairAuto, not SelectYesno.
            if (_repairRequestedAt != DateTime.MinValue
                && GenericHelpers.TryGetAddonByName<AtkUnitBase>("RepairAuto", out var repairAuto)
                && GenericHelpers.IsAddonReady(repairAuto))
            {
                if (FrameThrottler.Throttle("Mender RepairAuto Confirm", 300))
                    ClickNode(repairAuto, RepairAutoConfirmNodeId);

                return true;
            }

            // Some menders ask a yes/no question instead
            if (_repairRequestedAt != DateTime.MinValue
                && DateTime.UtcNow - _repairRequestedAt < TimeSpan.FromSeconds(15)
                && GenericHelpers.TryGetAddonMaster<SelectYesno>(out var yesNo)
                && yesNo.IsAddonReady)
            {
                if (EzThrottler.Throttle("Mender Repair Yesno", 300))
                    yesNo.Yes();

                return true;
            }

            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("Repair", out var repair)
                && GenericHelpers.IsAddonReady(repair))
            {
                var stale = _repairRequestedAt == DateTime.MinValue
                            || DateTime.UtcNow - _repairRequestedAt > TimeSpan.FromSeconds(10);

                if (stale && FrameThrottler.Throttle("Mender Repair All", 500))
                {
                    if (ClickNode(repair, GilRepairButtonNodeId))
                    {
                        _repairRequestedAt = DateTime.UtcNow;
                        _repairDisabledSince = null;
                        _repairClicks++;

                        IceLogging.Debug($"Clicked the mender's repair button (attempt {_repairClicks})", Tag);

                        if (_repairClicks >= MaxRepairClicks)
                        {
                            Fail("The repair button keeps getting pressed, but the gear still isn't repaired. Do you have enough gil?");
                            return true;
                        }
                    }
                    else
                    {
                        // The button is disabled or hidden. That means nothing can be paid for, usually because of gil.
                        _repairDisabledSince ??= DateTime.UtcNow;

                        if (DateTime.UtcNow - _repairDisabledSince.Value > RepairDisabledGrace)
                        {
                            Fail("The mender's pay-to-repair button stayed disabled. You most likely don't have enough gil to repair");
                            return true;
                        }

                        if (EzThrottler.Throttle("Mender Repair Disabled Wait"))
                            IceLogging.Debug("The repair button is disabled, waiting a moment", Tag);
                    }
                }

                return true;
            }

            if (_step == Step.Interact)
            {
                if (TrySelectRepairMenu("SelectString") || TrySelectRepairMenu("SelectIconString"))
                    return true;

                // Menders that open a plain talk window first
                if (DateTime.UtcNow - _interactedAt < TimeSpan.FromSeconds(10)
                    && GenericHelpers.TryGetAddonByName<AtkUnitBase>("Talk", out var talk)
                    && GenericHelpers.IsAddonReady(talk))
                {
                    if (EzThrottler.Throttle("Mender Repair Talk", 500))
                        talk->FireCallbackInt(0);

                    return true;
                }
            }

            return false;
        }

        private static unsafe bool TrySelectRepairMenu(string addonName)
        {
            if (!GenericHelpers.TryGetAddonByName<AtkUnitBase>(addonName, out var addon)
                || !GenericHelpers.IsAddonReady(addon))
                return false;

            var menu = addonName == "SelectIconString"
                           ? &((AddonSelectIconString*)addon)->PopupMenu.PopupMenu
                           : &((AddonSelectString*)addon)->PopupMenu.PopupMenu;

            for (var i = 0; i < menu->EntryCount; i++)
            {
                var text = menu->EntryNames[i].ToString() ?? string.Empty;

                if (!text.Contains("repair", StringComparison.OrdinalIgnoreCase)
                    && !text.Contains("mend", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (EzThrottler.Throttle($"Mender Repair Menu {addonName}", 500))
                {
                    IceLogging.Debug($"Selecting the mender menu entry: {text}", Tag);
                    addon->FireCallbackInt(i);
                }

                return true;
            }

            return false;
        }

        private static unsafe bool CloseRepairWindows()
        {
            var open = false;

            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("Repair", out var repair)
                && GenericHelpers.IsAddonReady(repair))
            {
                open = true;

                if (EzThrottler.Throttle("Mender Repair Close Repair", 500))
                    Ecom_Handler.CloseWindow(repair);
            }

            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("RepairAuto", out var repairAuto)
                && GenericHelpers.IsAddonReady(repairAuto))
            {
                open = true;

                if (EzThrottler.Throttle("Mender Repair Close RepairAuto", 500))
                    Ecom_Handler.CloseWindow(repairAuto);
            }

            if (GenericHelpers.TryGetAddonMaster<SelectYesno>(out var yesNo)
                && yesNo.IsAddonReady)
            {
                open = true;

                if (EzThrottler.Throttle("Mender Repair Close Yesno", 500))
                    Ecom_Handler.CloseWindow(yesNo.Base);
            }

            return open;
        }

        private static unsafe bool ClickNode(AtkUnitBase* addon, uint nodeId)
        {
            var node = addon->GetNodeById(nodeId);
            if (node == null)
                return false;

            var button = node->GetAsAtkComponentButton();
            if (button == null)
                return false;

            if (!button->IsEnabled || button->AtkResNode == null || !button->AtkResNode->IsVisible())
                return false;

            var owner = button->AtkComponentBase.OwnerNode;
            if (owner == null)
                return false;

            var evt = owner->AtkResNode.AtkEventManager.Event;
            while (evt != null && evt->State.EventType != AtkEventType.ButtonClick)
                evt = evt->NextEvent;

            if (evt == null)
                return false;

            addon->ReceiveEvent(evt->State.EventType, (int)evt->Param, evt);
            return true;
        }

        #endregion
    }
}
