using System;
using System.Net.Http.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PriceDropApi.Entities;
using PriceDropApi.Models.Enums;
using PriceDropApi.Services.Interfaces.Shops;

namespace PriceDropWorker;

public class CheckProductPrices
{
    private readonly ILogger _logger;
    private readonly IDbContextFactory<PriceDropDbContext> _dbContextFactory;
    private readonly IXKomService _xKomService;
    private readonly IMoreleService _moreleService;
    private readonly IHttpClientFactory _httpClientFactory;

    public CheckProductPrices(
        ILoggerFactory loggerFactory,
        IDbContextFactory<PriceDropDbContext> dbContextFactory,
        IXKomService xKomService,
        IMoreleService moreleService,
        IHttpClientFactory httpClientFactory)
    {
        _logger = loggerFactory.CreateLogger<CheckProductPrices>();
        _dbContextFactory = dbContextFactory;
        _xKomService = xKomService;
        _moreleService = moreleService;
        _httpClientFactory = httpClientFactory;
    }

    [Function("CheckProductPrices")]
    public async Task Run([TimerTrigger("0 0 12 1 * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var products = await dbContext.Products
            .Where(p => p.UserProducts.Any(up => up.NotificationsEnabled))
            .Include(p => p.UserProducts)
            .ToListAsync();

        var notifyDtos = new List<NotifyDto>();

        foreach (var product in products)
        {
            if (product.MoreleLink != null)
            {
                double moreleCurrentPrice = Convert.ToDouble(await _moreleService.GetPrice(product.MoreleLink));
                if (moreleCurrentPrice < product.MorelePrice)
                {
                    notifyDtos.Add(new NotifyDto
                    {
                        ProductId = product.Id,
                        UserId = product.UserProducts.First().UserId,
                        ProductName = product.Name,
                        NewPrice = moreleCurrentPrice,
                        ShopWithNewPrice = ShopType.Morele
                    });

                    continue;
                }
            }
            else if (product.X_KomLink != null)
            {
                double xKomCurrentPrice = Convert.ToDouble(await _xKomService.GetPrice(product.X_KomLink));
                if (xKomCurrentPrice < product.X_KomPrice)
                {
                    notifyDtos.Add(new NotifyDto
                    {
                        ProductId = product.Id,
                        UserId = product.UserProducts.First().UserId,
                        ProductName = product.Name,
                        NewPrice = xKomCurrentPrice,
                        ShopWithNewPrice = ShopType.XKom
                    });

                    continue;
                }
            }
        }

        if (notifyDtos.Count > 0)
        {
            var userIds = notifyDtos.Select(n => n.UserId);

            var userInfo =
                    (from u in dbContext.Users
                     where userIds.Contains(u.Id)
                     select new
                     {
                         Id = u.Id,
                         ExpoPushToken = u.ExpoPushToken
                     }).ToList();

            userInfo.ForEach(u =>
            {
                var notify = notifyDtos.First(n => n.UserId == u.Id);
                notify.UserExpoPushToken = u.ExpoPushToken;
            });

            // wysyłamy powiadomienia do użytkowników
            var httpClient = _httpClientFactory.CreateClient("ExpoPush");

            foreach (var notify in notifyDtos.Where(n => !string.IsNullOrWhiteSpace(n.UserExpoPushToken)))
            {
                var notification = new
                {
                    to = notify.UserExpoPushToken,
                    title = "Spadek ceny produktu",
                    body = $"Cena produktu {notify.ProductName} spadła do {notify.NewPrice:0.00} zł w sklepie {notify.ShopWithNewPrice}."
                };

                using var response = await httpClient.PostAsJsonAsync("send", notification);

                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Failed to send Expo notification for product {productId} to user {userId}. Status: {statusCode}. Response: {responseBody}",
                        notify.ProductId,
                        notify.UserId,
                        response.StatusCode,
                        responseBody);
                }
            }
        }

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}