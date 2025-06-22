using Microsoft.AspNetCore.Mvc.RazorPages;
using AzerothCoreIntegration.Models;
using AzerothCoreIntegration.Services;

public class IndexModel : PageModel
{
    private readonly AzerothCoreSoapClient _soap;
    private readonly CharacterStatisticsService _stats;
    private readonly CharacterService _chars;
    private readonly SiteUserService _users;
    private readonly IConfiguration _cfg;

    public List<CharacterModel> Characters { get; set; } = new();
    public ServerInfoResult? ServerInfo { get; set; }
    public FactionStatisticsModel? FactionStats { get; set; }
    public LastBossKillModel? LastBossKill { get; set; }
    public string RealmAddress { get; set; } = "";

    public IndexModel(
        AzerothCoreSoapClient soap,
        CharacterStatisticsService stats,
        CharacterService chars,
        SiteUserService users,
        IConfiguration cfg)
    {
        _soap = soap;
        _stats = stats;
        _chars = chars;
        _users = users;
        _cfg = cfg;
    }

    public async Task OnGetAsync()
    {
        RealmAddress = _cfg["AzerothCore:RealmAddress"] ?? "Unknown";

        //  world-stats (+5-7 adjusted in model / service)
        FactionStats = await _stats.GetFactionStatisticsAsync();

        //  server uptime / player count (optional)
        try { ServerInfo = await _soap.GetParsedServerInfoAsync(); }
        catch { /* SOAP offline – just ignore */ }

        LastBossKill = await _stats.GetLastBossKillAsync();

        if (!User.Identity.IsAuthenticated) return;

        var email = User.Identity.Name!;
        var user = await _users.GetByEmailAsync(email);
        if (user == null) return;

        var accountId = await _chars.GetAccountIdAsync(user.GameAccountName);
        if (accountId == null) return;

        Characters = await _chars.GetCharactersAsync(accountId.Value);

        foreach (var c in Characters)
        {
            c.Achievements = await _chars.GetAchievementsAsync(c.Guid);
            c.Reputations = await _chars.GetReputationsAsync(c.Guid);
        }
    }
}
