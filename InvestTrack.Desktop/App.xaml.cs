using System.Configuration;
using System.Data;
using System.Windows;
using InvestTrack.Model.Data;
using InvestTrack.Model.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvestTrack.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            // laad User Secrets
            var config = new ConfigurationBuilder()
                .AddUserSecrets<App>()
                .Build();

            string connectionString = config.GetConnectionString("DefaultConnection");

            services.AddDbContext<InvestTrackDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddIdentity<ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole>()
                .AddEntityFrameworkStores<InvestTrackDbContext>();

            ServiceProvider = services.BuildServiceProvider();

            // === Seed Roles + Admin user ===
            using (var scope = ServiceProvider.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                string[] roles = new[] { "Admin", "Trader" };
                foreach (var role in roles)
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));

                // Admin user
                var adminEmail = "admin@investtrack.local";
                var admin = await userManager.FindByEmailAsync(adminEmail);
                if (admin == null)
                {
                    admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = "System Admin" };
                    await userManager.CreateAsync(admin, "Admin#12345"); // tijdelijk wachtwoord
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            base.OnStartup(e);
        }


    }

}


