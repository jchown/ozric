using System.Collections;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.FileProviders;
using OzricEngine;
using OzricService;

const int homeAssistantIngressPort = 8099;
const string dockerWwwRoot = "/ozric/wwwroot";

var builder = WebApplication.CreateBuilder(args);

StaticFileOptions? staticFileOptions;
    
Console.WriteLine($"Web root: {builder.Environment.WebRootPath}");
Console.WriteLine($"Content root: {builder.Environment.ContentRootPath}");

Console.WriteLine("Environment:");
var env = Environment.GetEnvironmentVariables();
foreach (DictionaryEntry o in env)
    Console.WriteLine($"  {o.Key}={o.Value}");

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

builder.WebHost.ConfigureKestrel(kestrelOptions => kestrelOptions.ListenAnyIP(homeAssistantIngressPort));
builder.Services.Configure<JsonOptions>(options => Json.Configure(options.SerializerOptions));

await Service.Instance.Start(CancellationToken.None);

var app = builder.Build();

app.UseRewriter(new RewriteOptions().Add(Paths.RewritePagePaths));
app.UseDefaultFiles();
app.UseStaticFiles(staticFileOptions);
app.UseRouting();
app.UseDeveloperExceptionPage();

API.Map(app);
    
app.Run();