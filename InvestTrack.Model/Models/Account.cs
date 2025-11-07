using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvestTrack.Model.Identity;

namespace InvestTrack.Model.Models
{
    public class Account
    {
        public int AccountId { get; set; }

        public string AccountName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = "USD";

        // Soft Delete
        public bool IsDeleted { get; set; } = false;

        // Relationships
        public string UserId { get; set; }              // Foreign key naar ApplicationUser
        public ApplicationUser? User { get; set; }      // Navigatie naar de gebruiker

        public ICollection<Trade> Trades { get; set; } = new List<Trade>();
    }
}
