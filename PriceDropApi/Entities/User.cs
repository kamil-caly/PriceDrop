using System.ComponentModel.DataAnnotations;

namespace PriceDropApi.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Login { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public string? ExpoPushToken { get; set; }
        public virtual ICollection<UserProduct> UserProducts { get; set; } = new List<UserProduct>();
    }
}
