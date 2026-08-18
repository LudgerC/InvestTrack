using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using InvestTrack.Mobile.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace InvestTrack.Mobile.ViewModels;

public class FavoritesViewModel : INotifyPropertyChanged
{
    private readonly ApiService _apiService;
    private bool _isBusy;
    private string _statusMessage = string.Empty;

    public ObservableCollection<TradeDisplayItem> Favorites { get; } = new();

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool HasFavorites => Favorites.Count > 0;

    public ICommand RefreshCommand { get; }
    public ICommand RemoveFavoriteCommand { get; }

    public FavoritesViewModel(ApiService apiService)
    {
        _apiService = apiService;
        RefreshCommand = new Command(async () => await LoadAsync());
        RemoveFavoriteCommand = new Command<int>(async (tradeId) => await RemoveFavoriteAsync(tradeId));
    }

    public async Task LoadAsync()
    {
        var userId = Preferences.Get("UserId", string.Empty);
        if (string.IsNullOrEmpty(userId)) return;
        if (IsBusy) return;

        IsBusy = true;
        StatusMessage = string.Empty;

        try
        {
            var items = await _apiService.GetFavoritesAsync(userId);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Favorites.Clear();
                foreach (var t in items)
                {
                    Favorites.Add(new TradeDisplayItem
                    {
                        TradeId = t.TradeId,
                        SymbolCode = t.SymbolCode,
                        SymbolName = t.SymbolName,
                        Lots = t.Lots,
                        ProfitLoss = t.ProfitLoss,
                        AccountName = t.AccountName,
                        IsFavorite = true
                    });
                }
                OnPropertyChanged(nameof(HasFavorites));

                if (Favorites.Count == 0)
                    StatusMessage = "Geen favorieten gevonden. Voeg trades toe via het Trader Dashboard.";
            });
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Fout bij laden: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RemoveFavoriteAsync(int tradeId)
    {
        bool confirm = await Shell.Current.DisplayAlert("Verwijder Favoriet", "Wil je deze trade uit je favorieten verwijderen?", "Ja", "Nee");
        if (!confirm) return;

        bool success = await _apiService.ToggleFavoriteAsync(tradeId);
        if (success)
            await LoadAsync();
        else
            await Shell.Current.DisplayAlert("Fout", "Kon favoriet niet verwijderen. Controleer of je online bent.", "OK");
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
