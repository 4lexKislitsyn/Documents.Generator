using Documents.Generator;
using Documents.Generator.Contracts;
using Documents.Generator.Grpc;
using Documents.Generator.Services;
using PuppeteerSharp;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
builder.Services.AddGrpc();
builder.Services.AddHealthChecks();

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
app.MapGrpcService<GeneratorService>();
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