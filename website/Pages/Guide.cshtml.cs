using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

public class GuideModel : PageModel
{
    public string DownloadUrl { get; private set; } = "";
    public string RealmAddress { get; private set; } = "";

    private readonly IConfiguration _cfg;
    public GuideModel(IConfiguration cfg) => _cfg = cfg;

    public void OnGet()
    {
        DownloadUrl = _cfg["DownloadGameUrl"] ?? "#";
        RealmAddress = _cfg["AzerothCore:RealmAddress"];
    }
}