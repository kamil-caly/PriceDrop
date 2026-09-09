using PriceDropApi.Models;

namespace PriceDropApi.Services.Interfaces.Shops
{
    public interface IMediaExpertService
    {
        decimal GetPrice(string productUrl);
    }
}
