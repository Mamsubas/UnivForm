using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnivForm.Data;
using UnivForm.Models;

namespace UnivForm.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly AppDbContext _context;

    public AdminController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    // GET: /Admin (Dashboard with Statistics)
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        var model = new List<AdminUserViewModel>();

        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            var postCount = _context.ForumThreads.Count(p => p.AuthorId == u.Id) +
                           _context.Posts.Count(p => p.AuthorId == u.Id && !p.IsDeleted);

            model.Add(new AdminUserViewModel
            {
                Id = u.Id,
                Email = u.Email ?? "",
                Name = $"{u.FirstName} {u.LastName}",
                Roles = roles.ToList(),
                IsAdmin = roles.Contains("Admin"),
                IsActive = u.IsActive,
                PostCount = postCount,
                CreatedAt = u.CreatedAt,
                LastLogin = u.LastLogin
            });
        }

        // Dashboard Statistics
        var totalUsers = users.Count;
        var activeUsers = users.Count(u => u.IsActive);
        var totalThreads = _context.ForumThreads.Count();
        var totalPosts = _context.Posts.Count(p => !p.IsDeleted);
        var totalRoles = _roleManager.Roles.Count();

        ViewBag.TotalUsers = totalUsers;
        ViewBag.ActiveUsers = activeUsers;
        ViewBag.TotalThreads = totalThreads;
        ViewBag.TotalPosts = totalPosts;
        ViewBag.TotalRoles = totalRoles;

        return View(model);
    }

    // GET: /Admin/Roles
    public async Task<IActionResult> Roles()
    {
        var roles = _roleManager.Roles.ToList();
        var model = new List<AdminRoleViewModel>();

        foreach (var role in roles)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? "");
            model.Add(new AdminRoleViewModel
            {
                Id = role.Id,
                Name = role.Name ?? "",
                Description = role.Description ?? "",
                UserCount = usersInRole.Count
            });
        }

        return View(model);
    }

    // GET: /Admin/CreateRole
    public IActionResult CreateRole()
    {
        return View();
    }

    // POST: /Admin/CreateRole
    [HttpPost]
    public async Task<IActionResult> CreateRole(string roleName, string description)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            ModelState.AddModelError(string.Empty, "Rol adı zorunludur.");
            return View();
        }

        var roleExists = await _roleManager.RoleExistsAsync(roleName);
        if (roleExists)
        {
            ModelState.AddModelError(string.Empty, "Bu rol zaten mevcut.");
            return View();
        }

        var result = await _roleManager.CreateAsync(new AppRole
        {
            Name = roleName,
            Description = description ?? ""
        });

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = $"Rol '{roleName}' başarıyla oluşturuldu.";
            return RedirectToAction("Roles");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View();
    }

    // GET: /Admin/EditRole/{id}
    public async Task<IActionResult> EditRole(int id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null) return NotFound();

        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? "");
        var model = new EditRoleViewModel
        {
            Id = role.Id,
            Name = role.Name ?? "",
            Description = role.Description ?? "",
            Users = usersInRole.Select(u => u.Email ?? "").ToList()
        };

        return View(model);
    }

    // POST: /Admin/EditRole/{id}
    [HttpPost]
    public async Task<IActionResult> EditRole(int id, string roleName, string description)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null) return NotFound();

        role.Description = description ?? "";
        var result = await _roleManager.UpdateAsync(role);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Rol başarıyla güncellenmiştir.";
            return RedirectToAction("Roles");
        }

        return View();
    }

    // POST: /Admin/DeleteRole/{id}
    [HttpPost]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null) return NotFound();

        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? "");
        if (usersInRole.Count > 0)
        {
            TempData["ErrorMessage"] = "Bu role atanmış kullanıcılar var. Önce onları kaldırın.";
            return RedirectToAction("Roles");
        }

        var result = await _roleManager.DeleteAsync(role);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Rol başarıyla silindi.";
        }

        return RedirectToAction("Roles");
    }

    // POST: /Admin/AssignRole
    [HttpPost]
    public async Task<IActionResult> AssignRole(int userId, string roleId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return NotFound();

        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null) return NotFound();

        var isInRole = await _userManager.IsInRoleAsync(user, role.Name ?? "");
        if (!isInRole)
        {
            await _userManager.AddToRoleAsync(user, role.Name ?? "");
            TempData["SuccessMessage"] = $"Kullanıcı '{role.Name}' rolüne eklendi.";
        }
        else
        {
            TempData["ErrorMessage"] = "Kullanıcı zaten bu rolde.";
        }

        return RedirectToAction("Index");
    }

    // POST: /Admin/RemoveRole
    [HttpPost]
    public async Task<IActionResult> RemoveRole(int userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return NotFound();

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = $"Rol '{roleName}' kullanıcıdan kaldırıldı.";
        }

        return RedirectToAction("Index");
    }

    // GET: /Admin/UserPosts/{id}
    public IActionResult UserPosts(int id)
    {
        var user = _userManager.Users.FirstOrDefault(u => u.Id == id);
        if (user == null) return NotFound();

        var threads = _context.ForumThreads.Where(t => t.AuthorId == id).OrderByDescending(t => t.CreatedAt).ToList();
        var posts = _context.Posts.Where(p => p.AuthorId == id && !p.IsDeleted).OrderByDescending(p => p.CreatedAt).ToList();

        var model = new UserPostsViewModel
        {
            UserId = id,
            UserName = $"{user.FirstName} {user.LastName}",
            UserEmail = user.Email ?? "",
            Threads = threads,
            Posts = posts,
            ThreadCount = threads.Count,
            PostCount = posts.Count
        };

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
        public List<string> Roles { get; set; } = new();
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public int PostCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    public class AdminRoleViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int UserCount { get; set; }
    }

    public class EditRoleViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> Users { get; set; } = new();
    }

    public class UserPostsViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public List<ForumThread> Threads { get; set; } = new();
        public List<Post> Posts { get; set; } = new();
        public int ThreadCount { get; set; }
        public int PostCount { get; set; }
    }
}