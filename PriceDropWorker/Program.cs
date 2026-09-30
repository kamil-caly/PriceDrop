using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

var priceDropApiBaseAddress = builder.Configuration["PriceDropApiBaseAddress"]
    ?? throw new InvalidOperationException("Configuration value 'PriceDropApiBaseAddress' is not configured.");

builder.Services.AddHttpClient("PriceDropApi", client =>
{
    client.BaseAddress = new Uri(priceDropApiBaseAddress);
});

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

builder.Build().Run();
