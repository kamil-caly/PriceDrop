using PriceDropApi.Models.Enums;

namespace PriceDropWorker
{
    public class NotifyDto
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string ProductName { get; set; } = default!;
        public double NewPrice { get; set; }
        public ShopType ShopWithNewPrice { get; set; } = default!;
        public string? UserExpoPushToken { get; set; }
    }
}
