using InvestTrack.Model.Identity;
using InvestTrack.Model.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InvestTrack.Model.Data
{
    public class InvestTrackDbContext
        : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public InvestTrackDbContext(DbContextOptions<InvestTrackDbContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<Trade> Trades { get; set; } = null!;
        public DbSet<FavoriteTrade> FavoriteTrades { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // === Soft delete filters ===
            builder.Entity<Account>().HasQueryFilter(a => !a.IsDeleted);
            builder.Entity<Trade>().HasQueryFilter(t => !t.IsDeleted);
            builder.Entity<FavoriteTrade>().HasQueryFilter(f => !f.IsDeleted);

            // === Relations ===
            builder.Entity<Account>()
                .HasMany(a => a.Trades)
                .WithOne(t => t.Account)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.Accounts)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
