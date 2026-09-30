using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace PriceDropWorker;

public class CheckProductPrices
{
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public CheckProductPrices(
        ILoggerFactory loggerFactory,
        IHttpClientFactory httpClientFactory)
    {
        _logger = loggerFactory.CreateLogger<CheckProductPrices>();
        _httpClientFactory = httpClientFactory;
    }

    [Function("CheckProductPrices")]
    public async Task Run([TimerTrigger("0 0 12 25 * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);

        var httpClient = _httpClientFactory.CreateClient("PriceDropApi");

        using var response = await httpClient.PostAsync("api/products/check-prices", null);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "Failed to trigger product price check in PriceDropApi. Status: {statusCode}. Response: {responseBody}",
                response.StatusCode,
                responseBody);
        }

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }

        _logger.LogInformation("C# Timer trigger function finished execution at: {executionTime}", DateTime.Now);
    }
}