using PriceDropApi.Models.Enums;

namespace PriceDropApi.Models
{
    public class GetPriceDto
    {
        public string ProductUrl { get; set; } = default!;
        public ShopType ShopType { get; set; }
    }
}
