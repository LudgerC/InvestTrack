using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvestTrack.Model.Models;
using Microsoft.AspNetCore.Identity;


namespace InvestTrack.Model.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        // Soft Delete (kan gebruiker blokkeren / hide)
        public bool IsDeleted { get; set; } = false;
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
    }
}
