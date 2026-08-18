using System.Collections.Generic;
using InvestTrack.Model.Models;

namespace InvestTrack.Web.Models
{
    public class TraderDashboardViewModel
    {
        public List<Account> Accounts { get; set; } = new();
        public List<TradeItemViewModel> Trades { get; set; } = new();
        public List<TradeItemViewModel> Favorites { get; set; } = new();
        public List<Symbol> Symbols { get; set; } = new();

        public int SelectedAccountId { get; set; }
        public int SelectedFavoritesAccountId { get; set; }
        public string SelectedCategory { get; set; } = "Alle";
        public List<string> Categories { get; set; } = new();

        // Stat Overview
        public decimal TotalBalance { get; set; }
        public decimal TotalProfitLoss { get; set; }
        public int TotalTradesCount { get; set; }
    }

    public class TradeItemViewModel
    {
        public int TradeId { get; set; }
        public string SymbolCode { get; set; } = string.Empty;
        public string SymbolName { get; set; } = string.Empty;
        public decimal Lots { get; set; }
        public decimal ProfitLoss { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public int AccountId { get; set; }
        public bool IsFavorite { get; set; }
    }
}
