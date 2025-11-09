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
        public DbSet<Symbol> Symbols { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;




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

            builder.Entity<Trade>()
                .HasOne(t => t.Symbol)
                .WithMany()
                .HasForeignKey(t => t.SymbolId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Symbol>()
              .HasIndex(s => s.Code)
              .IsUnique(); // voorkomt dubbele symbolen

            builder.Entity<Symbol>().HasData(
                // Metals
                new Symbol { Id = 1, Code = "XAUUSD", DisplayName = "Goud", Category = "Metals"},
                new Symbol { Id = 2, Code = "XAGUSD", DisplayName = "Zilver", Category = "Metals"},

                // Forex Majors
                new Symbol { Id = 3, Code = "EURUSD", DisplayName = "Euro / Dollar", Category = "Forex"},
                new Symbol { Id = 4, Code = "GBPUSD", DisplayName = "Pond / Dollar", Category = "Forex"},
                new Symbol { Id = 5, Code = "USDJPY", DisplayName = "Dollar / Yen", Category = "Forex"},
                new Symbol { Id = 6, Code = "USDCHF", DisplayName = "Dollar / Zwitserse Frank", Category = "Forex"},
                new Symbol { Id = 7, Code = "AUDUSD", DisplayName = "Aussie / Dollar", Category = "Forex"},
                new Symbol { Id = 8, Code = "USDCAD", DisplayName = "Dollar / Canadese Dollar", Category = "Forex"},
                new Symbol { Id = 9, Code = "NZDUSD", DisplayName = "Kiwi / Dollar", Category = "Forex"},

                // Crypto
                new Symbol { Id = 10, Code = "BTCUSD", DisplayName = "Bitcoin / Dollar", Category = "Crypto"},

                // Indices
                new Symbol { Id = 11, Code = "US30", DisplayName = "Dow Jones 30", Category = "Index"},
                new Symbol { Id = 12, Code = "US100", DisplayName = "NASDAQ 100", Category = "Index"},
                new Symbol { Id = 13, Code = "US500", DisplayName = "S&P 500", Category = "Index"},
                new Symbol { Id = 14, Code = "UK100", DisplayName = "FTSE 100", Category = "Index"},
                new Symbol { Id = 15, Code = "STOXX50", DisplayName = "Euro Stoxx 50", Category = "Index"},
                new Symbol { Id = 16, Code = "JP225", DisplayName = "Nikkei 225", Category = "Index"},
                new Symbol { Id = 17, Code = "HK50", DisplayName = "Hang Seng 50", Category = "Index"}
            );

        }
    }
}
