using InvestTrack.Model.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvestTrack.Web.Controllers
{
    public class SymbolsController : Controller
    {
        private readonly InvestTrackDbContext _db;

        public SymbolsController(InvestTrackDbContext db)
        {
            _db = db;
        }

        // GET: /Symbols
        public async Task<IActionResult> Index()
        {
            var symbols = await _db.Symbols
                .OrderBy(s => s.Code)
                .ToListAsync();

            var categories = await _db.Symbols
                .Select(s => s.Category)
                .Distinct()
                .ToListAsync();

            ViewBag.Categories = categories;
            return View(symbols);
        }

        // GET: /Symbols/FilterJson?category=xxx
        [HttpGet]
        public async Task<IActionResult> FilterJson(string? category)
        {
            var query = _db.Symbols.AsQueryable();

            if (!string.IsNullOrWhiteSpace(category) && category != "ALL")
            {
                query = query.Where(s => s.Category == category);
            }

            var symbols = await query.OrderBy(s => s.Code).Select(s => new {
                s.Id,
                s.Code,
                s.DisplayName,
                s.Category
            }).ToListAsync();

            return Json(symbols);
        }
    }
}
