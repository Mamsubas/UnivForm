using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UnivForm.Data;

namespace UnivForm.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;

    public AdminController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: /Admin
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        var model = new List<AdminUserViewModel>();

        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            model.Add(new AdminUserViewModel
            {
                Id = u.Id,
                Email = u.Email ?? "",
                Name = $"{u.FirstName} {u.LastName}",
                IsAdmin = roles.Contains("Admin"),
                IsActive = u.IsActive
            });
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleAdmin(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Admin"))
        {
            await _userManager.RemoveFromRoleAsync(user, "Admin");
        }
        else
        {
            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new AppRole { Name = "Admin", Description = "Site yöneticisi" });
            }
            await _userManager.AddToRoleAsync(user, "Admin");
        }
        TempData["SuccessMessage"] = "Kullanıcı rolü güncellendi.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        TempData["SuccessMessage"] = "Kullanıcı aktiflik durumu güncellendi.";
        return RedirectToAction("Index");
    }

    public class AdminUserViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
    }
}
