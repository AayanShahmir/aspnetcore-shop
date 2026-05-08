using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BIsm2.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string? UserId { get; set; }   // Link to ASP.NET Identity user
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

    }
}
