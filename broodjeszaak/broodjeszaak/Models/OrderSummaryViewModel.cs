namespace broodjeszaak.Models
{
    public class OrderSummaryViewModel
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        private decimal _totalPrice;
        public decimal TotalPrice
        {
            get => _totalPrice;
            set => _totalPrice = Math.Round(value, 2); // Rond het totale prijsgetal af tot 2 decimalen
        }
    }
}