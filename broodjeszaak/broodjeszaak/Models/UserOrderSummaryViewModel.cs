using Microsoft.AspNetCore.Mvc;

namespace broodjeszaak.Models
{
    public class UserOrderSummaryViewModel
    {
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public List<OrderViewModel> Orders { get; set; } = new List<OrderViewModel>();
        private decimal _totalPricePerUser;
        public decimal TotalPricePerUser
        {
            get => _totalPricePerUser;
            set => _totalPricePerUser = Math.Round(value, 2); // Rond het totale prijsgetal per gebruiker af tot 2 decimalen
        }
    }

    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public List<OrderDetailViewModel> OrderDetails { get; set; } = new List<OrderDetailViewModel>();
        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            set => _totalPrice = Math.Round(value, 2); // Rond het totale prijsgetal af tot 2 decimalen
        }
    }

    // Als OrderDetail inderdaad als een ViewModel fungeert en past binnen je huidige structuur
    public class OrderDetailViewModel
    {
        public int OrderDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } // Aannemende dat je de productnaam wilt weergeven
        public int Quantity { get; set; }
        private decimal _price;
        public decimal Price
        {
            get => _price;
            set => _price = Math.Round(value, 2); // Rond het prijsgetal af tot 2 decimalen
        }
    }
}
