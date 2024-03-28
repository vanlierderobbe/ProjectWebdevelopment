using broodjeszaak.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace broodjeszaak.Controllers
{
    [Authorize]
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
            var products = _context.Products.ToList(); // Haal de lijst van producten op uit de database
            ViewData["Products"] = products;
            return View(new Order { OrderDetails = new List<OrderDetail> { new OrderDetail() } });
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order, int[] selectedProducts)
        {
            if (ModelState.IsValid)
            {
                if (selectedProducts != null && selectedProducts.Length > 0)
                {
                    foreach (int productId in selectedProducts)
                    {
                        var quantityStr = Request.Form["quantity_" + productId];
                        int quantity = int.Parse(quantityStr);

                        var product = await _context.Products.FindAsync(productId);
                        if (product != null && quantity > 0)
                        {
                            var orderDetail = new OrderDetail
                            {
                                ProductId = productId,
                                Quantity = quantity,
                                Price = product.Price
                            };
                            order.OrderDetails.Add(orderDetail);
                        }
                    }

                    // Voeg de order toe aan de context en sla op
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            // Als het model niet geldig is, zet de producten weer in de ViewBag voor de checkboxes
            ViewData["Products"] = new SelectList(_context.Products, "ProductID", "Name");
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var order = new Order { UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) };

                foreach (var item in model.Cart)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        var orderDetail = new OrderDetail
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = product.Price // Neem aan dat de prijs nog geldig is; anders hier verifiëren
                        };
                        order.OrderDetails.Add(orderDetail);
                    }
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // GET: Order/Summary
        [Authorize(Roles = "Employee, Admin")]
        public IActionResult Summary()
        {
            var orderSummary = _context.OrderDetails
                .Include(od => od.Product)
                .GroupBy(od => od.Product.Name)
                .Select(g => new OrderSummaryViewModel
                {
                    ProductName = g.Key,
                    Quantity = g.Sum(od => od.Quantity),
                    TotalPrice = g.Sum(od => od.Quantity * od.Product.Price)
                })
                .ToList();

            // Bereken de totale prijs voor alle bestellingen
            decimal totalOrderPrice = orderSummary.Sum(item => item.TotalPrice);

            ViewBag.TotalOrderPrice = totalOrderPrice;

            return View(orderSummary);
        }

        // GET: Order/IndividualOrdersSummary
        [Authorize(Roles = "Employee, Admin")]
        public async Task<IActionResult> IndividualOrdersSummary()
        {
            var userOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .ToListAsync();

            var ordersGroupedByUser = userOrders
                .GroupBy(o => o.UserId)
                .Select(group => new UserOrderSummaryViewModel
                {
                    UserId = group.Key,
                    UserEmail = GetUserEmail(group.Key), // Correct e-mailadres ophalen
                    Orders = group.Select(o => new OrderViewModel
                    {
                        OrderId = o.OrderId,
                        OrderDetails = o.OrderDetails.Select(od => new OrderDetailViewModel
                        {
                            ProductId = od.ProductId,
                            ProductName = od.Product.Name,
                            Quantity = od.Quantity,
                            Price = od.Price
                        }).ToList(),
                        TotalPrice = o.OrderDetails.Sum(od => od.Quantity * od.Price)
                    }).ToList(),
                    TotalPricePerUser = group.Sum(o => o.OrderDetails.Sum(od => od.Quantity * od.Price))
                }).ToList();

            return View(ordersGroupedByUser);
        }

        // Hulpfunctie om het e-mailadres van de gebruiker op te halen op basis van de UserId
        private string GetUserEmail(string userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            return user != null ? user.Email : "Onbekend"; // Geef het e-mailadres terug, of "Onbekend" als het niet gevonden wordt
        }
    }
}