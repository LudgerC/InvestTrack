using System;
using InvestTrack.Mobile.Services;
using InvestTrack.Mobile.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace InvestTrack.Mobile.Views;

public partial class FavoritesPage : ContentPage
{
    private readonly FavoritesViewModel _viewModel;

    public FavoritesPage() : this(CreateDefaultViewModel())
    {
    }

    public FavoritesPage(FavoritesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? CreateDefaultViewModel();
        BindingContext = _viewModel;
    }

    private static FavoritesViewModel CreateDefaultViewModel()
    {
        return IPlatformApplication.Current?.Services?.GetService<FavoritesViewModel>()
            ?? new FavoritesViewModel(new ApiService());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var role = Preferences.Get("UserRole", string.Empty);
            var userId = Preferences.Get("UserId", string.Empty);

            if (string.IsNullOrEmpty(userId))
            {
                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            if (role == "Admin")
            {
                // Admins hebben geen favorieten pagina
                await Shell.Current.GoToAsync("//AdminDashboardPage");
                return;
            }

            if (_viewModel != null)
            {
                await _viewModel.LoadAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FavoritesPage OnAppearing] Error: {ex}");
        }
    }
}
