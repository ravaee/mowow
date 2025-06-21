namespace AzerothCoreIntegration.Models;

public class SiteUser
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string GameAccountName { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

