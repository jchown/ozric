using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OzricEngine;

public static class Supervisor
{
    public static async Task<string> GetConfig()
    {
        var token = Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN") ?? throw new Exception("SUPERVISOR_TOKEN not set");
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        var response = await httpClient.GetAsync("http://supervisor/addon/ozric/config");
        var json = await response.Content.ReadAsStringAsync();
        //var data = Json.Deserialize<DownloadData>(json);
        return json;
    }
}