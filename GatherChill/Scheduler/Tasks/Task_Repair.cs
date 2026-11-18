using GatherChill.Enums;

namespace GatherChill.Scheduler.Tasks
{
    /// <summary>
    /// Picks the configured repair method before heading to a node.
    /// </summary>
    internal static class Task_Repair
    {
        public static bool Enqueue()
        {
            return C.Repair_Mode switch
            {
                RepairMode.Self => Task_SelfRepair.Enqueue(),
                RepairMode.Mender => Task_MenderRepair.Enqueue(),
                _ => true,
            };
        }
    }
}
