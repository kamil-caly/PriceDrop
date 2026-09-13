using PriceDropApi.Models;

namespace PriceDropApi.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<GetProductsDto>> GetProductsAsync();
    }
}
