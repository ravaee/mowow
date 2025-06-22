using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Text.Json;
using AzerothCoreIntegration.Services;

public class RegisterModel : PageModel
{
    private readonly AzerothCoreSoapClient _soapClient;
    private readonly SiteUserService _siteUserService;
    private readonly string _captchaSecret;

    // a single static HttpClient is OK here
    private static readonly HttpClient _http = new();

    public RegisterModel(
        AzerothCoreSoapClient soapClient,
        SiteUserService siteUserService,
        IConfiguration cfg)
    {
        _soapClient = soapClient;
        _siteUserService = siteUserService;
        _captchaSecret = cfg["CaptchaSecret"] ?? "";
    }

    /* ───────────── form fields ───────────── */
    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    [BindProperty] public string ConfirmPassword { get; set; } = "";

    /* token automatically posted by the widget */
    [BindProperty(Name = "g-recaptcha-response")]
    public string CaptchaToken { get; set; } = "";

    public string? SuccessMessage { get; set; }

    /* ───────────── GET ───────────── */
    public void OnGet() { }

    /* ───────────── POST ───────────── */
    public async Task<IActionResult> OnPostAsync()
    {
        /* 0️⃣  CAPTCHA ----------------------------------------------------- */
        if (!await CaptchaPassed())
        {
            ModelState.AddModelError("", "Captcha failed — please try again.");
            return Page();
        }

        /* 1️⃣ basic form validation --------------------------------------- */
        if (!ModelState.IsValid) return Page();

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ModelState.AddModelError("", "Email and password are required.");
            return Page();
        }

        if (!Email.Contains('@') || Email.Length > 256)
        {
            ModelState.AddModelError("", "Please provide a valid email address.");
            return Page();
        }

        if (Password != ConfirmPassword)
        {
            ModelState.AddModelError("", "Passwords do not match.");
            return Page();
        }

        /* 2️⃣ derive & validate game-account name ------------------------- */
        var gameAccountName = Email.Split('@')[0];

        if (gameAccountName.Length > 17)
        {
            ModelState.AddModelError("", "Email prefix (before @) is too long for the game account.");
            return Page();
        }

        if (!Regex.IsMatch(gameAccountName, "^[A-Za-z0-9_-]+$"))
        {
            ModelState.AddModelError("", "Email prefix contains invalid characters for the game account.");
            return Page();
        }

        if (await _siteUserService.IsEmailUsedAsync(Email))
        {
            ModelState.AddModelError("", "This email is already registered.");
            return Page();
        }

        /* 3️⃣ create game account via SOAP -------------------------------- */
        try
        {
            await _soapClient.CreateAccountAsync(gameAccountName, Password);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Game account creation failed: {ex.Message}");
            return Page();
        }

        /* 4️⃣ save to CMS DB --------------------------------------------- */
        await _siteUserService.InsertSiteUserAsync(Email, gameAccountName, Password);

        SuccessMessage = "Registration successful!";
        return Page();
    }

    /* ───────────── helper: verify reCAPTCHA ───────────── */
    private async Task<bool> CaptchaPassed()
    {
        if (string.IsNullOrWhiteSpace(CaptchaToken) || string.IsNullOrEmpty(_captchaSecret))
            return false;

        var content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string,string>("secret"  , _captchaSecret),
            new KeyValuePair<string,string>("response", CaptchaToken),
            new KeyValuePair<string,string>("remoteip", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "")
        ]);

        var resp = await _http.PostAsync(
            "https://www.google.com/recaptcha/api/siteverify", content);

        if (!resp.IsSuccessStatusCode) return false;

        using var json = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync());
        return json.RootElement.GetProperty("success").GetBoolean();
    }
}
