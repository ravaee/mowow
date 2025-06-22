using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using AzerothCoreIntegration.Models;

namespace AzerothCoreIntegration.Services;

public class AzerothCoreSoapClient
{
    private readonly HttpClient _http;
    private readonly string _user;
    private readonly string _pass;

    public AzerothCoreSoapClient(HttpClient http, IConfiguration cfg)
    {
        _http = http;
        _http.BaseAddress = new Uri(cfg["AzerothCore:SoapUrl"]);
        _user = cfg["AzerothCore:SoapUser"];
        _pass = cfg["AzerothCore:SoapPassword"];
    }

    /*──────────────────────────── core helper ────────────────────────────*/
    private async Task<string> SendAsync(string command)
    {
        var envelope = $"""
            <?xml version="1.0" encoding="utf-8"?>
            <SOAP-ENV:Envelope xmlns:SOAP-ENV="http://schemas.xmlsoap.org/soap/envelope/">
              <SOAP-ENV:Body>
                <ns1:executeCommand xmlns:ns1="urn:AC">
                  <command>{command}</command>
                </ns1:executeCommand>
              </SOAP-ENV:Body>
            </SOAP-ENV:Envelope>
            """;

        var req = new HttpRequestMessage(HttpMethod.Post, "")
        {
            Content = new StringContent(envelope, Encoding.UTF8, "text/xml")
        };

        var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_user}:{_pass}"));
        req.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);

        var res = await _http.SendAsync(req);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsStringAsync();
    }

    /*──────────────────────────── account ops ────────────────────────────*/
    public async Task CreateAccountAsync(string user, string pwd)
    {
        var rsp = await SendAsync($"account create {user} {pwd} {pwd}");
        if (!rsp.Contains("Account created", StringComparison.OrdinalIgnoreCase))
            throw new Exception("AzerothCore: account creation failed.");
    }

    public async Task ChangePasswordAsync(string user, string newPwd)
    {
        var rsp = await SendAsync($"account set password {user} {newPwd} {newPwd}");
        if (!rsp.Contains("Password updated", StringComparison.OrdinalIgnoreCase))
            throw new Exception("AzerothCore: password change failed.");
    }

    /*──────────────────────────── server-info ────────────────────────────*/
    private static readonly Regex _regexNum = new(@"(\d+)", RegexOptions.Compiled);

    public async Task<ServerInfoResult?> GetParsedServerInfoAsync()
    {
        string xml;
        try { xml = await SendAsync("server info"); }
        catch { return null; }   // SOAP offline

        // SOAP returns the cmd output between <result> … </result>.
        var body = Regex.Match(xml, @"<result[^>]*>(.*?)</result>", RegexOptions.Singleline)
                        .Groups[1].Value
                        .Replace("\r", "")
                        .Trim();

        if (string.IsNullOrEmpty(body)) return null;

        var lines = body.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                        .Select(l => l.Trim()).ToList();

        var info = new ServerInfoResult { RawText = body };

        foreach (var line in lines)
        {
            if (line.StartsWith("Players online", StringComparison.OrdinalIgnoreCase))
                info.ConnectedPlayers = ParseInt(line);
            else if (line.StartsWith("Characters in world", StringComparison.OrdinalIgnoreCase))
                info.CharactersInWorld = ParseInt(line);
            else if (line.StartsWith("Connection peak", StringComparison.OrdinalIgnoreCase))
                info.ConnectionPeak = ParseInt(line);
            else if (line.StartsWith("Server uptime", StringComparison.OrdinalIgnoreCase) ||
                     line.StartsWith("Active uptime", StringComparison.OrdinalIgnoreCase))
                info.Uptime = line[(line.IndexOf(':') + 1)..].Trim();
        }

        return info;
    }

    private static int ParseInt(string line)
    {
        var m = _regexNum.Match(line);
        return m.Success ? int.Parse(m.Value) : 0;
    }
}
