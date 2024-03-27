using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using broodjeszaak.Models; // Zorg ervoor dat je de juiste namespace gebruikt
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System;

namespace broodjeszaak.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }


        // Bijgewerkte lijst van gebruikers
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var userClaims = await _userManager.GetClaimsAsync(user);
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                var isApproved = userClaims.Any(c => c.Type == "IsApproved" && c.Value == "True");

                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    IsAdmin = isAdmin,
                    IsApproved = isApproved
                });
            }

            return View(userViewModels);
        }

        // Administrator toevoegen
        public async Task<IActionResult> AddAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }
            return RedirectToAction(nameof(Index));
        }

        // Administrator verwijderen
        public async Task<IActionResult> RemoveAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                // Controleer of er meer dan één admin is voordat je de huidige admin verwijdert
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                if (admins.Count <= 1)
                {
                    // Toon een foutmelding dat er minstens één admin moet blijven
                    TempData["Error"] = "Er moet minstens één admin overblijven.";
                }
                else
                {
                    // Als er meer admins zijn, ga door met het verwijderen
                    await _userManager.RemoveFromRoleAsync(user, "Admin");
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // Gebruiker goedkeuren
        public async Task<IActionResult> ApproveUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var claim = new Claim("IsApproved", "True");
                var result = await _userManager.AddClaimAsync(user, claim);
                if (result.Succeeded)
                {
                    // Logica voor het succesvol goedkeuren van de gebruiker
                }
                else
                {
                    // Logica voor als het goedkeuren niet succesvol was
                }
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> OrderList()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .ToListAsync();

            var orderViewModels = orders.GroupBy(o => o.UserId)
                .Select(group => new UserOrderSummaryViewModel
                {
                    UserId = group.Key,
                    UserEmail = _userManager.Users.FirstOrDefault(u => u.Id == group.Key)?.Email,
                    Orders = group.SelectMany(g => g.OrderDetails)
                        .GroupBy(od => od.OrderId)
                        .Select(g => new OrderViewModel
                        {
                            OrderId = g.Key,
                            OrderDetails = g.Select(od => new OrderDetailViewModel
                            {
                                OrderDetailId = od.OrderDetailId,
                                ProductId = od.ProductId,
                                ProductName = od.Product.Name, // Aannemende dat je Product model een Name eigenschap heeft
                                Quantity = od.Quantity,
                                Price = od.Price
                            }).ToList(),
                            TotalPrice = g.Sum(od => od.Price * od.Quantity)
                        }).ToList(),
                    TotalPricePerUser = group.SelectMany(g => g.OrderDetails).Sum(od => od.Price * od.Quantity)
                }).ToList();

            return View(orderViewModels);
        }

        public async Task<IActionResult> EditOrder(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            var model = new EditOrderViewModel
            {
                OrderId = order.OrderId,
                OrderDetails = order.OrderDetails.Select(od => new EditOrderDetailViewModel
                {
                    OrderDetailId = od.OrderDetailId,
                    ProductId = od.ProductId,
                    ProductName = od.Product.Name, // Pas aan gebaseerd op je model
                    Quantity = od.Quantity
                    // Voeg de prijs toe indien nodig
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditOrder(EditOrderViewModel model)
        {
            _logger.LogInformation("Begin van EditOrder POST-actie.");

            if (ModelState.IsValid)
            {
                _logger.LogInformation($"ModelState is geldig. OrderId: {model.OrderId}");

                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(o => o.OrderId == model.OrderId);

                if (order == null)
                {
                    _logger.LogWarning($"Geen order gevonden met OrderId: {model.OrderId}");
                    return NotFound();
                }

                foreach (var modelDetail in model.OrderDetails)
                {
                    _logger.LogInformation($"Verwerken OrderDetail: {modelDetail.OrderDetailId}, Nieuwe Quantity: {modelDetail.Quantity}");
                    var orderDetail = order.OrderDetails.FirstOrDefault(od => od.OrderDetailId == modelDetail.OrderDetailId);
                    if (orderDetail != null)
                    {
                        _logger.LogInformation($"Update OrderDetail {orderDetail.OrderDetailId} van Quantity {orderDetail.Quantity} naar {modelDetail.Quantity}.");
                        orderDetail.Quantity = modelDetail.Quantity;
                        _context.Entry(orderDetail).State = EntityState.Modified;
                    }
                    else
                    {
                        _logger.LogWarning($"OrderDetail niet gevonden: {modelDetail.OrderDetailId}");
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Wijzigingen succesvol opgeslagen in de database.");
                    return RedirectToAction(nameof(OrderList));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Er is een fout opgetreden bij het opslaan van de wijzigingen in de database.");
                    ModelState.AddModelError("", "Er is een fout opgetreden bij het opslaan van de wijzigingen.");
                }
            }
            else
            {
                _logger.LogWarning("ModelState is ongeldig.");
                // Log de details van de ModelState fouten
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        _logger.LogWarning($"Key: {entry.Key}, Error: {error.ErrorMessage}");
                    }
                }
            }

            // Als de ModelState niet geldig is, of als er een fout is opgetreden, bereid dan de view opnieuw voor
            return View(model);
        }


        // Enkele bestelling verwijderen
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Bestelling verwijderd.";
            }
            else
            {
                TempData["Error"] = "Bestelling niet gevonden.";
            }

            return RedirectToAction(nameof(OrderList));
        }

        // Bulk bestellingen verwijderen
        [HttpPost]
        public async Task<IActionResult> DeleteOrders(List<int> orderIds)
        {
            var orders = _context.Orders.Where(o => orderIds.Contains(o.OrderId)).ToList();
            _context.Orders.RemoveRange(orders);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(OrderList));
        }

        public async Task<IActionResult> EditProductPrices()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> EditProductPrices(List<Product> products)
        {
            if (ModelState.IsValid)
            {
                foreach (var updatedProduct in products)
                {
                    var product = await _context.Products.FindAsync(updatedProduct.ProductID);
                    if (product != null)
                    {
                        product.Price = updatedProduct.Price;
                        _context.Entry(product).State = EntityState.Modified;
                        _logger.LogInformation($"ProductID: {product.ProductID}, Updated Price: {product.Price}");
                    }
                    else
                    {
                        _logger.LogWarning($"Product with ID {updatedProduct.ProductID} not found.");
                    }
                }

                // Voeg een logbericht toe om te controleren of alle prijzen zijn bijgewerkt
                _logger.LogInformation("Alle productprijzen zijn bijgewerkt.");

                try
                {
                    await _context.SaveChangesAsync();
                    // Voeg een logbericht toe om te controleren of SaveChangesAsync correct is aangeroepen
                    _logger.LogInformation("Changes successfully saved to the database.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error saving changes to the database: {ex.Message}");
                    throw;
                }

                return RedirectToAction(nameof(EditProductPrices));
            }

            // Retourneer de view opnieuw als er iets misgaat
            _logger.LogWarning("ModelState is not valid.");
            return View(products);
        }
    }
}