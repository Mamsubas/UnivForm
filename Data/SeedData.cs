using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UnivForm.Data;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("SeedData");

        // Rolleri oluştur
        string[] roleNames = { "Admin", "Moderator", "Manager", "User" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new AppRole { Name = roleName });
            }
        }

        // Admin kullanıcısı oluştur
        var adminUser = await userManager.FindByEmailAsync("admin@univform.com");
        if (adminUser == null)
        {
            var admin = new AppUser
            {
                UserName = "admin",
                Email = "admin@univform.com",
                FirstName = "Admin",
                LastName = "User",
                IsActive = true,
                EmailConfirmed = true // make seeded admin bypass email confirmation
            };

            var password = "Admin123!";
            var createAdmin = await userManager.CreateAsync(admin, password);
            if (createAdmin.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                logger.LogInformation("Seeded admin user: {Email} with password: {Password}", admin.Email, password);
            }
        }
    }
}