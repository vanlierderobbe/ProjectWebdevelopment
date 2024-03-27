using System.ComponentModel.DataAnnotations.Schema;

namespace broodjeszaak.Models
{
    [Table("producten")]
    public class Product
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        private decimal _price;
        public decimal Price
        {
            get => _price;
            set => _price = Math.Round(value, 2); // Rond het prijsgetal af tot 2 decimalen
        }
        public string ImagePath { get; set; }
    }
}