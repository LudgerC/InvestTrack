using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestTrack.Model.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public int AccountId { get; set; }
        public Account Account { get; set; } = null!;

        public decimal Amount { get; set; }      // Positief = Deposit, Negatief = Withdrawal
        public string Type { get; set; } = string.Empty;  // "Deposit" of "Withdrawal"
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
