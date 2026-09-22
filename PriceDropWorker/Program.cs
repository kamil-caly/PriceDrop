using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PriceDropApi.Entities;
using PriceDropApi.Services.Interfaces.Shops;
using PriceDropApi.Services.Shops;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContextFactory<PriceDropDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddSingleton<IXKomService, XKomService>();
builder.Services.AddSingleton<IMoreleService, MoreleService>();

builder.Services.AddHttpClient("ExpoPush", client =>
{
    client.BaseAddress = new Uri("https://exp.host/--/api/v2/push/");
});

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Build().Run();
