namespace PriceDropApi.Models
{
    public class UpdateProductDto
    {
        public string Name { get; set; } = default!;
        public string? MoreleLink { get; set; }
        public string? X_KomLink { get; set; }
        public bool NotificationsEnabled { get; set; }
    }
}
