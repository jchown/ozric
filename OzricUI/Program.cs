using Microsoft.AspNetCore.Http.Json;
using OzricUI.Data;
using MudBlazor.Services;
using OzricEngine;
using OzricService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<DataService>();
builder.Services.AddMudServices();
builder.Services.Configure<JsonOptions>(options => Json.Configure(options.SerializerOptions));

var app = builder.Build();

await Service.Instance.Start(CancellationToken.None);

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
