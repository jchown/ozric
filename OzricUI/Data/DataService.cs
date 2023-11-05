using System.Text;
using System.Text.Json;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using OzricEngine;
using OzricService;
using Sentry;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace OzricUI.Data;

public class DataService
{
    public class DownloadData
    {
        public Graph graph;
        public GraphLayout layout;
    };
    
    public static void Map(WebApplication app)
    {
        var dataService = app.Services.GetService<DataService>()!;
        var engineService = app.Services.GetService<IEngineService>()!;
        
        app.MapGet("/api/download", async () =>
        {
            var data = new DownloadData
            {
                graph = engineService.Graph,
                layout = await dataService.GetGraphLayoutAsync(),
            };
            //return Results.Json(data, new JsonSerializerOptions { WriteIndented = true } );
            return data;
        });
    }

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DataService> _logger;
    
    private const string CLIENT_USER_AGENT = "Spike";

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

        var json = Json.Prettify(Json.Serialize(graphLayout));
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
    
    private async Task Put<TObject>(string apiPath, TObject entity) where TObject: class
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