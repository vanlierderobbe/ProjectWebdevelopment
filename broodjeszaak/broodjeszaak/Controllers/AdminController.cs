using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using broodjeszaak.Models; // Zorg ervoor dat je de juiste namespace gebruikt
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;

namespace broodjeszaak.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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
    }
}