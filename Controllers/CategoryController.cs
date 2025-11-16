using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnivForm.Data;
using UnivForm.Models;
using UnivForm.Models.ViewModels;

namespace UnivForm.Controllers
{
    // --- DEĞİŞİKLİK BURADA ---
    // Artık tüm controller'ı Admin'e kilitlemiyoruz,
    // Sadece "giriş yapmış" olmalarını şart koşuyoruz.
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        // Bu metot [Authorize] sayesinde sadece giriş yapanlar tarafından görülebilir.
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        // Bu metot [Authorize] sayesinde sadece giriş yapanlar tarafından görülebilir.
        public IActionResult Create()
        {
            return View();
        }

        // Bu metot [Authorize] sayesinde sadece giriş yapanlar tarafından kullanılabilir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var category = new Category
                {
                    Title = model.Title,
                    Description = model.Description
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return RedirectToAction("CreateThread", "Forum");
            }
            return View(model);
        }

        // --- YENİ EKLENDİ ---
        // Sadece Admin'ler bu sayfayı görebilir.
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Category'nin null kontrolünü yapın (Önceki kodunuzdan alındı)
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // --- YENİ EKLENDİ ---
        // Sadece Admin'ler bu işlemi yapabilir.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            // Threads koleksiyonunu null olarak ayarlayarak validasyon hatasını önle
            // (Bir önceki düzenlememizdeki gibi)
            category.Threads = null;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categories.Any(e => e.Id == category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // --- YENİ EKLENDİ ---
        // Sadece Admin'ler bu sayfayı görebilir.
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // --- YENİ EKLENDİ ---
        // Sadece Admin'ler bu işlemi yapabilir.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}