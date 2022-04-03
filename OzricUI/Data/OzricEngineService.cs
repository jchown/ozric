using Microsoft.Net.Http.Headers;
using OzricEngine;

namespace OzricUI.Data;

public class OzricEngineService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public OzricEngineService(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public Task<Graph> GetGraphAsync()
    {
        return Get<Graph>("api/graph");
    }

    private async Task<TObject> Get<TObject>(string apiPath)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:8099/{apiPath}")
        {
            Headers =
            {
                { HeaderNames.Accept, "application/json" },
                { HeaderNames.UserAgent, "OzricUI" }
            }
        };

        var client = _httpClientFactory.CreateClient();
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Request to {apiPath} failed: {response.StatusCode}/{response.ReasonPhrase}");

        return Json.Deserialize<TObject>(await response.Content.ReadAsStringAsync());
    }
}
