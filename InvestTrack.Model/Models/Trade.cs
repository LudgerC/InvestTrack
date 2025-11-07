using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestTrack.Model.Models
{
    public class Trade
    {
        public int TradeId { get; set; }

        public string Symbol { get; set; } = string.Empty;
        public decimal EntryPrice { get; set; }
        public decimal ExitPrice { get; set; }
        public decimal Lots { get; set; }
        public decimal ProfitLoss { get; set; }

        // Soft Delete
        public bool IsDeleted { get; set; } = false;

        // Foreign Key to Account
        public int AccountId { get; set; }
        public Account? Account { get; set; }
    }
}

