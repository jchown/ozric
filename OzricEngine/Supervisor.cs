using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace OzricEngine;

public static class Supervisor
{
    private static string? token;

    public static async Task<JsonDocument> GetConfig()
    {
        return await SupervisorGetAPI("http://supervisor/addons/self/config");
    }
    
    private static async Task<JsonDocument> SupervisorGetAPI(string endpointURL)
    {
        var token = GetSupervisorToken();
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", $"Bearer {token}");
        var response = await httpClient.GetAsync(endpointURL);
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json);
    }

    private static string GetSupervisorToken()
    {
        if (token != null)
            return token;

        token = Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN");
        if (token != null)
            return token;
        
        throw new Exception("SUPERVISOR_TOKEN not set");
    }
}