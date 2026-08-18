using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using InvestTrack.Model.Data;
using InvestTrack.Model.Models;
using InvestTrack.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvestTrack.Web.Controllers
{
    [Authorize(Roles = "Trader,Admin")]
    public class TraderController : Controller
    {
        private readonly InvestTrackDbContext _context;

        public TraderController(InvestTrackDbContext context)
        {
            _context = context;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int accountId = 0, int favAccountId = 0, string category = "Alle")
        {
            var userId = GetCurrentUserId();

            // Accounts of current user
            var accounts = await _context.Accounts
                .AsNoTracking()
                .Where(a => a.UserId == userId && !a.IsDeleted)
                .ToListAsync();

            // Total Balance
            decimal totalBalance = accounts.Sum(a => a.Balance);

            // Trades Query
            var tradesQuery = _context.Trades
                .Include(t => t.Symbol)
                .Include(t => t.Account)
                .AsNoTracking()
                .Where(t => t.Account != null && t.Account.UserId == userId && !t.IsDeleted);

            if (accountId > 0)
            {
                tradesQuery = tradesQuery.Where(t => t.AccountId == accountId);
            }

            var tradesList = await tradesQuery.ToListAsync();
            var favoriteTradeIds = await _context.FavoriteTrades
                .AsNoTracking()
                .Where(f => !f.IsDeleted)
                .Select(f => f.TradeId)
                .ToListAsync();

            var trades = tradesList.Select(t => new TradeItemViewModel
            {
                TradeId = t.TradeId,
                SymbolCode = t.Symbol?.Code ?? "N/B",
                SymbolName = t.Symbol?.DisplayName ?? "Onbekend",
                Lots = t.Lots,
                ProfitLoss = t.ProfitLoss,
                AccountName = t.Account?.Name ?? t.Account?.AccountName ?? "Onbekend",
                AccountId = t.AccountId,
                IsFavorite = favoriteTradeIds.Contains(t.TradeId)
            }).ToList();

            // Favorites Query
            var favoritesQuery = _context.FavoriteTrades
                .Include(f => f.Trade)
                .ThenInclude(t => t.Symbol)
                .Include(f => f.Trade)
                .ThenInclude(t => t.Account)
                .AsNoTracking()
                .Where(f => f.Trade != null && f.Trade.Account != null && f.Trade.Account.UserId == userId && !f.Trade.IsDeleted && !f.IsDeleted);

            if (favAccountId > 0)
            {
                favoritesQuery = favoritesQuery.Where(f => f.Trade.AccountId == favAccountId);
            }

            var favList = await favoritesQuery.ToListAsync();
            var favorites = favList.Where(f => f.Trade != null).Select(f => new TradeItemViewModel
            {
                TradeId = f.Trade!.TradeId,
                SymbolCode = f.Trade.Symbol?.Code ?? "N/B",
                SymbolName = f.Trade.Symbol?.DisplayName ?? "Onbekend",
                Lots = f.Trade.Lots,
                ProfitLoss = f.Trade.ProfitLoss,
                AccountName = f.Trade.Account?.Name ?? f.Trade.Account?.AccountName ?? "Onbekend",
                AccountId = f.Trade.AccountId,
                IsFavorite = true
            }).ToList();

            // Symbols & Categories
            var categories = await _context.Symbols
                .Where(s => s.Category != null)
                .Select(s => s.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
            categories.Insert(0, "Alle");

            var symbolsQuery = _context.Symbols.AsNoTracking();
            if (!string.IsNullOrEmpty(category) && category != "Alle")
            {
                symbolsQuery = symbolsQuery.Where(s => s.Category == category);
            }
            var symbols = await symbolsQuery.OrderBy(s => s.DisplayName).ToListAsync();

            var model = new TraderDashboardViewModel
            {
                Accounts = accounts,
                Trades = trades,
                Favorites = favorites,
                Symbols = symbols,
                SelectedAccountId = accountId,
                SelectedFavoritesAccountId = favAccountId,
                SelectedCategory = category,
                Categories = categories,
                TotalBalance = totalBalance,
                TotalProfitLoss = trades.Sum(t => t.ProfitLoss),
                TotalTradesCount = trades.Count
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAccount(string name, string currency, decimal initialBalance)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "Accountnaam is verplicht.";
                return RedirectToAction("Index");
            }

            var account = new Account
            {
                UserId = userId,
                Name = name.Trim(),
                AccountName = name.Trim(),
                Currency = string.IsNullOrWhiteSpace(currency) ? "EUR" : currency.Trim().ToUpper(),
                Balance = initialBalance
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Nieuw account '{account.Name}' succesvol aangemaakt!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTrade(int accountId, int symbolId, decimal lots, decimal profitLoss)
        {
            var userId = GetCurrentUserId();
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId && a.UserId == userId);

            if (account == null)
            {
                TempData["ErrorMessage"] = "Selecteer een geldig account.";
                return RedirectToAction("Index");
            }

            if (lots <= 0)
            {
                TempData["ErrorMessage"] = "Voer een geldig aantal lots in.";
                return RedirectToAction("Index");
            }

            var trade = new Trade
            {
                AccountId = accountId,
                SymbolId = symbolId,
                Lots = lots,
                ProfitLoss = profitLoss
            };

            _context.Trades.Add(trade);
            account.Balance += profitLoss;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Trade succesvol toegevoegd!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DepositTransaction(int accountId, decimal amount, string? note)
        {
            var userId = GetCurrentUserId();
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId && a.UserId == userId);

            if (account == null || amount <= 0)
            {
                TempData["ErrorMessage"] = "Voer een geldig deposit bedrag in.";
                return RedirectToAction("Index");
            }

            var transaction = new Transaction
            {
                AccountId = accountId,
                Amount = amount,
                Type = "Deposit",
                Note = note,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            account.Balance += amount;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Storting van €{amount:N2} succesvol verwerkt!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WithdrawTransaction(int accountId, decimal amount, string? note)
        {
            var userId = GetCurrentUserId();
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountId && a.UserId == userId);

            if (account == null || amount <= 0)
            {
                TempData["ErrorMessage"] = "Voer een geldig opname bedrag in.";
                return RedirectToAction("Index");
            }

            if (account.Balance - amount < 0)
            {
                TempData["ErrorMessage"] = "Onvoldoende saldo voor deze opname.";
                return RedirectToAction("Index");
            }

            var transaction = new Transaction
            {
                AccountId = accountId,
                Amount = -amount,
                Type = "Withdrawal",
                Note = note,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            account.Balance -= amount;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Opname van €{amount:N2} succesvol verwerkt!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int tradeId)
        {
            var favorite = await _context.FavoriteTrades.FirstOrDefaultAsync(f => f.TradeId == tradeId);
            if (favorite == null)
            {
                _context.FavoriteTrades.Add(new FavoriteTrade { TradeId = tradeId });
            }
            else
            {
                _context.FavoriteTrades.Remove(favorite);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTrade(int tradeId)
        {
            var userId = GetCurrentUserId();
            var trade = await _context.Trades
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.TradeId == tradeId && t.Account != null && t.Account.UserId == userId);

            if (trade != null)
            {
                if (trade.Account != null)
                {
                    trade.Account.Balance -= trade.ProfitLoss;
                }
                trade.IsDeleted = true;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Trade verwijderd en saldo aangepast.";
            }

            return RedirectToAction("Index");
        }
    }
}
