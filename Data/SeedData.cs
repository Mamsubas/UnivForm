using Microsoft.AspNetCore.Identity;
using UnivForm.Data;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<AppRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

        // Rolleri oluştur
        string[] roleNames = { "Admin", "User", "Manager", "Student" };

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
                IsActive = true
            };

            var createAdmin = await userManager.CreateAsync(admin, "Admin123!");
            if (createAdmin.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}