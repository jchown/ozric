using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.FileProviders;
using OzricUI.Data;
using MudBlazor.Services;
using OzricEngine;
using OzricService;
using OzricUI.Hubs;
using OzricUI.Mock;
using Sentry;

const int homeAssistantIngressPort = 8099;

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

builder.WebHost.ConfigureKestrel(kestrelOptions => kestrelOptions.ListenAnyIP(homeAssistantIngressPort));

// Add services to the container.
builder.Services.Configure<JsonOptions>(options => Json.Configure(options.SerializerOptions));
builder.Services.AddSignalR().AddJsonProtocol(options => Json.Configure(options.PayloadSerializerOptions));
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<DataService>();
builder.Services.AddSingleton<HomeHubController>();
builder.Services.AddMudServices();
builder.Services.AddResponseCompression(opts => opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" }));
builder.WebHost.UseSentry(options =>
{
    if (builder.Environment.IsDevelopment())
        return;
    
    options.Release = "ozric@0.10.5";
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

app.MapBlazorHub();
app.MapHub<HomeHub>(HomeHub.ENDPOINT);
app.Services.GetService<IHomeHubController>();
app.MapFallbackToPage("/_Host");

API.Map(app);
DataService.Map(app);

try
{
    var json = await Supervisor.GetInfo();
    Console.WriteLine($"Supervisor info: {json}");
    SentrySdk.CaptureMessage(json, SentryLevel.Warning);
}
catch (Exception e)
{
    Console.WriteLine($"Failed to get Supervisor info: {e.Message}");
    SentrySdk.CaptureException(e);
}

try
{
    var json = await Supervisor.GetConfig();
    Console.WriteLine($"Supervisor config: {json}");
    SentrySdk.CaptureMessage(json, SentryLevel.Warning);
}
catch (Exception e)
{
    Console.WriteLine($"Failed to get Supervisor config: {e.Message}");
    SentrySdk.CaptureException(e);
}

try
{
    var json = await Supervisor.GetAddons();
    Console.WriteLine($"Supervisor addons: {json}");
    SentrySdk.CaptureMessage(json, SentryLevel.Warning);
}
catch (Exception e)
{
    Console.WriteLine($"Failed to get Supervisor addons: {e.Message}");
    SentrySdk.CaptureException(e);
}

app.Run();
