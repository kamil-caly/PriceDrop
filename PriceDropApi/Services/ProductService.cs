using Microsoft.EntityFrameworkCore;
using PriceDropApi.Entities;
using PriceDropApi.Models;
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
    }
}
