using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.ResponseCompression;
using OzricUI.Data;
using MudBlazor.Services;
using OzricEngine;
using OzricService;
using OzricUI.Hubs;
using OzricUI.Mock;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();
builder.Services.Configure<JsonOptions>(options => Json.Configure(options.SerializerOptions));
builder.Services.AddSignalR().AddJsonProtocol(options => Json.Configure(options.PayloadSerializerOptions));
builder.Services.AddSingleton<DataService>();
builder.Services.AddSingleton<HomeHubController>();
builder.Services.AddMudServices();
builder.Services.AddResponseCompression(opts => opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" }));

IEngineService engine = (Environment.GetEnvironmentVariable("OZRIC_MOCK") != null) ? new MockEngineService() : new EngineService();
await engine.Start(CancellationToken.None);
builder.Services.AddSingleton(_ => engine);

var app = builder.Build();
app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapHub<HomeHub>(HomeHub.ENDPOINT);
var hcc = app.Services.GetService<IHomeHubController>();
app.MapFallbackToPage("/_Host");

API.Map(app);

app.Run();
