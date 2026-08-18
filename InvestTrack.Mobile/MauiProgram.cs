using InvestTrack.Mobile.Services;
using InvestTrack.Mobile.ViewModels;
using InvestTrack.Mobile.Views;

namespace InvestTrack.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Services
        builder.Services.AddSingleton<ApiService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<TraderDashboardViewModel>();
        builder.Services.AddTransient<AdminDashboardViewModel>();
        builder.Services.AddTransient<SymbolsViewModel>();
        builder.Services.AddTransient<FavoritesViewModel>();

        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<TraderDashboardPage>();
        builder.Services.AddTransient<AdminDashboardPage>();
        builder.Services.AddTransient<SymbolsPage>();
        builder.Services.AddTransient<FavoritesPage>();
        builder.Services.AddTransient<AddTradePage>();

        // Routes
        Routing.RegisterRoute("AddTradePage", typeof(AddTradePage));
        Routing.RegisterRoute("TraderDashboardPage", typeof(TraderDashboardPage));
        Routing.RegisterRoute("AdminDashboardPage", typeof(AdminDashboardPage));
        Routing.RegisterRoute("FavoritesPage", typeof(FavoritesPage));
        Routing.RegisterRoute("SymbolsPage", typeof(SymbolsPage));

        return builder.Build();
    }
}
