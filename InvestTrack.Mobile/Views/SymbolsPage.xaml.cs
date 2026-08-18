using System;
using InvestTrack.Mobile.Services;
using InvestTrack.Mobile.ViewModels;
using Microsoft.Maui.Controls;

namespace InvestTrack.Mobile.Views;

public partial class SymbolsPage : ContentPage
{
    private readonly SymbolsViewModel _viewModel;

    public SymbolsPage() : this(CreateDefaultViewModel())
    {
    }

    public SymbolsPage(SymbolsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? CreateDefaultViewModel();
        BindingContext = _viewModel;
    }

    private static SymbolsViewModel CreateDefaultViewModel()
    {
        return IPlatformApplication.Current?.Services?.GetService<SymbolsViewModel>()
            ?? new SymbolsViewModel(new ApiService());
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            _viewModel?.RefreshRole();
            if (_viewModel != null)
            {
                await _viewModel.LoadAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SymbolsPage OnAppearing] Error: {ex}");
        }
    }
}
