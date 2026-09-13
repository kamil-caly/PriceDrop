using Microsoft.EntityFrameworkCore;
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

        public ProductService(PriceDropDbContext dbCtx, IUserContextService userContextService, IMoreleService moreleService,
            IXKomService xkomService)
        {
            this.dbCtx = dbCtx;
            this.userContextService = userContextService;
            this.moreleService = moreleService;
            this.xkomService = xkomService;
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

            dbCtx.UserProducts.Remove(userProduct);
            await dbCtx.SaveChangesAsync();

            // teraz sprawdzamy, czy w tabeli z produktami jest produkt, który nie jest powiązany z żadnym użytkownikiem
            var isProductLinkedToAnyUser = await dbCtx.UserProducts
                .AnyAsync(up => up.ProductId == productId);

            if (!isProductLinkedToAnyUser)
            {
                var productToDel = await dbCtx.Products.FindAsync(productId);
                if (productToDel != null)
                {
                    dbCtx.Products.Remove(productToDel);
                    await dbCtx.SaveChangesAsync();
                }
            }
        }
    }
}
