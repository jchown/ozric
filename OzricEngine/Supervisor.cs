using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OzricEngine;

public static class Supervisor
{
    public static async Task<string> GetAddons()
    {
        var token = Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN") ?? throw new Exception("SUPERVISOR_TOKEN not set");
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        var response = await httpClient.GetAsync("http://supervisor/addons");
        var json = await response.Content.ReadAsStringAsync();
        //var data = Json.Deserialize<DownloadData>(json);
        return json;
    }

    public static async Task<string> GetInfo()
    {
        var token = Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN") ?? throw new Exception("SUPERVISOR_TOKEN not set");
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        var response = await httpClient.GetAsync("http://supervisor/addons/ozric/info");
        var json = await response.Content.ReadAsStringAsync();
        //var data = Json.Deserialize<DownloadData>(json);
        return json;
    }

    public static async Task<string> GetConfig()
    {
        var token = Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN") ?? throw new Exception("SUPERVISOR_TOKEN not set");
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        var response = await httpClient.GetAsync("http://supervisor/addons/self/config");
        var json = await response.Content.ReadAsStringAsync();
        //var data = Json.Deserialize<DownloadData>(json);
        return json;
    }
}