using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvestTrack.Model.Identity;
using InvestTrack.Model.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InvestTrack.Model.Data
{
    public class InvestTrackDbContext : IdentityDbContext<ApplicationUser>
    {
        public InvestTrackDbContext(DbContextOptions<InvestTrackDbContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Trade> Trades { get; set; }
        public DbSet<FavoriteTrade> FavoriteTrades { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Soft Delete filter
            builder.Entity<Account>().HasQueryFilter(a => !a.IsDeleted);
            builder.Entity<Trade>().HasQueryFilter(t => !t.IsDeleted);
            builder.Entity<FavoriteTrade>().HasQueryFilter(f => !f.IsDeleted);

            // Relationships 

            builder.Entity<Account>()
                .HasMany(a => a.Trades)
                .WithOne(t => t.Account)
                .HasForeignKey(t => t.AccountId);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.Accounts)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId);
        }
    }
}
