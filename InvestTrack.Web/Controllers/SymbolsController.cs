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

            return View(symbols);
        }
    }
}
