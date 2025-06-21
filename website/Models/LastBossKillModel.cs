namespace AzerothCoreIntegration.Models
{
    public class LastBossKillModel
    {
        public string CharacterName { get; set; } = string.Empty;
        public string AchievementName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
