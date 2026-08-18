using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace InvestTrack.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        UpdateRoleNavigation();
    }

    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);
        UpdateRoleNavigation();
    }

    public void UpdateRoleNavigation()
    {
        try
        {
            var role = Preferences.Get("UserRole", string.Empty);
            var userId = Preferences.Get("UserId", string.Empty);

            if (string.IsNullOrEmpty(userId))
            {
                // Niet ingelogd: enkel Account tabblad
                if (TraderTab != null) TraderTab.IsVisible = false;
                if (FavoritesTab != null) FavoritesTab.IsVisible = false;
                if (SymbolsTab != null) SymbolsTab.IsVisible = false;
                if (AdminTab != null) AdminTab.IsVisible = false;
            }
            else if (role == "Admin")
            {
                // Admin ingelogd: Admin + Symbolen
                if (TraderTab != null) TraderTab.IsVisible = false;
                if (FavoritesTab != null) FavoritesTab.IsVisible = false;
                if (SymbolsTab != null) SymbolsTab.IsVisible = true;
                if (AdminTab != null) AdminTab.IsVisible = true;
            }
            else
            {
                // Gewone Trader ingelogd: Trader + Favorieten + Symbolen
                if (TraderTab != null) TraderTab.IsVisible = true;
                if (FavoritesTab != null) FavoritesTab.IsVisible = true;
                if (SymbolsTab != null) SymbolsTab.IsVisible = true;
                if (AdminTab != null) AdminTab.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UpdateRoleNavigation] Error: {ex}");
        }
    }
}
