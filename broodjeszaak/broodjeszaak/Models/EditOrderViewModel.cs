using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace broodjeszaak.Models
{
    public class EditOrderViewModel
    {
        public int OrderId { get; set; }
        public List<EditOrderDetailViewModel> OrderDetails { get; set; } = new List<EditOrderDetailViewModel>();
    }

    public class EditOrderDetailViewModel
    {
        public int OrderDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Aantal moet groter dan 0 zijn")]
        public int Quantity { get; set; }
    }
}
