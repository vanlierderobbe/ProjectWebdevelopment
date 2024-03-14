using broodjeszaak.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace broodjeszaak.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Order/Create
        public IActionResult Create()
        {
            ViewData["Products"] = new SelectList(_context.Products, "ProductID", "Name");
            return View(new Order { OrderDetails = new List<OrderDetail> { new OrderDetail() } });
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Products"] = new SelectList(_context.Products, "ProductId", "Name", order.OrderDetails?.FirstOrDefault()?.ProductId);
            return View(order);
        }

        // Andere acties...
    }
}