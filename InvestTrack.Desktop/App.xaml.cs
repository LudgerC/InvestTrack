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

            // === Database ===
            services.AddDbContext<InvestTrackDbContext>(options =>
                options.UseSqlite(connectionString));

            // === Logging (voor RoleManager) ===
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

            // === Roles + Admin seeden ===
            using (var scope = ServiceProvider.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                string[] roles = { "Admin", "Trader" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }

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
            }

            // === Start Login venster ===
            using (var scope = ServiceProvider.CreateScope())
            {
                var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                var login = new Views.Auth.LoginWindow(signInManager, userManager);
                login.Show();
            }

            base.OnStartup(e);
        }
    }
}
