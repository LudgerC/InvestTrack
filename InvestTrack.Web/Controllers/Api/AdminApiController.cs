using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestTrack.Model.Data;
using InvestTrack.Model.Identity;
using InvestTrack.Model.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvestTrack.Web.Controllers.Api
{
    [ApiController]
    [Route("api/admin")]
    [Route("api/[controller]")]
    public class AdminApiController : ControllerBase
    {
        private readonly InvestTrackDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminApiController(
            InvestTrackDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public class AdminDashboardDto
        {
            public List<AdminUserDto> Users { get; set; } = new();
            public List<AdminAccountDto> Accounts { get; set; } = new();
            public List<AdminTradeDto> Trades { get; set; } = new();
            public List<SymbolDto> Symbols { get; set; } = new();
        }

        public class AdminUserDto
        {
            public string Id { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }

        public class AdminAccountDto
        {
            public int AccountId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Currency { get; set; } = string.Empty;
            public decimal Balance { get; set; }
            public string UserId { get; set; } = string.Empty;
            public string UserEmail { get; set; } = string.Empty;
        }

        public class AdminTradeDto
        {
            public int TradeId { get; set; }
            public string SymbolCode { get; set; } = string.Empty;
            public decimal Lots { get; set; }
            public decimal ProfitLoss { get; set; }
            public string AccountName { get; set; } = string.Empty;
        }

        public class SymbolDto
        {
            public int SymbolId { get; set; }
            public string Code { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
        }

        public class CreateUserRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Role { get; set; } = "Trader";
            public string? FullName { get; set; }
        }

        public class AddSymbolRequest
        {
            public string Code { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public string Category { get; set; } = "Algemeen";
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var rawUsers = await _db.Users.ToListAsync();
                var userList = new List<AdminUserDto>();

                foreach (var u in rawUsers)
                {
                    var roles = await _userManager.GetRolesAsync(u);
                    userList.Add(new AdminUserDto
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
                    .Select(a => new AdminAccountDto
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
                    .Select(t => new AdminTradeDto
                    {
                        TradeId = t.TradeId,
                        SymbolCode = t.Symbol != null ? t.Symbol.Code : "N/B",
                        Lots = t.Lots,
                        ProfitLoss = t.ProfitLoss,
                        AccountName = t.Account != null ? t.Account.Name : "Onbekend"
                    })
                    .ToListAsync();

                var symbols = await _db.Symbols
                    .OrderBy(s => s.Code)
                    .Select(s => new SymbolDto
                    {
                        SymbolId = s.Id,
                        Code = s.Code,
                        DisplayName = s.DisplayName,
                        Category = s.Category
                    })
                    .ToListAsync();

                return Ok(new AdminDashboardDto
                {
                    Users = userList,
                    Accounts = accounts,
                    Trades = trades,
                    Symbols = symbols
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "E-mailadres en wachtwoord zijn verplicht." });

            var existing = await _userManager.FindByEmailAsync(request.Email.Trim());
            if (existing != null)
                return BadRequest(new { message = "E-mailadres is al in gebruik." });

            var user = new ApplicationUser
            {
                UserName = request.Email.Trim(),
                Email = request.Email.Trim(),
                FullName = string.IsNullOrWhiteSpace(request.FullName) ? request.Email.Trim() : request.FullName.Trim()
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return BadRequest(new { message = string.Join("; ", result.Errors.Select(e => e.Description)) });

            string targetRole = string.IsNullOrWhiteSpace(request.Role) ? "Trader" : request.Role;
            if (!await _roleManager.RoleExistsAsync(targetRole))
                await _roleManager.CreateAsync(new IdentityRole(targetRole));

            await _userManager.AddToRoleAsync(user, targetRole);

            return Ok(new { success = true, message = $"Gebruiker '{user.Email}' aangemaakt." });
        }

        [HttpPost("delete-user")]
        public async Task<IActionResult> DeleteUser([FromBody] string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(new { message = "Gebruiker niet gevonden." });

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { message = "Kon gebruiker niet verwijderen." });

            return Ok(new { success = true });
        }

        [HttpPost("add-symbol")]
        public async Task<IActionResult> AddSymbol([FromBody] AddSymbolRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.DisplayName))
                return BadRequest(new { message = "Code en naam zijn verplicht." });

            var symbol = new Symbol
            {
                Code = request.Code.Trim().ToUpper(),
                DisplayName = request.DisplayName.Trim(),
                Category = string.IsNullOrWhiteSpace(request.Category) ? "Algemeen" : request.Category.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _db.Symbols.Add(symbol);
            await _db.SaveChangesAsync();

            return Ok(new { success = true, symbolId = symbol.Id });
        }

        [HttpPost("delete-symbol")]
        public async Task<IActionResult> DeleteSymbol([FromBody] int symbolId)
        {
            var symbol = await _db.Symbols.FindAsync(symbolId);
            if (symbol == null) return NotFound();

            _db.Symbols.Remove(symbol);
            await _db.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpPost("delete-account")]
        public async Task<IActionResult> DeleteAccount([FromBody] int accountId)
        {
            var account = await _db.Accounts.Include(a => a.Trades).FirstOrDefaultAsync(a => a.AccountId == accountId);
            if (account == null) return NotFound();

            foreach (var trade in account.Trades)
            {
                trade.IsDeleted = true;
            }
            account.IsDeleted = true;
            await _db.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpPost("delete-trade")]
        public async Task<IActionResult> DeleteTrade([FromBody] int tradeId)
        {
            var trade = await _db.Trades.FirstOrDefaultAsync(t => t.TradeId == tradeId);
            if (trade == null) return NotFound();

            trade.IsDeleted = true;
            await _db.SaveChangesAsync();

            return Ok(new { success = true });
        }
    }
}
