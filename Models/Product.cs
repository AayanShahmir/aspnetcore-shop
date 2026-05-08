using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BIsm2.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Seller { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public DateTime DateOfUpload { get; set; }
        public ICollection<ProductMedia> Media { get; set; } = new List<ProductMedia>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public int Popularity { get; set; }

    }
}
