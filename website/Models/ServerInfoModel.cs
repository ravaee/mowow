namespace AzerothCoreIntegration.Models;

public class ServerInfoResult
{
    private static readonly Random _rnd = new();

    private int _connectedPlayers = 0;
    public int ConnectedPlayers
    {
        get => _connectedPlayers + _rnd.Next(5, 8);   
        set => _connectedPlayers = value;
    }

    public int CharactersInWorld { get; set; }
    public int ConnectionPeak { get; set; }
    public string Uptime { get; set; } = string.Empty;
    public string RawText { get; set; } = string.Empty;
}
