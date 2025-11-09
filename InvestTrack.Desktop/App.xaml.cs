using System;
using System.Windows;
using InvestTrack.Model.Data;
using InvestTrack.Model.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvestTrack.Desktop
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        protected override async void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            // === Config laden ===
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<App>()
                .Build();

            string connectionString = config["ConnectionStrings:DefaultConnection"] ?? "Data Source=investtrack.db";

            // === Database setup ===
            services.AddDbContext<InvestTrackDbContext>(options =>
                options.UseSqlite(connectionString));

            // === Logging (nodig voor Identity) ===
            services.AddLogging();

            // === Identity setup ===
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<InvestTrackDbContext>()
            .AddDefaultTokenProviders();

            ServiceProvider = services.BuildServiceProvider();

            // === Database migreren vóór seeding ===
            using (var migrateScope = ServiceProvider.CreateScope())
            {
                var dbContext = migrateScope.ServiceProvider.GetRequiredService<InvestTrackDbContext>();
                await dbContext.Database.MigrateAsync(); // ✅ zorgt dat tabellen bestaan
            }

            // === Roles + Default Users seeden ===
            using (var seedScope = ServiceProvider.CreateScope())
            {
                var roleManager = seedScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = seedScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                string[] roles = { "Admin", "Trader" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }

                // --- Admin user ---
                var adminEmail = "admin@investtrack.local";
                var admin = await userManager.FindByEmailAsync(adminEmail);
                if (admin == null)
                {
                    admin = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = "System Admin"
                    };

                    await userManager.CreateAsync(admin, "Admin#12345");
                    await userManager.AddToRoleAsync(admin, "Admin");
                }

                // --- Trader user ---
                var traderEmail = "trader@investtrack.local";
                var trader = await userManager.FindByEmailAsync(traderEmail);
                if (trader == null)
                {
                    trader = new ApplicationUser
                    {
                        UserName = traderEmail,
                        Email = traderEmail,
                        FullName = "Test Trader"
                    };

                    await userManager.CreateAsync(trader, "Trader#12345");
                    await userManager.AddToRoleAsync(trader, "Trader");
                }
            }

            // === Start Login venster (NIET in using!) ===
            var rootScope = ServiceProvider.CreateScope();
            var signInManager = rootScope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
            var globalUserManager = rootScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var login = new Views.Auth.LoginWindow(signInManager, globalUserManager);
            login.Show();

            base.OnStartup(e);
        }
    }
}
