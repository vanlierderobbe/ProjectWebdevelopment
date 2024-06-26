﻿using System.Collections.Generic;

namespace broodjeszaak.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }

    public class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        private decimal _price;
        public decimal Price
        {
            get => _price;
            set => _price = Math.Round(value, 2);
        }
        public Product Product { get; set; }
    }
}