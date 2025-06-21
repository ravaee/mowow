using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

public class LoginModel : PageModel
{
    private readonly AuthenticationService _authService;

    public LoginModel(AuthenticationService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (await _authService.ValidateLoginAsync(Email, Password))
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, Email) };
            var identity = new ClaimsIdentity(claims, "MyCookieAuth");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("MyCookieAuth", principal);
            return RedirectToPage("/Index");
        }

        ErrorMessage = "Invalid email or password.";
        return Page();
    }
}
