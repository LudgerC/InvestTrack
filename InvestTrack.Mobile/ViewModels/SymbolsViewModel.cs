using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using InvestTrack.Mobile.Services;
using InvestTrack.Model.Contracts;
using InvestTrack.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace InvestTrack.Mobile.ViewModels;

public class SymbolsViewModel : INotifyPropertyChanged
{
    private readonly ApiService _api;
    private bool _isBusy;
    private string _statusMessage = string.Empty;
    private bool _isAdmin;

    public ObservableCollection<SymbolDto> Symbols { get; } = new();

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

    public bool IsAdmin
    {
        get => _isAdmin;
        set { _isAdmin = value; OnPropertyChanged(); }
    }

    public ICommand RefreshCommand { get; }
    public ICommand AddSymbolCommand { get; }
    public ICommand DeleteSymbolCommand { get; }

    public SymbolsViewModel(ApiService api)
    {
        _api = api;
        RefreshRole();
        RefreshCommand = new Command(async () => await LoadAsync());
        AddSymbolCommand = new Command(async () => await AddSymbolAsync());
        DeleteSymbolCommand = new Command<int>(async (symbolId) => await DeleteSymbolAsync(symbolId));
    }

    public void RefreshRole()
    {
        var role = Preferences.Get("UserRole", string.Empty);
        IsAdmin = (role == "Admin");
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        StatusMessage = string.Empty;
        RefreshRole();

        try
        {
            List<SymbolDto> items = await _api.GetSymbolsListAsync();

            if (items == null || items.Count == 0)
            {
                StatusMessage = "Offline modus: Lokale symbolen geladen.";
                // Fallback to local SQLite database
                try
                {
                    using var db = DatabaseService.CreateDbContext();
                    items = db.Symbols.OrderBy(s => s.Code).Select(s => new SymbolDto
                    {
                        Id = s.Id,
                        Code = s.Code,
                        DisplayName = s.DisplayName ?? s.Code,
                        Category = s.Category ?? "Algemeen"
                    }).ToList();
                }
                catch (Exception dbEx)
                {
                    StatusMessage = $"Kon ook lokale symbolen niet laden: {dbEx.Message}";
                    System.Diagnostics.Debug.WriteLine($"[Symbols Offline DB] Error: {dbEx}");
                }
            }
            else
            {
                // Save to local SQLite database for offline usage
                try
                {
                    using var db = DatabaseService.CreateDbContext();
                    foreach (var item in items)
                    {
                        var existing = db.Symbols.Find(item.Id);
                        if (existing == null)
                        {
                            db.Symbols.Add(new Symbol
                            {
                                Id = item.Id,
                                Code = item.Code,
                                DisplayName = item.DisplayName,
                                Category = item.Category
                            });
                        }
                        else
                        {
                            existing.Code = item.Code;
                            existing.DisplayName = item.DisplayName;
                            existing.Category = item.Category;
                        }
                    }
                    db.SaveChanges();
                }
                catch (Exception dbEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[Symbols Save DB] Error: {dbEx}");
                }
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Symbols.Clear();
                if (items != null)
                {
                    foreach (var s in items)
                    {
                        Symbols.Add(s);
                    }
                }
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Fout bij laden: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddSymbolAsync()
    {
        if (!IsAdmin) return;

        var code = await Shell.Current.DisplayPromptAsync("Nieuw Symbool", "Code (bijv. BTCUSD, AAPL, NVDA):", "Volgende", "Annuleren");
        if (string.IsNullOrWhiteSpace(code)) return;

        var name = await Shell.Current.DisplayPromptAsync("Nieuw Symbool", "Weergavenaam (bijv. Bitcoin, Apple, Nvidia):", "Volgende", "Annuleren");
        if (string.IsNullOrWhiteSpace(name)) return;

        var category = await Shell.Current.DisplayActionSheet("Selecteer Categorie", "Annuleren", null, "Crypto", "Forex", "Aandelen", "Commodities", "Index");
        if (string.IsNullOrWhiteSpace(category) || category == "Annuleren") category = "Algemeen";

        var success = await _api.AdminAddSymbolAsync(new ApiService.AddSymbolRequest
        {
            Code = code.Trim().ToUpper(),
            DisplayName = name.Trim(),
            Category = category
        });

        if (success)
        {
            await Shell.Current.DisplayAlert("Succes", $"Symbool '{code}' is succesvol toegevoegd!", "OK");
            await LoadAsync();
        }
        else
        {
            await Shell.Current.DisplayAlert("Fout", "Symbool toevoegen mislukt. Controleer of je online bent.", "OK");
        }
    }

    private async Task DeleteSymbolAsync(int symbolId)
    {
        if (!IsAdmin) return;

        bool confirm = await Shell.Current.DisplayAlert("Symbool Verwijderen", "Weet u zeker dat u dit symbool wilt verwijderen?", "Ja, Verwijder", "Nee");
        if (!confirm) return;

        var success = await _api.AdminDeleteSymbolAsync(symbolId);
        if (success)
        {
            await Shell.Current.DisplayAlert("Succes", "Symbool verwijderd.", "OK");
            await LoadAsync();
        }
        else
        {
            await Shell.Current.DisplayAlert("Fout", "Symbool verwijderen mislukt. Controleer of je online bent.", "OK");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
