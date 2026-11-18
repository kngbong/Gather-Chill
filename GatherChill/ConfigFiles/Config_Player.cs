using GatherChill.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace GatherChill.ConfigFiles;

public partial class Config
{
    // Legacy setting, kept so old configs still deserialize. Migrated into Repair_Mode on load.
    public bool SelfRepair { get; set; } = false;
    public RepairMode Repair_Mode { get; set; } = RepairMode.None;
    public uint Mender_Aetheryte { get; set; } = 0;
    public int Minimum_DarkMatter { get; set; } = 11;
    public int Repair_Amount { get; set; } = 50;
    public bool Extract_Spiritbond { get; set; } = false;
}
