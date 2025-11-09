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

        // Naam van de account (bijv. "Main Trading Account")
        public string Name { get; set; } = string.Empty;

        public string AccountName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = "USD";

        // Soft Delete
        public bool IsDeleted { get; set; } = false;

        // Relationships
        public string UserId { get; set; } = string.Empty;             // Foreign key naar ApplicationUser
        public ApplicationUser? User { get; set; }      // Navigatie naar de gebruiker

        public ICollection<Trade> Trades { get; set; } = new List<Trade>();
    }
}
