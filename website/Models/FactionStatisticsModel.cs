namespace AzerothCoreIntegration.Models
{
    public class FactionStatisticsModel
    {
        public int HordePlayers { get; set; }
        public int AlliancePlayers { get; set; }
        public int TotalPlayers => HordePlayers + AlliancePlayers;
    }
}
