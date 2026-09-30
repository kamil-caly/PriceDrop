using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using PriceDropApi.Entities;
using PriceDropApi.Models;
using PriceDropApi.Models.Enums;
using PriceDropApi.Services.Interfaces;
using PriceDropApi.Services.Interfaces.Shops;

namespace PriceDropApi.Services
{
    public class ProductService : IProductService
    {
        private readonly PriceDropDbContext dbCtx;
        private readonly IUserContextService userContextService;
        private readonly IMoreleService moreleService;
        private readonly IXKomService xkomService;
        private readonly IHttpClientFactory httpClientFactory;

        public ProductService(PriceDropDbContext dbCtx, IUserContextService userContextService, IMoreleService moreleService,
            IXKomService xkomService, IHttpClientFactory httpClientFactory)
        {
            this.dbCtx = dbCtx;
            this.userContextService = userContextService;
            this.moreleService = moreleService;
            this.xkomService = xkomService;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<decimal?> GetPriceAsync(GetPriceDto dto)
        {
            return dto.ShopType switch
            {
                ShopType.XKom => await xkomService.GetPrice(dto.ProductUrl),
                ShopType.Morele => await moreleService.GetPrice(dto.ProductUrl),
                _ => null
            };
        }

        public async Task CheckProductPricesAndNotifyUsersAsync()
        {
            var products = await dbCtx.Products
                .Where(p => p.UserProducts.Any(up => up.NotificationsEnabled))
                .Include(p => p.UserProducts)
                .ToListAsync();

            var notifyDtos = new List<PriceDropNotificationDto>();

            foreach (var product in products)
            {
                if (product.MoreleLink != null)
                {
                    double moreleCurrentPrice = Convert.ToDouble(await moreleService.GetPrice(product.MoreleLink));
                    if (moreleCurrentPrice < product.MorelePrice)
                    {
                        notifyDtos.Add(new PriceDropNotificationDto
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
                    double xKomCurrentPrice = Convert.ToDouble(await xkomService.GetPrice(product.X_KomLink));
                    if (xKomCurrentPrice < product.X_KomPrice)
                    {
                        notifyDtos.Add(new PriceDropNotificationDto
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

            if (notifyDtos.Count == 0)
            {
                return;
            }

            var userIds = notifyDtos.Select(n => n.UserId);

            var userInfo =
                    (from u in dbCtx.Users
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

            var httpClient = httpClientFactory.CreateClient("ExpoPush");

            foreach (var notify in notifyDtos.Where(n => !string.IsNullOrWhiteSpace(n.UserExpoPushToken)))
            {
                var notification = new
                {
                    to = notify.UserExpoPushToken,
                    title = "Spadek ceny produktu",
                    body = $"Cena produktu {notify.ProductName} spadła do {notify.NewPrice:0.00} zł w sklepie {notify.ShopWithNewPrice}."
                };

                using var response = await httpClient.PostAsJsonAsync("send", notification);
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task<IEnumerable<GetProductsDto>> GetProductsAsync()
        {
            int? userId = userContextService.GetUserId();
            if (userId == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var products = await dbCtx.UserProducts
                .Where(up => up.UserId == userId)
                .Include(up => up.Product)
                .Select(up => up.Product)
                .ToListAsync();

            // aktualizujemy ceny produktów w bazie danych
            for (int i = 0; i < products.Count; i++)
            {
                var product = products[i];
                bool priceUpdated = false;

                if (product.MoreleLink != null)
                {
                    var morelePrice = await moreleService.GetPrice(product.MoreleLink);
                    product.MorelePrice = decimal.ToDouble(morelePrice);
                    priceUpdated = true;
                }

                if (product.X_KomLink != null)
                {
                    var xkomPrice = await xkomService.GetPrice(product.X_KomLink);
                    product.X_KomPrice = decimal.ToDouble(xkomPrice);
                    priceUpdated = true;
                }

                if (priceUpdated)
                {
                    product.LastCheckAt = DateTime.UtcNow;
                    dbCtx.Products.Update(product);
                }
            }

            await dbCtx.SaveChangesAsync();

            return await dbCtx.UserProducts
                .Where(up => up.UserId == userId)
                .Include(up => up.Product)
                .Select(up => new GetProductsDto
                {
                    Id = up.Product.Id,
                    Name = up.Product.Name,
                    LastCheckAt = up.Product.LastCheckAt,
                    MoreleLink = up.Product.MoreleLink,
                    MorelePrice = up.Product.MorelePrice,
                    X_KomLink = up.Product.X_KomLink,
                    X_KomPrice = up.Product.X_KomPrice,
                    NotificationsEnabled = up.NotificationsEnabled
                })
                .ToListAsync();
        }

        public async Task<int> AddProductAsync(AddProductDto dto)
        {
            int? userId = userContextService.GetUserId();
            if (userId == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var newProduct = new Product
            {
                Name = dto.Name,
                MoreleLink = dto.MoreleLink,
                X_KomLink = dto.X_KomLink
            };

            dbCtx.Products.Add(newProduct);

            var newUserProduct = new UserProduct
            {
                UserId = userId.Value,
                Product = newProduct,
                NotificationsEnabled = dto.NotificationsEnabled
            };

            dbCtx.UserProducts.Add(newUserProduct);
            await dbCtx.SaveChangesAsync();

            return newProduct.Id;
        }

        public async Task DeleteProductAsync(int productId)
        {
            int? userId = userContextService.GetUserId();
            if (userId == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var userProduct = await dbCtx.UserProducts
                .FirstOrDefaultAsync(up => up.UserId == userId && up.ProductId == productId);

            if (userProduct == null)
            {
                throw new InvalidOperationException("Product not found.");
            }

            var productToDel = await dbCtx.Products.FirstAsync(p => p.Id == productId);

            dbCtx.UserProducts.Remove(userProduct);
            dbCtx.Products.Remove(productToDel);

            await dbCtx.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(int productId, UpdateProductDto dto)
        {
            int? userId = userContextService.GetUserId();
            if (userId == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var userProduct = await dbCtx.UserProducts
                .FirstOrDefaultAsync(up => up.UserId == userId && up.ProductId == productId);

            if (userProduct == null)
            {
                throw new InvalidOperationException("Product not found.");
            }

            var productToUpdate = await dbCtx.Products.FirstAsync(p => p.Id == productId);

            productToUpdate.Name = dto.Name;
            productToUpdate.MoreleLink = dto.MoreleLink;
            productToUpdate.X_KomLink = dto.X_KomLink;
            userProduct.NotificationsEnabled = dto.NotificationsEnabled;

            await dbCtx.SaveChangesAsync();
        }
    }
}
