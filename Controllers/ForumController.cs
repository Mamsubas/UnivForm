using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using UnivForm.Data;
using UnivForm.Models;
using UnivForm.Models.ViewModels;
using System.Security.Claims;

namespace UnivForm.Controllers
{
    public class ForumController : Controller
    {
        private readonly ILogger<ForumController> _logger;
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ForumController(ILogger<ForumController> logger, AppDbContext context, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }


        public async Task<IActionResult> Index()
        {

            var categoriesQuery = _context.Categories
                .Include(c => c.Threads!)
                    .ThenInclude(t => t.Author)
                .Include(c => c.Threads!)
                    .ThenInclude(t => t.Posts);

            var categories = await categoriesQuery.AsNoTracking().ToListAsync();

            return View(categories);

        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CreateThread()
        {
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Title");
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateThread(CreateThreadViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Challenge();
                }

                var thread = new ForumThread
                {
                    Title = model.Title,
                    Content = model.Content,
                    CategoryId = model.CategoryId,
                    AuthorId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ForumThreads.Add(thread);
                await _context.SaveChangesAsync();

                return RedirectToAction("ThreadDetail", new { id = thread.Id });
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Title", model.CategoryId);
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> ThreadDetail(int id)
        {
            var thread = await _context.ForumThreads
                .Include(t => t.Author)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (thread == null)
            {
                return NotFound();
            }


            int currentUserId = 0;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

                int.TryParse(userIdString, out currentUserId);
            }



            var allPosts = await _context.Posts
                .Where(p => p.ForumThreadId == id)
                .Include(p => p.Author)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();


            var postIds = allPosts.Select(p => p.Id).ToList();
            var allLikes = await _context.PostLikes
                .Where(pl => postIds.Contains(pl.PostId))
                .AsNoTracking()
                .ToListAsync();


            var allPostVMs = allPosts.Select(post => new PostViewModel
            {
                Post = post,
                LikeCount = allLikes.Count(l => l.PostId == post.Id),

                UserHasLiked = allLikes.Any(l => l.PostId == post.Id && l.UserId == currentUserId),
                Replies = new List<PostViewModel>()
            }).ToList();


            var hierarchicalPosts = new List<PostViewModel>();
            var postVmDictionary = allPostVMs.ToDictionary(p => p.Post.Id);

            foreach (var vm in allPostVMs)
            {
                if (vm.Post.ParentPostId == null)
                {
                    hierarchicalPosts.Add(vm);
                }
                else
                {
                    if (postVmDictionary.TryGetValue(vm.Post.ParentPostId.Value, out var parentVm))
                    {
                        parentVm.Replies.Add(vm);
                    }
                }
            }

            var model = new ThreadDetailViewModel
            {
                Thread = thread,
                Posts = hierarchicalPosts,
                NewPost = new CreatePostViewModel { ThreadId = id }
            };

            return View(model);
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPost([Bind(Prefix = "NewPost")] CreatePostViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Challenge();
                }

                var post = new Post
                {
                    Content = model.Content,
                    ForumThreadId = model.ThreadId,
                    AuthorId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    ParentPostId = model.ParentPostId
                };

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                return RedirectToAction("ThreadDetail", new { id = model.ThreadId });
            }

            return RedirectToAction("ThreadDetail", new { id = model.ThreadId });
        }



        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound();
            }


            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var currentUserId))
            {

                return Challenge();
            }


            var existingLike = await _context.PostLikes
                .FirstOrDefaultAsync(pl => pl.PostId == postId && pl.UserId == currentUserId);

            if (existingLike == null)
            {
                _context.PostLikes.Add(new PostLike
                {
                    PostId = postId,
                    UserId = currentUserId,
                    LikedAt = DateTime.UtcNow
                });
            }
            else
            {
                _context.PostLikes.Remove(existingLike);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("ThreadDetail", new { id = post.ForumThreadId });
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound();
            }


            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var currentUserId))
            {
                return Challenge();
            }



            if (post.AuthorId == currentUserId || User.IsInRole("Admin"))
            {
                post.IsDeleted = true;
                post.Content = "[Bu yorum, yazarı tarafından silinmiştir]";
                post.EditedAt = DateTime.UtcNow;

                _context.Update(post);
                await _context.SaveChangesAsync();
            }
            else
            {
                return Forbid();
            }

            return RedirectToAction("ThreadDetail", new { id = post.ForumThreadId });
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditPost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null || post.IsDeleted)
            {
                return NotFound();
            }


            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var currentUserId))
            {
                return Challenge();
            }



            if (post.AuthorId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(post);
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, [FromForm] string content)
        {

            if (string.IsNullOrWhiteSpace(content))
            {

                TempData["EditPostError"] = "Yorum içeriği boş olamaz.";
                return RedirectToAction("EditPost", new { id = id });
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null || post.IsDeleted)
            {
                return NotFound();
            }


            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var currentUserId))
            {
                return Challenge();
            }



            if (post.AuthorId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            post.Content = content;
            post.EditedAt = DateTime.UtcNow;
            _context.Update(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("ThreadDetail", new { id = post.ForumThreadId });
        }
    }
}