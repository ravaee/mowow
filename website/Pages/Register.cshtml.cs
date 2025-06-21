using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;
using AzerothCoreIntegration.Services;

public class RegisterModel : PageModel
{
    private readonly AzerothCoreSoapClient _soapClient;
    private readonly SiteUserService _siteUserService;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? SuccessMessage { get; set; }

    public RegisterModel(AzerothCoreSoapClient soapClient, SiteUserService siteUserService)
    {
        _soapClient = soapClient;
        _siteUserService = siteUserService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ModelState.AddModelError("", "Email and password are required.");
            return Page();
        }

        if (!Email.Contains("@") || Email.Length > 256)
        {
            ModelState.AddModelError("", "Please provide a valid email address.");
            return Page();
        }

        if (Password != ConfirmPassword)
        {
            ModelState.AddModelError("", "Passwords do not match.");
            return Page();
        }

        var gameAccountName = Email.Split('@')[0];
        if (gameAccountName.Length > 17)
        {
            ModelState.AddModelError("", "Email prefix (before @) is too long to create game account.");
            return Page();
        }

        // Validate game account name for AzerothCore rules
        if (!Regex.IsMatch(gameAccountName, "^[A-Za-z0-9_-]+$"))
        {
            ModelState.AddModelError("", "Email prefix contains invalid characters for game account.");
            return Page();
        }

        if (await _siteUserService.IsEmailUsedAsync(Email))
        {
            ModelState.AddModelError("", "This email is already registered.");
            return Page();
        }

        // First create game account
        try
        {
            await _soapClient.CreateAccountAsync(gameAccountName, Password);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Game account creation failed: {ex.Message}");
            return Page();
        }

        // Save full email into our CMS database
        await _siteUserService.InsertSiteUserAsync(Email, gameAccountName, Password);

        SuccessMessage = "Registration successful!";
        return Page();
    }
}