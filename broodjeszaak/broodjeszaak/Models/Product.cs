using System.ComponentModel.DataAnnotations.Schema;

namespace broodjeszaak.Models
{
    [Table("producten")]
    public class Product
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}