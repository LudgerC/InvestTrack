using InvestTrack.Mobile.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace InvestTrack.Mobile.Views;

[QueryProperty(nameof(UserId), "UserId")]
public partial class TraderDashboardPage : ContentPage
{
    private readonly TraderDashboardViewModel _viewModel;

    public string UserId
    {
        set
        {
            if (_viewModel != null)
            {
                _viewModel.UserId = value;
            }
        }
    }

    public TraderDashboardPage(TraderDashboardViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var userId = Preferences.Get("UserId", string.Empty);
        var role = Preferences.Get("UserRole", string.Empty);

        if (string.IsNullOrEmpty(userId))
        {
            await Shell.Current.GoToAsync("//LoginPage");
            return;
        }

        if (role == "Admin")
        {
            // Admins horen in het Admin panel
            await Shell.Current.GoToAsync("//AdminDashboardPage");
            return;
        }

        if (string.IsNullOrEmpty(_viewModel.UserId))
        {
            _viewModel.UserId = userId;
        }
        else
        {
            await _viewModel.LoadDataAsync();
        }
    }
}
