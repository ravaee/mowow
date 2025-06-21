namespace AzerothCoreIntegration.Models
{
    public class ServerInfoResult
    {
        public int ConnectedPlayers { get; set; }
        public int CharactersInWorld { get; set; }
        public int ConnectionPeak { get; set; }
        public string Uptime { get; set; } = string.Empty;
        public string RawText { get; set; } = string.Empty; // Keep full original text if needed
    }
}
