using System.Security.Cryptography;
using System.Text;
using AzerothCoreIntegration.Services;

public class AuthenticationService
{
    private readonly SiteUserService _siteUserService;

    public AuthenticationService(SiteUserService siteUserService)
    {
        _siteUserService = siteUserService;
    }

    public async Task<bool> ValidateLoginAsync(string email, string password)
    {
        var user = await _siteUserService.GetByEmailAsync(email);
        if (user == null) return false;

        return _siteUserService.VerifyPassword(password, user.PasswordHash);
    }
}
