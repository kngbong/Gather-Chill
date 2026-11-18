using System;
using System.Collections.Generic;
using System.Numerics;

namespace GatherChill.Utilities.Traveling;

/// <summary>
/// A town aetheryte that has a mender NPC nearby, plus the spot to stand on once we arrive.
/// Table is shared with RepairDeez.
/// </summary>
public sealed record MenderDestination(
    string Zone,
    string Name,
    uint Aetheryte,
    uint Territory,
    float X = 0f,
    float Y = 0f,
    float Z = 0f)
{
    public static readonly MenderDestination[] All =
    [
        new("Amh Araeng", "Mord Souq", 140, 815, 299.49f, 1.47f, -236.01f),
        new("Amh Araeng", "The Inn at Journey's Head", 161, 815, 299.49f, 1.47f, -236.01f),
        new("Amh Araeng", "Twine", 141, 815, 299.49f, 1.47f, -236.01f),
        new("Central Shroud", "Bentbranch Meadows", 3, 148, 16.20f, -8.04f, -15.61f),
        new("Central Thanalan", "Black Brush Station", 53, 141, -8.32f, -2.05f, -149.34f),
        new("Coerthas Central Highlands", "Camp Dragonhead", 23, 155, 237.59f, 222.21f, 320.05f),
        new("Coerthas Western Highlands", "Falcon's Nest", 71, 397, -297.90f, 126.79f, 2.13f),
        new("East Shroud", "The Hawthorne Hut", 4, 152, -213.93f, 2.33f, 300.44f),
        new("Eastern La Noscea", "Costa del Sol", 11, 137, -37.55f, 71.73f, -39.60f),
        new("Eastern La Noscea", "Wineport", 12, 137, -37.55f, 71.73f, -39.60f),
        new("Eastern Thanalan", "Camp Drybone", 18, 145, -406.29f, -53.70f, 95.15f),
        new("Elpis", "Anagnorisis", 176, 961, 146.40f, 10.39f, 164.42f),
        new("Elpis", "Poieten Oikos", 178, 961, 146.40f, 10.39f, 164.42f),
        new("Elpis", "The Twelve Wonders", 177, 961, 146.40f, 10.39f, 164.42f),
        new("Garlemald", "Camp Broken Glass", 172, 958, 492.82f, -36.31f, -192.98f),
        new("Garlemald", "Tertium", 173, 958, 492.82f, -36.31f, -192.98f),
        new("Heritage Found", "Electrope Strike", 212, 1191, -208.75f, 31.00f, 134.07f),
        new("Heritage Found", "The Outskirts", 211, 1191, -208.75f, 31.00f, 134.07f),
        new("Heritage Found", "Yyasulani Station", 210, 1191, -208.75f, 31.00f, 134.07f),
        new("Kholusia", "Stilltide", 137, 814, -223.99f, 21.23f, 372.00f),
        new("Kholusia", "Tomra", 139, 814, -223.99f, 21.23f, 372.00f),
        new("Kholusia", "Wright", 138, 814, -223.99f, 21.23f, 372.00f),
        new("Kozama'uka", "Dock Poga", 238, 1188, 591.64f, 116.84f, 220.63f),
        new("Kozama'uka", "Earthenshire", 204, 1188, -438.50f, 122.19f, 321.98f),
        new("Kozama'uka", "Many Fires", 203, 1188, 591.64f, 116.84f, 220.63f),
        new("Kozama'uka", "Ok'hanu", 202, 1188, 591.64f, 116.84f, 220.63f),
        new("Labyrinthos", "Aporia", 168, 956, 32.65f, -31.53f, -71.93f),
        new("Labyrinthos", "Sharlayan Hamlet", 167, 956, 32.65f, -31.53f, -71.93f),
        new("Labyrinthos", "The Archeion", 166, 956, 32.65f, -31.53f, -71.93f),
        new("Lakeland", "Fort Jobb", 132, 813, -140.57f, 14.62f, -129.05f),
        new("Lakeland", "The Ostall Imperative", 136, 813, -140.57f, 14.62f, -129.05f),
        new("Limsa Lominsa Lower Decks", "Limsa Lominsa Lower Decks", 8, 129, -397.63f, 3.10f, 81.01f),
        new("Lower La Noscea", "Moraby Drydocks", 10, 135, 554.44f, 89.03f, -49.80f),
        new("Mare Lamentorum", "Bestways Burrow", 175, 959, -19.82f, -132.95f, -456.50f),
        new("Mare Lamentorum", "Sinus Lacrimarum", 174, 959, -19.82f, -132.95f, -456.50f),
        new("Middle La Noscea", "Summerford Farms", 52, 134, 201.72f, 98.42f, -206.08f),
        new("Mor Dhona", "Revenant's Toll", 24, 156, 430.59f, -5.33f, -453.06f),
        new("North Shroud", "Fallgourd Float", 7, 154, 10.64f, -44.73f, 220.22f),
        new("Northern Thanalan", "Camp Bluefog", 21, 147, -3.22f, 43.13f, 32.68f),
        new("Northern Thanalan", "Ceruleum Processing Plant", 22, 147, -3.22f, 43.13f, 32.68f),
        new("Outer La Noscea", "Camp Overlook", 16, 180, 19.50f, 16.16f, -189.55f),
        new("Rhalgr's Reach", "Rhalgr's Reach", 104, 635, 78.45f, 0.00f, -2.94f),
        new("Shaaloani", "Hhusatahwi", 207, 1190, -151.40f, 2.09f, 274.80f),
        new("Shaaloani", "Mehwahhetsoan", 209, 1190, -151.40f, 2.09f, 274.80f),
        new("Shaaloani", "Sheshenewezi Springs", 208, 1190, -151.40f, 2.09f, 274.80f),
        new("South Shroud", "Camp Tranquil", 6, 153, 161.97f, 9.51f, -60.11f),
        new("South Shroud", "Quarrymill", 5, 153, 161.97f, 9.51f, -60.11f),
        new("Southern Thanalan", "Forgotten Springs", 20, 146, -278.80f, 7.98f, 378.87f),
        new("Southern Thanalan", "Little Ala Mhigo", 19, 146, 97.07f, 14.67f, -382.46f),
        new("Thavnair", "Palaka's Stand", 171, 957, 432.62f, 3.12f, -237.26f),
        new("Thavnair", "The Great Work", 170, 957, 432.62f, 3.12f, -237.26f),
        new("Thavnair", "Yedlihmad", 169, 957, 432.62f, 3.12f, -237.26f),
        new("The Azim Steppe", "Dhoro Iloh", 128, 622, 90.82f, 114.90f, 23.65f),
        new("The Azim Steppe", "Reunion", 109, 622, 90.82f, 114.90f, 23.65f),
        new("The Azim Steppe", "The Dawn Throne", 110, 622, 90.82f, 114.90f, 23.65f),
        new("The Churning Mists", "Moghome", 78, 400, -95.51f, -8.77f, 189.50f),
        new("The Churning Mists", "Zenith", 79, 400, -95.51f, -8.77f, 189.50f),
        new("The Doman Enclave", "The Doman Enclave", 127, 759, 45.39f, 0.03f, 24.82f),
        new("The Dravanian Forelands", "Anyx Trine", 77, 398, 479.40f, -51.14f, 24.66f),
        new("The Dravanian Forelands", "Tailfeather", 76, 398, 479.40f, -51.14f, 24.66f),
        new("The Fringes", "Castrum Oriens", 98, 612, 4.03f, 56.02f, 269.48f),
        new("The Fringes", "The Peering Stones", 99, 612, 4.03f, 56.02f, 269.48f),
        new("The Lochs", "Porta Praetoria", 102, 621, -545.81f, 8.65f, -1.31f),
        new("The Peaks", "Ala Gannha", 100, 620, 313.04f, 324.30f, 379.48f),
        new("The Peaks", "Ala Ghiri", 101, 620, 313.04f, 324.30f, 379.48f),
        new("The Rak'tika Greatwood", "Fanow", 143, 817, -87.72f, -19.01f, 283.41f),
        new("The Rak'tika Greatwood", "Slitherbough", 142, 817, -87.72f, -19.01f, 283.41f),
        new("The Ruby Sea", "Onokoro", 106, 613, -34.74f, -198.63f, -72.11f),
        new("The Ruby Sea", "Tamamizu", 105, 613, -34.74f, -198.63f, -72.11f),
        new("The Tempest", "The Macarenses Angle", 148, 818, 524.92f, 348.36f, -206.60f),
        new("The Tempest", "The Ondo Cups", 147, 818, 524.92f, 348.36f, -206.60f),
        new("Ultima Thule", "Abode of the Ea", 180, 960, 467.02f, 437.00f, 327.21f),
        new("Ultima Thule", "Base Omicron", 181, 960, 467.02f, 437.00f, 327.21f),
        new("Ultima Thule", "Reah Tahra", 179, 960, 467.02f, 437.00f, 327.21f),
        new("Upper La Noscea", "Camp Bronze Lake", 15, 139, 427.28f, 4.10f, 115.29f),
        new("Urqopacha", "Wachunpelo", 200, 1187, 420.76f, -116.82f, -197.36f),
        new("Urqopacha", "Worlar's Echo", 201, 1187, 420.76f, -116.82f, -197.36f),
        new("Western La Noscea", "Aleport", 14, 138, 55.57f, -3.12f, 65.66f),
        new("Western La Noscea", "Swiftperch", 13, 138, 55.57f, -3.12f, 65.66f),
        new("Western Thanalan", "Horizon", 17, 140, 207.17f, 52.04f, 134.20f),
        new("Wolves' Den Pier", "Wolves' Den Pier", 55, 250, 27.38f, 5.20f, -16.85f),
        new("Yak T'el", "Iq Br'aax", 205, 1189, -386.39f, 23.54f, -444.56f),
        new("Yanxia", "Namai", 107, 614, 472.95f, 68.01f, -102.22f),
        new("Yanxia", "The House of the Fierce", 108, 614, 472.95f, 68.01f, -102.22f),
    ];

    public Vector3 Position => new(X, Y, Z);

    public string Label => Name == Zone ? Zone : $"{Zone} - {Name}";

    /// <summary>One entry per aetheryte id, so the settings list has no duplicates.</summary>
    public static List<MenderDestination> Unique()
    {
        var seen = new HashSet<uint>();
        var result = new List<MenderDestination>();

        foreach (var destination in All)
        {
            if (seen.Add(destination.Aetheryte))
                result.Add(destination);
        }

        result.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.OrdinalIgnoreCase));
        return result;
    }

    public static MenderDestination? For(uint aetheryte)
    {
        foreach (var destination in All)
        {
            if (destination.Aetheryte == aetheryte)
                return destination;
        }

        return null;
    }

    public static bool IsAttuned(uint aetheryte)
    {
        foreach (var entry in Svc.AetheryteList)
        {
            if (entry.AetheryteId == aetheryte)
                return true;
        }

        return false;
    }
}
