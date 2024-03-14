namespace broodjeszaak.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public string UserId { get; set; } // Zorg ervoor dat dit overeenkomt met je User model
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }

    public class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
