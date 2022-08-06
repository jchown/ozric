using System.Text;
using Microsoft.Net.Http.Headers;
using OzricEngine;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace OzricUI.Data;

public class DataService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DataService> _logger;
    
    private const string CLIENT_USER_AGENT = "OzricUI";

    public DataService(IHttpClientFactory httpClientFactory, ILogger<DataService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public string? Debug { get; set; } = null;//"kitchen-light-2";
    
    public async Task<GraphLayout> GetGraphLayoutAsync()
    {
        try
        {
            var json = await File.ReadAllTextAsync("/data/graph_layout.json");
            var graphLayout = Json.Deserialize<GraphLayout>(json);

            ShowDebug("Load", graphLayout);
            
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
        ShowDebug("Save", graphLayout);

        var json = Json.Serialize(graphLayout);
        await File.WriteAllTextAsync("/data/graph_layout.json", json);
    }

    private void ShowDebug(string save, GraphLayout graphLayout)
    {
        if (Debug == null)
            return;

        if (!graphLayout.nodeLayout.ContainsKey(Debug))
        {
            _logger.Log(LogLevel.Information, "No key for {0} during graph {1}", Debug, save);
            return;
        }

        var pos = graphLayout.nodeLayout[Debug];
        _logger.Log(LogLevel.Information, "{0}.position = {1} during graph {1}", Debug, pos, save);
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