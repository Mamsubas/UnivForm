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
        // Dinamik veriler
        var totalUsers = _context.Users.Count(u => u.IsActive);
        var activeUsers = _context.Users.Count(u => u.IsActive && u.LastLogin.HasValue &&
            u.LastLogin.Value.AddDays(7) > DateTime.Now);
        var totalQuestions = _context.ForumThreads.Count(t => !t.IsDeleted);
        var totalAnswers = _context.Posts.Count(p => !p.IsDeleted);

        // Popüler konular (en çok yazı olan thread'ler)
        var popularTopics = _context.ForumThreads
            .Where(t => !t.IsDeleted)
            .OrderByDescending(t => t.Posts.Count())
            .ThenByDescending(t => t.ViewCount)
            .Take(10)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.CreatedAt,
                PostCount = t.Posts.Count(p => !p.IsDeleted),
                t.ViewCount,
                Author = t.Author.FirstName + " " + t.Author.LastName
            })
            .ToList();

        ViewBag.TotalUsers = totalUsers;
        ViewBag.ActiveUsers = activeUsers;
        ViewBag.TotalQuestions = totalQuestions;
        ViewBag.TotalAnswers = totalAnswers;
        ViewBag.PopularTopics = popularTopics;

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

    public IActionResult PopularTopics(int page = 1, string sortBy = "posts")
    {
        int pageSize = 15;

        var query = _context.ForumThreads
            .Where(t => !t.IsDeleted)
            .AsQueryable();

        // Sıralama seçeneğine göre
        query = sortBy switch
        {
            "views" => query.OrderByDescending(t => t.ViewCount).ThenByDescending(t => t.Posts.Count()),
            "recent" => query.OrderByDescending(t => t.CreatedAt),
            _ => query.OrderByDescending(t => t.Posts.Count()).ThenByDescending(t => t.ViewCount)
        };

        var totalCount = query.Count();
        var topics = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.CreatedAt,
                t.CategoryId,
                Category = t.Category.Title,
                PostCount = t.Posts.Count(p => !p.IsDeleted),
                t.ViewCount,
                Author = t.Author.FirstName + " " + t.Author.LastName,
                AuthorId = t.Author.Id
            })
            .ToList();

        ViewBag.Topics = topics;
        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = totalCount;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        ViewBag.SortBy = sortBy;

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
