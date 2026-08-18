using InvestTrack.Mobile.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace InvestTrack.Mobile.Views;

public partial class AdminDashboardPage : ContentPage
{
    private readonly AdminDashboardViewModel _viewModel;

    public AdminDashboardPage(AdminDashboardViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var role = Preferences.Get("UserRole", string.Empty);
        if (role != "Admin")
        {
            await DisplayAlert("Geen toegang", "Alleen beheerders (Admins) hebben toegang tot deze pagina.", "OK");
            await Shell.Current.GoToAsync("//LoginPage");
            return;
        }

        await _viewModel.LoadDataAsync();
    }
}
