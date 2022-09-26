using Microsoft.AspNetCore.Http.Json;
using OzricUI.Data;
using MudBlazor.Services;
using OzricEngine;
using OzricService;
using OzricUI.Mock;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<DataService>();
builder.Services.AddMudServices();
builder.Services.Configure<JsonOptions>(options => Json.Configure(options.SerializerOptions));

IEngineService engine = (Environment.GetEnvironmentVariable("OZRIC_MOCK") != null) ? new MockEngineService() : new EngineService();
await engine.Start(CancellationToken.None);
builder.Services.AddSingleton(_ => engine);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

API.Map(app);

app.Run();
