using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UnivForm.Data;

namespace UnivForm.Controllers;

[Route("[controller]")]
public class UserProfileController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public UserProfileController(
        UserManager<AppUser> userManager,
        AppDbContext context,
        IWebHostEnvironment env)
    {
        _userManager = userManager;
        _context = context;
        _env = env;
    }

    // GET: /UserProfile/Edit
    [HttpGet("edit")]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login", "Account");

        return View(user);
    }

    // POST: /UserProfile/Edit
    [HttpPost("edit")]
    public async Task<IActionResult> Edit(AppUser model, IFormFile? profileImage)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login", "Account");

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Biography = model.Biography ?? "";
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;

        // Profil resmi yükleme - sunucu tarafı doğrulama
        if (profileImage != null && profileImage.Length > 0)
        {
            // Maksimum 2 MB
            const long maxBytes = 2 * 1024 * 1024;
            if (profileImage.Length > maxBytes)
            {
                ModelState.AddModelError("profileImage", "Dosya çok büyük. Maksimum 2 MB.");
                return View(user);
            }

            // İçerik tipi kontrolü
            if (!profileImage.ContentType.StartsWith("image/"))
            {
                ModelState.AddModelError("profileImage", "Geçersiz dosya türü. Lütfen bir resim yükleyin.");
                return View(user);
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "profiles");
            Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(profileImage.FileName);
            if (string.IsNullOrEmpty(ext)) ext = ".png";

            var safeFileName = $"{user.Id}_{DateTime.UtcNow.Ticks}{ext}";
            var filePath = Path.Combine(uploadsFolder, safeFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profileImage.CopyToAsync(stream);
            }

            user.ProfileImageUrl = $"/uploads/profiles/{safeFileName}";
        }

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Profil başarıyla güncellenmiştir.";
            return RedirectToAction("View");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(user);
    }

    // GET: /UserProfile/View/{id?}
    [HttpGet("view/{id?}")]
    public async Task<IActionResult> View(int? id)
    {
        AppUser user;

        if (id.HasValue)
        {
            user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound();
        }
        else
        {
            user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");
        }

        // Get user statistics
        var postCount = _context.ForumThreads.Count(p => p.AuthorId == user.Id);
        var answerCount = _context.Posts.Count(p => p.AuthorId == user.Id && !p.IsDeleted);
        var likesReceived = _context.PostLikes.Count(l =>
            _context.Posts.Any(p => p.Id == l.PostId && p.AuthorId == user.Id));

        var recentPosts = _context.ForumThreads
            .Where(p => p.AuthorId == user.Id)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToList();

        ViewBag.PostCount = postCount;
        ViewBag.AnswerCount = answerCount;
        ViewBag.LikesReceived = likesReceived;
        ViewBag.RecentPosts = recentPosts;

        return View(user);
    }
}
