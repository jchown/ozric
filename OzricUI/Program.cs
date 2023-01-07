using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.FileProviders;
using OzricUI.Data;
using MudBlazor.Services;
using OzricEngine;
using OzricService;
using OzricUI.Hubs;
using OzricUI.Mock;

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

app.Run();
