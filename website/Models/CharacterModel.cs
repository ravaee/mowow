
namespace AzerothCoreIntegration.Models;

public class CharacterModel
{
    public ulong Guid { get; set; }
    public string Name { get; set; } = "";
    public int Level { get; set; }
    public long Money { get; set; }
    public int Gold => (int)(Money / 10000);
    public int Silver => (int)((Money % 10000) / 100);
    public int Copper => (int)(Money % 100);
    public int XP { get; set; }
    public int Race { get; set; }
    public int Class { get; set; }
    public int Gender { get; set; }  // <-- ADD GENDER

    public string RaceName => Race switch
    {
        1 => "human",
        2 => "orc",
        3 => "dwarf",
        4 => "nightelf",
        5 => "undead",
        6 => "tauren",
        7 => "gnome",
        8 => "troll",
        10 => "bloodelf",
        11 => "draenei",
        _ => "unknown"
    };

    public string ClassName => Class switch
    {
        1 => "warrior",
        2 => "paladin",
        3 => "hunter",
        4 => "rogue",
        5 => "priest",
        6 => "deathknight",
        7 => "shaman",
        8 => "mage",
        9 => "warlock",
        10 => "monk", // if available
        _ => "unknown"
    };

    public string RaceImage => $"race_{RaceName}_{(Gender == 0 ? "male" : "female")}.jpg";
    public string ClassImage => $"{ClassName}.png";

    public List<AchievementModel> Achievements { get; set; }
    public List<ReputationModel> Reputations { get; set; }
}

public class AchievementModel
{
    public int AchievementId { get; set; }
    public DateTime Date { get; set; }
}

public class ReputationModel
{
    public int Faction { get; set; }
    public int Standing { get; set; }
}
