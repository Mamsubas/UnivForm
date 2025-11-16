using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using UnivForm.Models;
using UnivForm.Data;

namespace UnivForm.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, AppDbContext context)
    {
        _logger = logger;
        _userManager = userManager;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login", "Account");

        // Get user's recent posts/answers
        var recentPosts = _context.ForumThreads
            .Where(p => p.AuthorId == user.Id)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToList();

        var recentReplies = _context.Posts
            .Where(p => p.AuthorId == user.Id && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToList();

        // Statistics
        var postCount = _context.ForumThreads.Count(p => p.AuthorId == user.Id);
        var answerCount = _context.Posts.Count(p => p.AuthorId == user.Id && !p.IsDeleted);
        var likesReceived = _context.PostLikes.Count(l =>
            _context.Posts.Any(p => p.Id == l.PostId && p.AuthorId == user.Id));

        ViewBag.User = user;
        ViewBag.PostCount = postCount;
        ViewBag.AnswerCount = answerCount;
        ViewBag.LikesReceived = likesReceived;
        ViewBag.RecentPosts = recentPosts;
        ViewBag.RecentReplies = recentReplies;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
    public IActionResult More()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
