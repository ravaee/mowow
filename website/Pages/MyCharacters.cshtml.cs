using Microsoft.AspNetCore.Mvc.RazorPages;
using AzerothCoreIntegration.Services;
using Microsoft.AspNetCore.Authorization;
using AzerothCoreIntegration.Models;

[Authorize]
public class MyCharactersModel : PageModel
{
    private readonly CharacterService _characterService;
    private readonly SiteUserService _siteUserService;

    public List<CharacterModel> Characters { get; set; } = new();

    public MyCharactersModel(CharacterService characterService, SiteUserService siteUserService)
    {
        _characterService = characterService;
        _siteUserService = siteUserService;
    }

    public async Task OnGetAsync()
    {
        string? email = User.Identity?.Name;
        if (string.IsNullOrEmpty(email))
            return;

        var siteUser = await _siteUserService.GetByEmailAsync(email);
        if (siteUser == null)
            return;

        var accountId = await _characterService.GetAccountIdAsync(siteUser.GameAccountName);
        if (accountId == null)
            return;

        Characters = await _characterService.GetCharactersAsync(accountId.Value);

        foreach (var c in Characters)
        {
            c.Achievements = await _characterService.GetAchievementsAsync(c.Guid);
            c.Reputations = await _characterService.GetReputationsAsync(c.Guid);
        }
    }


}
