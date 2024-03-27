using Microsoft.AspNetCore.Mvc;

namespace broodjeszaak.Models
{
    public class CreateOrderViewModel
    {
        public List<CartItem> Cart { get; set; }
    }

    public class CartItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
