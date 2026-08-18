using System.IO;
using InvestTrack.Model.Data;
using InvestTrack.Model.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// === Config & Database Connection ===
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                          ?? "Data Source=../Data/investtrack.db";

// Ensure database directory exists
try
{
    var sqliteBuilder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString);
    if (!string.IsNullOrEmpty(sqliteBuilder.DataSource))
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(sqliteBuilder.DataSource));
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
}
catch { }

builder.Services.AddDbContext<InvestTrackDbContext>(options =>
    options.UseSqlite(connectionString));

// === Identity Setup ===
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<InvestTrackDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// === MVC & API Controllers ===
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

var app = builder.Build();

// === Database Migration & Seeding ===
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<InvestTrackDbContext>();
        await dbContext.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

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
            admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = "System Admin" };
            await userManager.CreateAsync(admin, "Admin#12345");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        var traderEmail = "trader@investtrack.local";
        var trader = await userManager.FindByEmailAsync(traderEmail);
        if (trader == null)
        {
            trader = new ApplicationUser { UserName = traderEmail, Email = traderEmail, FullName = "Test Trader" };
            await userManager.CreateAsync(trader, "Trader#12345");
            await userManager.AddToRoleAsync(trader, "Trader");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error seeding database.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
