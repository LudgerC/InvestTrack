using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestTrack.Model.Models
{
    public class FavoriteTrade
    {
        public int FavoriteTradeId { get; set; }

        public string Note { get; set; } = string.Empty;

        // Soft Delete
        public bool IsDeleted { get; set; } = false;

        // Foreign Key to Trade
        public int TradeId { get; set; }
        public Trade? Trade { get; set; }
    }
}
