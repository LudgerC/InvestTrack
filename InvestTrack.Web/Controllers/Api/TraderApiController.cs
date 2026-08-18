using System;
using System.Linq;
using System.Threading.Tasks;
using InvestTrack.Model.Data;
using InvestTrack.Model.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvestTrack.Web.Controllers.Api
{
    [ApiController]
    [Route("api/trader")]
    [Route("api/[controller]")]
    public class TraderApiController : ControllerBase
    {
        private readonly InvestTrackDbContext _context;

        public TraderApiController(InvestTrackDbContext context)
        {
            _context = context;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard([FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("UserId is verplicht.");
            }

            var accounts = await _context.Accounts
                .AsNoTracking()
                .Where(a => a.UserId == userId && !a.IsDeleted)
                .ToListAsync();

            var favoriteTradeIds = await _context.FavoriteTrades
                .AsNoTracking()
                .Where(f => !f.IsDeleted)
                .Select(f => f.TradeId)
                .ToListAsync();

            var trades = await _context.Trades
                .Include(t => t.Symbol)
                .Include(t => t.Account)
                .AsNoTracking()
                .Where(t => t.Account != null && t.Account.UserId == userId && !t.IsDeleted)
                .Select(t => new
                {
                    t.TradeId,
                    SymbolCode = t.Symbol != null ? t.Symbol.Code : "N/B",
                    SymbolName = t.Symbol != null ? t.Symbol.DisplayName : "Onbekend",
                    t.Lots,
                    t.ProfitLoss,
                    AccountName = t.Account != null ? t.Account.Name : "Onbekend",
                    t.AccountId
                })
                .ToListAsync();

            var tradesWithFav = trades.Select(t => new
            {
                t.TradeId,
                t.SymbolCode,
                t.SymbolName,
                t.Lots,
                t.ProfitLoss,
                t.AccountName,
                t.AccountId,
                IsFavorite = favoriteTradeIds.Contains(t.TradeId)
            }).ToList();

            var symbols = await _context.Symbols
                .AsNoTracking()
                .OrderBy(s => s.DisplayName)
                .Select(s => new
                {
                    SymbolId = s.Id,
                    s.Code,
                    s.DisplayName,
                    s.Category
                })
                .ToListAsync();

            return Ok(new
            {
                TotalBalance = accounts.Sum(a => a.Balance),
                Accounts = accounts,
                Trades = tradesWithFav,
                Symbols = symbols
            });
        }

        [HttpGet("favorites")]
        public async Task<IActionResult> GetFavorites([FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId is verplicht.");

            var favorites = await _context.FavoriteTrades
                .Include(f => f.Trade).ThenInclude(t => t.Symbol)
                .Include(f => f.Trade).ThenInclude(t => t.Account)
                .AsNoTracking()
                .Where(f => !f.IsDeleted && f.Trade != null && !f.Trade.IsDeleted
                         && f.Trade.Account != null && f.Trade.Account.UserId == userId)
                .Select(f => new
                {
                    f.Trade!.TradeId,
                    SymbolCode = f.Trade.Symbol != null ? f.Trade.Symbol.Code : "N/B",
                    SymbolName = f.Trade.Symbol != null ? f.Trade.Symbol.DisplayName : "Onbekend",
                    f.Trade.Lots,
                    f.Trade.ProfitLoss,
                    AccountName = f.Trade.Account != null ? f.Trade.Account.Name : "Onbekend",
                    f.Trade.AccountId,
                    IsFavorite = true
                })
                .ToListAsync();

            return Ok(favorites);
        }

        public class CreateAccountRequest
        {
            public string UserId { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Currency { get; set; } = "EUR";
            public decimal InitialBalance { get; set; } = 1000.00m;
        }

        [HttpPost("account")]
        public async Task<IActionResult> AddAccount([FromBody] CreateAccountRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("UserId en Name zijn verplicht.");
            }

            var account = new Account
            {
                UserId = request.UserId,
                Name = request.Name.Trim(),
                AccountName = request.Name.Trim(),
                Currency = request.Currency,
                Balance = request.InitialBalance
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Ok(account);
        }

        public class CreateTradeRequest
        {
            public int AccountId { get; set; }
            public int SymbolId { get; set; }
            public string? SymbolCode { get; set; }
            public decimal Lots { get; set; }
            public decimal ProfitLoss { get; set; }
            public decimal EntryPrice { get; set; } = 0;
            public decimal ExitPrice { get; set; } = 0;
        }

        [HttpPost("trade")]
        public async Task<IActionResult> AddTrade([FromBody] CreateTradeRequest request)
        {
            if (request.AccountId <= 0)
                return BadRequest("AccountId is ongeldig.");
            if (request.Lots <= 0)
                return BadRequest("Lots moet groter zijn dan 0.");

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == request.AccountId);
            if (account == null)
                return NotFound("Account niet gevonden.");

            Symbol? symbol = null;
            if (request.SymbolId > 0)
            {
                symbol = await _context.Symbols.FirstOrDefaultAsync(s => s.Id == request.SymbolId);
            }
            if (symbol == null && !string.IsNullOrWhiteSpace(request.SymbolCode))
            {
                symbol = await _context.Symbols.FirstOrDefaultAsync(s => s.Code == request.SymbolCode.Trim());
            }

            if (symbol == null)
            {
                return BadRequest($"Symbool niet gevonden (ID: {request.SymbolId}, Code: {request.SymbolCode}).");
            }

            var trade = new Trade
            {
                AccountId = request.AccountId,
                SymbolId = symbol.Id,
                Lots = request.Lots,
                ProfitLoss = request.ProfitLoss,
                EntryPrice = request.EntryPrice,
                ExitPrice = request.ExitPrice
            };

            try
            {
                _context.Trades.Add(trade);
                account.Balance += request.ProfitLoss;
                await _context.SaveChangesAsync();
                return Ok(trade);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database fout: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        public class TransactionRequest
        {
            public int AccountId { get; set; }
            public decimal Amount { get; set; }
            public string? Note { get; set; }
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] TransactionRequest request)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == request.AccountId);
            if (account == null || request.Amount <= 0)
                return BadRequest("Ongeldig account of bedrag.");

            _context.Transactions.Add(new Transaction
            {
                AccountId = request.AccountId,
                Amount = request.Amount,
                Type = "Deposit",
                Note = request.Note,
                CreatedAt = DateTime.UtcNow
            });
            account.Balance += request.Amount;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] TransactionRequest request)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == request.AccountId);
            if (account == null || request.Amount <= 0)
                return BadRequest("Ongeldig account of bedrag.");
            if (account.Balance - request.Amount < 0)
                return BadRequest("Onvoldoende saldo.");

            _context.Transactions.Add(new Transaction
            {
                AccountId = request.AccountId,
                Amount = -request.Amount,
                Type = "Withdrawal",
                Note = request.Note,
                CreatedAt = DateTime.UtcNow
            });
            account.Balance -= request.Amount;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("deletetrade")]
        public async Task<IActionResult> DeleteTrade([FromBody] int tradeId)
        {
            var trade = await _context.Trades
                .Include(t => t.Account)
                .FirstOrDefaultAsync(t => t.TradeId == tradeId);

            if (trade == null) return NotFound();

            if (trade.Account != null)
                trade.Account.Balance -= trade.ProfitLoss;

            trade.IsDeleted = true;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("togglefavorite")]
        public async Task<IActionResult> ToggleFavorite([FromBody] int tradeId)
        {
            var favorite = await _context.FavoriteTrades.FirstOrDefaultAsync(f => f.TradeId == tradeId);
            if (favorite == null)
                _context.FavoriteTrades.Add(new FavoriteTrade { TradeId = tradeId });
            else
                _context.FavoriteTrades.Remove(favorite);

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
