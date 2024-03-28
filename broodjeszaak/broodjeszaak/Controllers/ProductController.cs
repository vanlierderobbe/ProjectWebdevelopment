using Microsoft.AspNetCore.Mvc;

namespace broodjeszaak.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Products.ToList(); // Verkrijg de lijst van producten uit de database
            return View(products); // Stuur de lijst naar de view
        }
    }
}