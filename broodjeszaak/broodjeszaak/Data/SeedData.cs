using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace broodjeszaak.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Voeg rollen toe
            await EnsureRoleAsync(roleManager, "Administrator");
            await EnsureRoleAsync(roleManager, "User");

            // Voeg gebruiker toe en ken rol toe
            await EnsureUserAsync(userManager, "admin@example.com", "Admin#1234", "Administrator");
            // Voeg meer gebruikers toe zoals nodig
        }

        private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            var exists = await roleManager.RoleExistsAsync(roleName);
            if (!exists)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        private static async Task EnsureUserAsync(UserManager<IdentityUser> userManager, string email, string password, string roleName)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser { UserName = email, Email = email };
                await userManager.CreateAsync(user, password);
            }

            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                await userManager.AddToRoleAsync(user, roleName);
            }
        }
    }
}