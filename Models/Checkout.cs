using System.ComponentModel.DataAnnotations;

namespace BIsm2.Models
{
    public class Checkout
    {
        public int Id { get; set; }

        // Customer info
        [Required] public string? Name { get; set; }
        [Required] public string? Phone { get; set; }
        [Required] public string? Email { get; set; }
        [Required] public string? Country { get; set; }
        [Required] public string? City { get; set; }
        [Required] public string? Address { get; set; }
        [Required] public string? Street { get; set; }
        [Required] public string? HouseNumber { get; set; }
        public string? Landmark { get; set; }
        [Required] public string? PaymentMethod { get; set; }
        public bool IsPaid { get; set; }

        // Totals
        public decimal TotalPrice { get; set; }
        public decimal Charges { get; set; }
        public decimal GrandTotal { get; set; }

        // Multiple products
        public List<CheckoutItem> Items { get; set; } = new();
    }

    public class CheckoutItem
    {
        public int Id { get; set; }
        public int CheckoutId { get; set; }
        public Checkout Checkout { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }
}
