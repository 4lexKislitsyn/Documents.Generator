using System.Reflection;
using System.Text.Json;
using Documents.Generator;
using Documents.Generator.Contracts;
using Documents.Generator.Grpc;
using Documents.Generator.Services;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using PuppeteerSharp;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
builder.Services.AddGrpc();
builder.Services.AddApiVersioning();
builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});
builder.Services.AddHealthChecks();
builder.Services.AddSwaggerGen(options =>
{
    var documentationFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, documentationFileName));
});
builder.Services.ConfigureOptions<ConfigureSwaggerApiVersionsOptions>();
builder.Services.Configure<SwaggerGenOptions>(options =>
{
    options.MapType<JsonDocument>(() => new OpenApiSchema
    {
        Type = "object",
        Default = new OpenApiObject(),
    });
});

// Add services to the container.
builder.Services.Configure<PdfOptions>(builder.Configuration.GetSection(nameof(PdfOptions)));
builder.Services.Configure<LaunchOptions>(builder.Configuration.GetSection(nameof(LaunchOptions)));
builder.Services.AddSingleton<ILiquidExecutor, FluidLiquidExecutor>();
builder.Services.AddSingleton<IPdfRenderer, PdfRenderer>();
builder.Services.AddSingleton<IDocumentGenerator, DocumentGeneratorService>();
builder.Services.AddControllers();
builder.Services.AddSingleton<BrowserInitializer>();
builder.Services.AddSingleton<IBrowser>(provider => provider.GetRequiredService<BrowserInitializer>().GetBrowser());

var app = builder.Build();

app.Logger.LogInformation("Configure the HTTP request pipeline");
app.MapGrpcService<PdfGeneratorService>();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var apiExplorer = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var description in apiExplorer.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
            description.GroupName.ToUpperInvariant());
    }

    options.RoutePrefix = string.Empty;
});

app.MapControllers();
app.MapHealthChecks("/status");
app.Logger.LogInformation("Initialize browser");
try
{
    await app.Services.GetRequiredService<BrowserInitializer>().InitializeAsync(app.Lifetime.ApplicationStopping);
}
catch (Exception e)
{
    app.Logger.LogError(e, "Browser cannot be initialized");
    throw;
}

app.Logger.LogInformation("Start application");
await app.RunAsync();