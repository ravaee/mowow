// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.RazorPages;
// using AzerothCoreIntegration.Models;
// using AzerothCoreIntegration.Services;

// public class ServerInfoPageModel : PageModel
// {
//     private readonly AzerothCoreSoapClient _soapClient;

//     public ServerInfoResult? ServerInfo { get; set; }

//     public ServerInfoPageModel(AzerothCoreSoapClient soapClient)
//     {
//         _soapClient = soapClient;
//     }

//     public async Task OnGetAsync()
//     {
//         ServerInfo = await _soapClient.GetParsedServerInfoAsync();
//     }

//     public async Task<IActionResult> OnPostAsync()
//     {
//         ServerInfo = await _soapClient.GetParsedServerInfoAsync();
//         return Page();
//     }
// }
