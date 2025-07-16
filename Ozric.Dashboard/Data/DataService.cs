using Ozric.Dashboard.Shared;
using Ozric.Service;
using OzricEngine;
using OzricService;

namespace Ozric.Dashboard.Data;

public class DataService
{
    public static void Map(WebApplication app)
    {
        var dataService = app.Services.GetService<DataService>()!;
        var engineService = app.Services.GetService<IOzricService>()!;
        
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
    private readonly string _filename;

    private const string ClientUserAgent = "Spike";

    public DataService(IHttpClientFactory httpClientFactory, ILogger<DataService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _filename = $"{Storage.RootPath}/graph_layout.json";
    }
    
    public async Task<GraphLayout> GetGraphLayoutAsync()
    {
        try
        {
            var json = await File.ReadAllTextAsync(_filename);
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
        var json = Json.Prettify(Json.Serialize(graphLayout));
        await File.WriteAllTextAsync(_filename, json);
    }
}