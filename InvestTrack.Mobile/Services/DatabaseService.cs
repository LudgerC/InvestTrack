using System;
using System.IO;
using System.Linq;
using InvestTrack.Model.Data;
using InvestTrack.Model.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

namespace InvestTrack.Mobile.Services
{
    public class DatabaseService
    {
        private static bool _isMigrated = false;
        private static readonly object _lock = new object();

        public static string GetDatabasePath()
        {
            if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
            {
                return Path.Combine(FileSystem.AppDataDirectory, "investtrack.db");
            }

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string dbPath = Path.Combine(baseDir, "..", "..", "..", "..", "..", "Data", "investtrack.db");
                string fullPath = Path.GetFullPath(dbPath);

                var dir = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                return fullPath;
            }
            catch
            {
                return Path.Combine(FileSystem.AppDataDirectory, "investtrack.db");
            }
        }

        public static InvestTrackDbContext CreateDbContext()
        {
            string dbPath = GetDatabasePath();
            var options = new DbContextOptionsBuilder<InvestTrackDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            var dbContext = new InvestTrackDbContext(options);

            if (!_isMigrated)
            {
                lock (_lock)
                {
                    if (!_isMigrated)
                    {
                        try
                        {
                            dbContext.Database.Migrate();
                            SeedDefaultUsers(dbContext);
                        }
                        catch { }
                        _isMigrated = true;
                    }
                }
            }

            return dbContext;
        }

        private static void SeedDefaultUsers(InvestTrackDbContext dbContext)
        {
            try
            {
                if (!dbContext.Roles.Any(r => r.Name == "Trader"))
                {
                    dbContext.Roles.Add(new IdentityRole { Id = "role-trader-id", Name = "Trader", NormalizedName = "TRADER" });
                }
                if (!dbContext.Roles.Any(r => r.Name == "Admin"))
                {
                    dbContext.Roles.Add(new IdentityRole { Id = "role-admin-id", Name = "Admin", NormalizedName = "ADMIN" });
                }

                var passwordHasher = new PasswordHasher<ApplicationUser>();

                if (!dbContext.Users.Any(u => u.Email == "trader@investtrack.local"))
                {
                    var trader = new ApplicationUser
                    {
                        Id = "trader-user-id",
                        UserName = "trader@investtrack.local",
                        NormalizedUserName = "TRADER@INVESTTRACK.LOCAL",
                        Email = "trader@investtrack.local",
                        NormalizedEmail = "TRADER@INVESTTRACK.LOCAL",
                        FullName = "Test Trader",
                        EmailConfirmed = true
                    };
                    trader.PasswordHash = passwordHasher.HashPassword(trader, "Trader#12345");
                    dbContext.Users.Add(trader);
                    dbContext.UserRoles.Add(new IdentityUserRole<string> { UserId = trader.Id, RoleId = "role-trader-id" });
                }

                if (!dbContext.Users.Any(u => u.Email == "admin@investtrack.local"))
                {
                    var admin = new ApplicationUser
                    {
                        Id = "admin-user-id",
                        UserName = "admin@investtrack.local",
                        NormalizedUserName = "ADMIN@INVESTTRACK.LOCAL",
                        Email = "admin@investtrack.local",
                        NormalizedEmail = "ADMIN@INVESTTRACK.LOCAL",
                        FullName = "System Admin",
                        EmailConfirmed = true
                    };
                    admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin#12345");
                    dbContext.Users.Add(admin);
                    dbContext.UserRoles.Add(new IdentityUserRole<string> { UserId = admin.Id, RoleId = "role-admin-id" });
                }

                dbContext.SaveChanges();
            }
            catch { }
        }
    }
}
