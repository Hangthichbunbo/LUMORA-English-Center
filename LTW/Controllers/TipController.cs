using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTW.Data;
using LTW.Models;

namespace LTW.Controllers
{
    public class TipController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TipController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Tip/Index
        [HttpGet]
        public async Task<IActionResult> Index(string? category, string? search)
        {
            var query = _context.IeltsTips.Where(t => t.IsPublished);

            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                query = query.Where(t => t.Category.ToLower() == category.ToLower());
            }

            if (!string.IsNullOrEmpty(search))
            {
                var s = search.ToLower().Trim();
                query = query.Where(t => t.Title.ToLower().Contains(s) || t.Summary.ToLower().Contains(s));
            }

            var tips = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

            ViewBag.CurrentCategory = category ?? "All";
            ViewBag.CurrentSearch = search ?? "";

            return View(tips);
        }

        // GET: /Tip/Detail/5
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var tip = await _context.IeltsTips.FirstOrDefaultAsync(t => t.TipId == id && t.IsPublished);
            if (tip == null)
            {
                return NotFound();
            }

            // Increment views
            tip.ViewsCount++;
            await _context.SaveChangesAsync();

            // Related articles
            var related = await _context.IeltsTips
                .Where(t => t.TipId != id && t.IsPublished && (t.Category == tip.Category || true))
                .OrderByDescending(t => t.CreatedAt)
                .Take(3)
                .ToListAsync();

            ViewBag.RelatedTips = related;
            return View(tip);
        }
    }
}

