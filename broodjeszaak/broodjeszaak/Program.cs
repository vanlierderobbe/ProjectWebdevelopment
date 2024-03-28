using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.MySql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

// Hier voeg je de Identity configuratie toe met rollenondersteuning
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>() // Voeg rollenondersteuning toe
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configureer de cookie-instellingen
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Account/Login";
    options.LogoutPath = $"/Account/Logout";
    options.AccessDeniedPath = $"/Account/AccessDenied";
});

var app = builder.Build();

// Configureer de HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Rolinitialisatie
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        await InitializeRolesAsync(roleManager, userManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Een fout is opgetreden bij het initialiseren van de rollen.");
    }
}

async Task InitializeRolesAsync(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
{
    string[] roleNames = { "Admin", "Employee", "User" }; // Definieer je rollen inclusief "Employee"
    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            // Maak de rollen aan en sla ze op in de database
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    var employeeEmail = "employee@vives.be";
    var employeeUserName = employeeEmail;
    var employeePassword = "Employee123!"; // Zorg voor een sterk wachtwoord in productie

    var employeeUser = await userManager.FindByEmailAsync(employeeEmail);
    if (employeeUser == null)
    {
        employeeUser = new IdentityUser
        {
            UserName = employeeUserName,
            Email = employeeEmail
        };
        var createEmployeeResult = await userManager.CreateAsync(employeeUser, employeePassword);
        if (createEmployeeResult.Succeeded)
        {
            await userManager.AddToRoleAsync(employeeUser, "Employee"); // Voeg de rol "Employee" toe aan de gebruiker
        }
    }

    // Aanmaken van een standaard admin gebruiker
    var adminEmail = "admin@vives.be";
    var adminUserName = adminEmail;
    var adminPassword = "Admin123!";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminUserName,
            Email = adminEmail
        };
        var createAdminResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (createAdminResult.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

app.Run();