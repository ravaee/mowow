using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;

public class LoginModel : PageModel
{
    private readonly AuthenticationService _authService;
    private readonly string _captchaSecret;
    private static readonly HttpClient _http = new();

    public LoginModel(AuthenticationService authService, IConfiguration cfg)
    {
        _authService = authService;
        _captchaSecret = cfg["CaptchaSecret"] ?? "";
    }

    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    [BindProperty(Name = "g-recaptcha-response")]
    public string CaptchaToken { get; set; } = "";

    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!await CaptchaOk()) { ErrorMessage = "Captcha failed."; return Page(); }

        if (await _authService.ValidateLoginAsync(Email, Password))
        {
            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, Email) }, "MyCookieAuth"));

            await HttpContext.SignInAsync("MyCookieAuth", principal);
            return RedirectToPage("/Index");
        }

        ErrorMessage = "Invalid email or password.";
        return Page();
    }

    private async Task<bool> CaptchaOk()
    {
        if (string.IsNullOrWhiteSpace(CaptchaToken) || string.IsNullOrEmpty(_captchaSecret))
            return false;

        var data = new FormUrlEncodedContent(
        [
            new KeyValuePair<string,string> ("secret"  , _captchaSecret),
            new KeyValuePair<string,string> ("response", CaptchaToken),
            new KeyValuePair<string,string> ("remoteip", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "")
        ]);

        var resp = await _http.PostAsync(
            "https://www.google.com/recaptcha/api/siteverify", data);

        if (!resp.IsSuccessStatusCode) return false;

        using var json = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync());
        return json.RootElement.GetProperty("success").GetBoolean();
    }
}
