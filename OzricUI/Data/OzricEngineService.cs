using System.Text;
using Microsoft.Net.Http.Headers;
using OzricEngine;
using OzricEngine.logic;

namespace OzricUI.Data;

public class OzricEngineService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string CLIENT_USER_AGENT = "OzricUI";

    public OzricEngineService(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public Task<Home> GetHomeAsync()
    {
        return Get<Home>("api/home");
    }

    public Task<Graph> GetGraphAsync()
    {
        return Get<Graph>("api/graph");
    }

    public Task SetGraphAsync(Graph g)
    {
        return Put<Graph>("api/graph", g);
    }

    public async Task<GraphLayout> GetGraphLayoutAsync()
    {
        try
        {
            var json = await File.ReadAllTextAsync("/data/graph_layout.json");
            var graphLayout = Json.Deserialize<GraphLayout>(json);
            return graphLayout;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load graph layout: {e.Message}");
            return new GraphLayout();
        }
    }

    public async Task SetGraphLayoutAsync(GraphLayout graphLayout)
    {
        var json = Json.Serialize(graphLayout);
        await File.WriteAllTextAsync("/data/graph_layout.json", json);
    }

    private async Task<TObject> Get<TObject>(string apiPath)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:8099/{apiPath}")
        {
            Headers =
            {
                { HeaderNames.Accept, "application/json" },
                { HeaderNames.UserAgent, CLIENT_USER_AGENT }
            }
        };

        var client = _httpClientFactory.CreateClient();
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Get request to {apiPath} failed: {response.StatusCode}/{response.ReasonPhrase}");

        return Json.Deserialize<TObject>(await response.Content.ReadAsStringAsync());
    }
    
    private async Task Put<TObject>(string apiPath, TObject entity)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"http://localhost:8099/{apiPath}")
        {
            Headers =
            {
                { HeaderNames.UserAgent, CLIENT_USER_AGENT }
            },
            Content = new StringContent(Json.Serialize(entity), Encoding.UTF8, "application/json")
        };

        var client = _httpClientFactory.CreateClient();
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Put request to {apiPath} failed: {response.StatusCode}/{response.ReasonPhrase}");
    }
}
