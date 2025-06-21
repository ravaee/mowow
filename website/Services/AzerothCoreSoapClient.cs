using System.Net.Http.Headers;
using System.Text;


namespace AzerothCoreIntegration.Services;

public class AzerothCoreSoapClient
{
    private readonly HttpClient _httpClient;
    private readonly string _soapUser;
    private readonly string _soapPassword;

    public AzerothCoreSoapClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(config["AzerothCore:SoapUrl"]);
        _soapUser = config["AzerothCore:SoapUser"];
        _soapPassword = config["AzerothCore:SoapPassword"];
    }

    private async Task<string> SendSoapCommandAsync(string command)
    {
        var soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
            <SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"">
            <SOAP-ENV:Body>
            <ns1:executeCommand xmlns:ns1=""urn:AC"">
                <command>{command}</command>
            </ns1:executeCommand>
            </SOAP-ENV:Body>
            </SOAP-ENV:Envelope>";

        var request = new HttpRequestMessage(HttpMethod.Post, "")
        {
            Content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml")
        };

        var authBytes = Encoding.ASCII.GetBytes($"{_soapUser}:{_soapPassword}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return responseContent;
    }

    public async Task CreateAccountAsync(string username, string password)
    {
        var response = await SendSoapCommandAsync($"account create {username} {password} {password}");
        if (!response.Contains("Account created"))
            throw new Exception("Failed to create account in AzerothCore");
    }

    public async Task ChangePasswordAsync(string username, string newPassword)
    {
        var response = await SendSoapCommandAsync($"account set password {username} {newPassword} {newPassword}");
        if (!response.Contains("Password updated"))
            throw new Exception("Failed to update password in AzerothCore");
    }
}

