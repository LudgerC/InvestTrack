using System;
using System.Linq;
using System.Threading.Tasks;
using InvestTrack.Model.Data;
using InvestTrack.Model.Identity;
using InvestTrack.Model.Models;
using InvestTrack.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvestTrack.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly InvestTrackDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            InvestTrackDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var rawUsers = await _db.Users.ToListAsync();
            var userList = new System.Collections.Generic.List<ApplicationUserViewModel>();

            foreach (var u in rawUsers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                userList.Add(new ApplicationUserViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName ?? "",
                    Email = u.Email ?? "",
                    FullName = u.FullName ?? u.UserName ?? "",
                    Role = roles.FirstOrDefault() ?? "Trader"
                });
            }

            var accounts = await _db.Accounts
                .Include(a => a.User)
                .Where(a => !a.IsDeleted)
                .Select(a => new AdminAccountItemViewModel
                {
                    AccountId = a.AccountId,
                    Name = a.Name,
                    Currency = a.Currency,
                    Balance = a.Balance,
                    UserId = a.UserId,
                    UserEmail = a.User != null ? a.User.Email ?? "(geen e-mail)" : "(geen e-mail)"
                })
                .ToListAsync();

            var trades = await _db.Trades
                .Include(t => t.Symbol)
                .Include(t => t.Account)
                .Where(t => !t.IsDeleted)
                .Select(t => new AdminTradeItemViewModel
                {
                    TradeId = t.TradeId,
                    SymbolCode = t.Symbol != null ? t.Symbol.Code : "N/B",
                    Lots = t.Lots,
                    ProfitLoss = t.ProfitLoss,
                    AccountName = t.Account != null ? t.Account.Name : "Onbekend"
                })
                .ToListAsync();

            var symbols = await _db.Symbols.OrderBy(s => s.Code).ToListAsync();

            var model = new AdminDashboardViewModel
            {
                Users = userList,
                Accounts = accounts,
                Trades = trades,
                Symbols = symbols
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(string email, string password, string role, string? fullName)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["ErrorMessage"] = "Vul e-mailadres en wachtwoord in.";
                return RedirectToAction("Index");
            }

            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                TempData["ErrorMessage"] = "E-mailadres is al in gebruik.";
                return RedirectToAction("Index");
            }

            var user = new ApplicationUser
            {
                UserName = email.Trim(),
                Email = email.Trim(),
                FullName = string.IsNullOrWhiteSpace(fullName) ? email.Trim() : fullName.Trim()
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join("; ", result.Errors.Select(e => e.Description));
                return RedirectToAction("Index");
            }

            string targetRole = string.IsNullOrWhiteSpace(role) ? "Trader" : role;
            if (!await _roleManager.RoleExistsAsync(targetRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(targetRole));
            }

            await _userManager.AddToRoleAsync(user, targetRole);

            TempData["SuccessMessage"] = $"Gebruiker '{email}' ({targetRole}) succesvol aangemaakt!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
                TempData["SuccessMessage"] = $"Gebruiker '{user.Email}' verwijderd.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(int accountId)
        {
            var account = await _db.Accounts.Include(a => a.Trades).FirstOrDefaultAsync(a => a.AccountId == accountId);
            if (account != null)
            {
                foreach (var trade in account.Trades)
                {
                    trade.IsDeleted = true;
                }
                account.IsDeleted = true;
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Account '{account.Name}' en de bijbehorende trades soft-deleted.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTrade(int tradeId)
        {
            var trade = await _db.Trades.FirstOrDefaultAsync(t => t.TradeId == tradeId);
            if (trade != null)
            {
                trade.IsDeleted = true;
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Trade #{tradeId} soft-deleted.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSymbol(string code, string displayName, string category)
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(displayName))
            {
                TempData["ErrorMessage"] = "Code en naam zijn verplicht.";
                return RedirectToAction("Index");
            }

            var symbol = new Symbol
            {
                Code = code.Trim().ToUpper(),
                DisplayName = displayName.Trim(),
                Category = string.IsNullOrWhiteSpace(category) ? "Algemeen" : category.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _db.Symbols.Add(symbol);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Symbool '{symbol.Code}' toegevoegd.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSymbol(int symbolId)
        {
            var symbol = await _db.Symbols.FindAsync(symbolId);
            if (symbol != null)
            {
                _db.Symbols.Remove(symbol);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Symbool '{symbol.Code}' verwijderd.";
            }

            return RedirectToAction("Index");
        }
    }
}
