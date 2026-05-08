using BIsm2.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BIsm2.Models
{
    public class Order
    {
        public int? Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        //public List<OrderItem> Items { get; set; } = new();
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; } // product price * quantity
        public decimal Charges { get; set; }    // 1.5% company charges
        public decimal GrandTotal { get; set; } // TotalPrice + Charges

        // Customer info
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Phone { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Country { get; set; }
        [Required]
        public string? City { get; set; }
        [Required]
        public string? Address { get; set; }
        [Required]
        public string? Street { get; set; }
        [Required]
        public string? HouseNumber { get; set; }
        public string? Landmark { get; set; }
        [Required]
        public string? PaymentMethod { get; set; }
        public bool IsPaid { get; set; }
    }
}
