using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace OzricEngine;

public static class Supervisor
{
    private static string? _token;

    public static async Task<JsonDocument> GetConfig()
    {
        return await SupervisorGetAPI("http://supervisor/addons/self/config");
    }
    
    private static async Task<JsonDocument> SupervisorGetAPI(string endpointUrl)
    {
        try
        {
            var token = GetSupervisorToken();
            
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", $"Bearer {token}");
            var response = await httpClient.GetAsync(endpointUrl);
            var json = await response.Content.ReadAsStringAsync();

            try
            {
                return JsonDocument.Parse(json);
            }
            catch (Exception e)
            {
                throw new RethrownException(e, $"while parsing JSON: {json}");
            }
        }
        catch (Exception e)
        {
            throw new RethrownException(e, $"while fetching {endpointUrl}");
        }
    }

    private static string GetSupervisorToken()
    {
        if (_token != null)
            return _token;

        _token = Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN");
        if (_token != null)
            return _token;
        
        throw new Exception("SUPERVISOR_TOKEN not set");
    }
}