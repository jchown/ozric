using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.FileProviders;
using OzricUI.Data;
using MudBlazor.Services;
using OzricEngine;
using OzricService;
using OzricUI.Hubs;
using OzricUI.Mock;

OzricConfig ozricConfig = new();

try
{
    JsonDocument addonInfo = await Supervisor.GetAddonInfo();
    Console.WriteLine($"Addon Info:\n{Json.Prettify(addonInfo)}");

    if (!addonInfo.RootElement.TryGetProperty("data", out var data))
        throw new Exception("Expected 'data' property in Supervisor info");

    if (data.TryGetProperty("ingress_port", out var portProperty))
        ozricConfig.port = portProperty.GetInt32();
    
    if (data.TryGetProperty("ingress_url", out var urlProperty))
        ozricConfig.url = urlProperty.GetString() ?? "/";
}
catch (Exception e)
{
    Console.WriteLine($"Failed to get Supervisor config: {e.Message}");
}

Console.WriteLine($"Config\n  URL = {ozricConfig.url}\n  Port = {ozricConfig.port}");

const string dockerWwwRoot = "/ozric/wwwroot";
StaticFileOptions? staticFileOptions;

Console.WriteLine("Environment:");
var env = Environment.GetEnvironmentVariables();
foreach (var key in env.Keys)
    Console.WriteLine($"  {key} = {env[key]}");

if (Directory.Exists(dockerWwwRoot))
{
    Console.WriteLine($"Static files: {dockerWwwRoot}");

    staticFileOptions = new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(dockerWwwRoot),
        RequestPath = ""
    };
}
else
{
    Console.WriteLine("Static files: default");

    staticFileOptions = new StaticFileOptions();
}

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(kestrelOptions => kestrelOptions.ListenAnyIP(ozricConfig.port));

// Add services to the container.
builder.Services.Configure<JsonOptions>(options => Json.Configure(options.SerializerOptions));
builder.Services.AddSignalR().AddJsonProtocol(options => Json.Configure(options.PayloadSerializerOptions));
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();
builder.Services.AddSingleton(ozricConfig);
builder.Services.AddSingleton<DataService>();
builder.Services.AddSingleton<HomeHubController>();
builder.Services.AddMudServices();
builder.Services.AddResponseCompression(opts => opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" }));
builder.WebHost.UseSentry(options =>
{
    if (builder.Environment.IsDevelopment())
        return;
    
    options.Release = "ozric@0.10.11";
    options.Dsn = "https://349904e9528eefef3e076a1a8c329987@o4506172979806208.ingest.sentry.io/4506172982755328";
    options.Debug = true;
    options.TracesSampleRate = 1.0;
    options.AutoSessionTracking = true;
});

IEngineService ozricEngine = (Environment.GetEnvironmentVariable("OZRIC_MOCK") != null) ? new MockOzricService() : new EngineService();
await ozricEngine.Start(CancellationToken.None);
builder.Services.AddSingleton<IEngineService>(_ => ozricEngine);
builder.Services.AddSingleton<IOzricService>(_ => ozricEngine);
builder.Services.AddHttpContextAccessor();

var app = builder.Build();
app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles(staticFileOptions);
app.UseStaticFiles();  // https://stackoverflow.com/questions/58088302/blazor-server-js-file-not-found
app.UseRouting();

app.MapBlazorHub($"{ozricConfig.url}_blazor");
app.MapHub<HomeHub>(HomeHub.ENDPOINT);
app.Services.GetService<IHomeHubController>();
app.MapFallbackToPage("/_Host");

API.Map(app);
DataService.Map(app);

app.Run();