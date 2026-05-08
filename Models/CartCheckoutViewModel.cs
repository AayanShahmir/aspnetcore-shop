namespace BIsm2.Models
{
    public class CartCheckoutViewModel
    {
        public List<CartItem> CartItems { get; set; } = new();
        public decimal TotalPrice { get; set; }
        public decimal Charges { get; set; }
        public decimal GrandTotal { get; set; }
        public Order Order { get; set; } = new();
    }
}
