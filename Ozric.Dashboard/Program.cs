using System.Reflection;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.FileProviders;
using Ozric.Dashboard.Data;
using MudBlazor.Services;
using Ozric.Engine;
using Ozric.Service;
using Ozric.Dashboard.Shared;

OzricConfig ozricConfig = new()
{
    Version = Assembly.GetAssembly(typeof(OzricConfig))?.GetName().Version?.ToString() ?? "Unknown"
};

Console.WriteLine($"Version:\n  {ozricConfig.Version}");

try
{
    var addonInfo = await Supervisor.GetAddonInfo();
    Console.WriteLine($"Addon Info:\n{Json.Prettify(addonInfo)}");

    if (!addonInfo.RootElement.TryGetProperty("data", out var data))
        throw new Exception("Expected 'data' property in Supervisor info");

    if (data.TryGetProperty("ingress_port", out var portProperty))
        ozricConfig.Port = portProperty.GetInt32();
    
    if (data.TryGetProperty("ingress_url", out var urlProperty))
        ozricConfig.Path = urlProperty.GetString() ?? "/";
}
catch (Exception e)
{
    Console.WriteLine($"Failed to get Supervisor config: {e.Message}");
}

Console.WriteLine($"Config:\n  URL = {ozricConfig.Path}\n  Port = {ozricConfig.Port}");

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

builder.WebHost.ConfigureKestrel(kestrelOptions => kestrelOptions.ListenAnyIP(ozricConfig.Port));

// Add services to the container.
builder.Services.Configure<JsonOptions>(options => Json.Configure(options.SerializerOptions));
builder.Services.AddSignalR().AddJsonProtocol(options => Json.Configure(options.PayloadSerializerOptions));
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();
builder.Services.AddSingleton(ozricConfig);
builder.Services.AddSingleton<DataService>();
builder.Services.AddScoped<CookieProvider>();
builder.Services.AddMudServices();
builder.Services.AddResponseCompression(opts => opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/octet-stream"]));

builder.WebHost.UseSentry(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.Dsn = "";
        return;
    }

    options.Release = $"ozric@{ozricConfig.Version}";
    options.Dsn = "https://349904e9528eefef3e076a1a8c329987@o4506172979806208.ingest.sentry.io/4506172982755328";
    options.Debug = true;
    options.TracesSampleRate = 1.0;
    options.AutoSessionTracking = true;
});

IOzricService ozricEngine = new Ozric.Service.OzricService();
await ozricEngine.Start(CancellationToken.None);
builder.Services.AddSingleton<IOzricService>(_ => ozricEngine);
builder.Services.AddHttpContextAccessor();

var app = builder.Build();
app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

// Intercept 404s with a diagnostic body so ingress / base-path issues are visible
// in the browser instead of a blank "404 Not Found".
app.UseStatusCodePages(async context =>
{
    var http = context.HttpContext;
    if (http.Response.StatusCode != StatusCodes.Status404NotFound)
        return;

    var req = http.Request;
    http.Response.ContentType = "text/plain; charset=utf-8";

    var sb = new System.Text.StringBuilder();
    sb.AppendLine("404 - Ozric Dashboard could not match this request to a route or static file.");
    sb.AppendLine();
    sb.AppendLine("Request:");
    sb.AppendLine($"  Method      = {req.Method}");
    sb.AppendLine($"  Scheme      = {req.Scheme}");
    sb.AppendLine($"  Host        = {req.Host}");
    sb.AppendLine($"  PathBase    = {req.PathBase}");
    sb.AppendLine($"  Path        = {req.Path}");
    sb.AppendLine($"  QueryString = {req.QueryString}");
    sb.AppendLine();
    sb.AppendLine("Configured:");
    sb.AppendLine($"  Base path   = {ozricConfig.Path}");
    sb.AppendLine($"  Port        = {ozricConfig.Port}");
    sb.AppendLine($"  Version     = {ozricConfig.Version}");
    sb.AppendLine();
    sb.AppendLine("Headers:");
    foreach (var (name, values) in req.Headers)
        sb.AppendLine($"  {name} = {values}");

    await http.Response.WriteAsync(sb.ToString());
});

app.UseStaticFiles(staticFileOptions);
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

DataService.Map(app);

app.Run();