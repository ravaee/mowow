using Microsoft.AspNetCore.Mvc.RazorPages;
using AzerothCoreIntegration.Models;
using AzerothCoreIntegration.Services;

public class IndexModel : PageModel
{
    private readonly AzerothCoreSoapClient _soapClient;
    private readonly CharacterStatisticsService _dbService;
    private readonly IConfiguration _config;

    public ServerInfoResult? ServerInfo { get; set; }
    public FactionStatisticsModel? FactionStats { get; set; }
    public LastBossKillModel? LastBossKill { get; set; }
    public string RealmAddress { get; set; } = string.Empty;

    public IndexModel(
        AzerothCoreSoapClient soapClient,
        CharacterStatisticsService dbService,
        IConfiguration config)
    {
        _soapClient = soapClient;
        _dbService = dbService;
        _config = config;
    }

    public async Task OnGetAsync()
    {
        RealmAddress = _config["AzerothCore:RealmAddress"] ?? "Unknown";

        // ServerInfo = await _soapClient.GetParsedServerInfoAsync();
        FactionStats = await _dbService.GetFactionStatisticsAsync();
        LastBossKill = await _dbService.GetLastBossKillAsync();
    }
}
