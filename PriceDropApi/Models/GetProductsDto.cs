namespace PriceDropApi.Models
{
    public class GetProductsDto
    {
        public string Name { get; set; } = default!;
        public DateTime LastCheckAt { get; set; }
        public double? MorelePrice { get; set; }
        public string? MoreleLink { get; set; }
        public double? X_KomPrice { get; set; }
        public string? X_KomLink { get; set; }
        public bool NotificationsEnabled { get; set; }
    }
}
