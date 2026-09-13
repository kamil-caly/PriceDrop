using PriceDropApi.Models;

namespace PriceDropApi.Services.Interfaces
{
    public interface IProductService
    {
        Task<decimal?> GetPriceAsync(GetPriceDto dto);
        Task<IEnumerable<GetProductsDto>> GetProductsAsync();
    }
}
