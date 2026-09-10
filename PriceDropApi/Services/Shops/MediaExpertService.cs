using PriceDropApi.Services.Interfaces;
using PriceDropApi.Services.Interfaces.Shops;

namespace PriceDropApi.Services.Shops
{
    public class MediaExpertService : IMediaExpertService
    {
        public decimal GetPrice(string productUrl)
        {
            return 2.2m;
        }
    }
}
