using System.ComponentModel.DataAnnotations;

namespace PriceDropApi.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public DateTime? LastCheckAt { get; set; }
        public double? MorelePrice { get; set; }
        public string? MoreleLink { get; set; }
        public double? X_KomPrice { get; set; }
        public string? X_KomLink { get; set; }
        public virtual ICollection<UserProduct> UserProducts { get; set; } = new List<UserProduct>();
    }
}
