using PriceDropApi.Models.Enums;

namespace PriceDropApi.Models
{
    public class GetPriceDto
    {
        public string ShopUrl { get; set; } = default!;
        public ShopType ShopType { get; set; }
    }
}
