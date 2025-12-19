using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using InvestTrack.Model.Contracts;

namespace InvestTrack.Mobile.Views;

public partial class SymbolsPage : ContentPage
{
    private readonly HttpClient _http;

    public SymbolsPage()
    {
        InitializeComponent();

        
        _http = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7027/") 
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            ErrorLabel.IsVisible = false;
            Loading.IsVisible = true;
            Loading.IsRunning = true;

            var items = await _http.GetFromJsonAsync<List<SymbolDto>>("api/symbols")
            ?? new List<SymbolDto>();

            SymbolsList.ItemsSource = items;

        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.Message ?? "Onbekende fout.";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            Loading.IsRunning = false;
            Loading.IsVisible = false;
        }
    }
}
