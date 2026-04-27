using Ozric.Dashboard.Shared;
using Ozric.Engine.Live;
using Ozric.Engine.Utils;
using Ozric.Service;
using Ozric.Engine;
using LogLevel = Ozric.Engine.Utils.LogLevel;

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
                layout = dataService.GetGraphLayout(),
                settings = dataService.GetSettings()
            };

            return Json.Serialize(data, pretty: true);
        });

        app.MapGet("/api/ha-image", async (string url, HttpContext context) =>
        {
            var baseUrl = HaConnectionInfo.BaseHttpUrl;
            var token = HaConnectionInfo.Token;
            if (baseUrl == null || token == null)
            {
                context.Response.StatusCode = 503;
                return;
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{baseUrl}{url}");
            if (!response.IsSuccessStatusCode)
            {
                context.Response.StatusCode = (int)response.StatusCode;
                return;
            }

            var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/png";
            context.Response.ContentType = contentType;
            context.Response.Headers.CacheControl = "public, max-age=3600";

            await response.Content.CopyToAsync(context.Response.Body);
        });
    }

    private static readonly Logger _log = new(nameof(DataService));

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DataService> _logger;
    private readonly string _layoutFilename;
    private readonly string _settingsFilename;
    
    private GraphLayout _currentGraphLayout;
    private Settings _currentSettings;

    private const string ClientUserAgent = "Ozric";

    public DataService(IHttpClientFactory httpClientFactory, ILogger<DataService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _layoutFilename = $"{Storage.RootPath}/graph_layout.json";
        _settingsFilename = $"{Storage.RootPath}/settings.json";
        
        try
        {
            var json = File.ReadAllText(_layoutFilename);
            _currentGraphLayout = Json.Deserialize<GraphLayout>(json);
        }
        catch (Exception e)
        {
            _log.Log(LogLevel.Warning, "Failed to load graph layout: {0}", e.Message);
            _currentGraphLayout = new GraphLayout();
        }
        
        try
        {
            var json = File.ReadAllText(_settingsFilename);
            _currentSettings = Json.Deserialize<Settings>(json);
        }
        catch (Exception e)
        {
            _log.Log(LogLevel.Warning, "Failed to load settings: {0}", e.Message);
            _currentSettings = new Settings();
        }
    }
    
    public GraphLayout GetGraphLayout()
    {
        return _currentGraphLayout;
    }

    public async Task SetGraphLayoutAsync(GraphLayout graphLayout)
    {
        _currentGraphLayout = graphLayout;
        var json = Json.Prettify(Json.Serialize(graphLayout));
        await File.WriteAllTextAsync(_layoutFilename, json);
    }
    
    public Settings GetSettings()
    {
        return _currentSettings;
    }
    
    public async Task SetSettingsAsync(Settings settings)
    {
        _currentSettings = settings;
        var json = Json.Prettify(Json.Serialize(settings));
        await File.WriteAllTextAsync(_settingsFilename, json);
    }
}