using System;
using System.Globalization;
using InvestTrack.Mobile.Services;
using InvestTrack.Mobile.ViewModels;
using Microsoft.Maui.Controls;

namespace InvestTrack.Mobile.Views;

[QueryProperty(nameof(UserId), "UserId")]
public partial class AddTradePage : ContentPage
{
    private readonly ApiService _apiService;
    private List<AccountDisplayItem> _accounts = new();
    private List<SymbolDisplayItem> _symbols = new();

    public string UserId { get; set; } = string.Empty;

    public AddTradePage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await LoadPickerDataAsync();
    }

    private async Task LoadPickerDataAsync()
    {
        try
        {
            var data = await _apiService.GetDashboardAsync(UserId);
            if (data != null)
            {
                _accounts = data.Accounts.Select(a => new AccountDisplayItem
                {
                    AccountId = a.AccountId,
                    Name = a.Name,
                    Balance = a.Balance,
                    Currency = a.Currency
                }).ToList();

                _symbols = data.Symbols.Select(s => new SymbolDisplayItem
                {
                    SymbolId = s.SymbolId,
                    Code = s.Code,
                    DisplayName = s.DisplayName
                }).ToList();

                AccountPicker.ItemsSource = _accounts;
                SymbolPicker.ItemsSource = _symbols;
            }
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Fout bij laden: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        var selectedAccount = AccountPicker.SelectedItem as AccountDisplayItem;
        var selectedSymbol = SymbolPicker.SelectedItem as SymbolDisplayItem;

        if (selectedAccount == null)
        {
            ErrorLabel.Text = "Selecteer een account.";
            ErrorLabel.IsVisible = true;
            return;
        }
        if (selectedSymbol == null)
        {
            ErrorLabel.Text = "Selecteer een symbool.";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (!decimal.TryParse(LotsEntry.Text?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal lots) || lots <= 0)
        {
            ErrorLabel.Text = "Voer een geldig aantal lots in (groter dan 0).";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (!decimal.TryParse(ProfitLossEntry.Text?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal profitLoss))
        {
            ErrorLabel.Text = "Voer een geldig winst/verlies bedrag in.";
            ErrorLabel.IsVisible = true;
            return;
        }

        SaveButton.IsEnabled = false;
        SaveButton.Text = "Opslaan...";

        var result = await _apiService.AddTradeAsync(new ApiService.CreateTradeRequest
        {
            AccountId = selectedAccount.AccountId,
            SymbolId = selectedSymbol.SymbolId,
            SymbolCode = selectedSymbol.Code,
            Lots = lots,
            ProfitLoss = profitLoss
        });

        if (result.Success)
        {
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            ErrorLabel.Text = result.Error.Length > 0
                ? result.Error
                : "Trade opslaan mislukt. Controleer of je online bent.";
            ErrorLabel.IsVisible = true;
            SaveButton.IsEnabled = true;
            SaveButton.Text = "Trade Opslaan";
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
