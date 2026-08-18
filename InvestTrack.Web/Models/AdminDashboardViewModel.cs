using System.Collections.Generic;
using InvestTrack.Model.Identity;
using InvestTrack.Model.Models;

namespace InvestTrack.Web.Models
{
    public class AdminDashboardViewModel
    {
        public List<ApplicationUserViewModel> Users { get; set; } = new();
        public List<AdminAccountItemViewModel> Accounts { get; set; } = new();
        public List<AdminTradeItemViewModel> Trades { get; set; } = new();
        public List<Symbol> Symbols { get; set; } = new();
    }

    public class ApplicationUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class AdminAccountItemViewModel
    {
        public int AccountId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
    }

    public class AdminTradeItemViewModel
    {
        public int TradeId { get; set; }
        public string SymbolCode { get; set; } = string.Empty;
        public decimal Lots { get; set; }
        public decimal ProfitLoss { get; set; }
        public string AccountName { get; set; } = string.Empty;
    }
}
