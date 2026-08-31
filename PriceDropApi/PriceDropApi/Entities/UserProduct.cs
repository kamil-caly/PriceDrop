using System.ComponentModel.DataAnnotations;

namespace PriceDropApi.Entities
{
    public class UserProduct
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public bool NotificationsEnabled { get; set; }
        public int NotifyEveryHours { get; set; }
    }
}
