using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using InvestTrack.Mobile.Services;
using InvestTrack.Model.Data;
using InvestTrack.Model.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestTrack.Mobile.ViewModels
{
    public class TraderDashboardViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private string _userId = string.Empty;
        private decimal _totalBalance;
        private decimal _totalProfitLoss;
        private int _totalTradesCount;
        private bool _isBusy;
        private string _statusMessage = string.Empty;

        public string UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                OnPropertyChanged();
                Task.Run(LoadDataAsync);
            }
        }

        public decimal TotalBalance
        {
            get => _totalBalance;
            set { _totalBalance = value; OnPropertyChanged(); }
        }

        public decimal TotalProfitLoss
        {
            get => _totalProfitLoss;
            set { _totalProfitLoss = value; OnPropertyChanged(); }
        }

        public int TotalTradesCount
        {
            get => _totalTradesCount;
            set { _totalTradesCount = value; OnPropertyChanged(); }
        }

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

        public ObservableCollection<TradeDisplayItem> Trades { get; } = new();
        public ObservableCollection<AccountDisplayItem> Accounts { get; } = new();
        public ObservableCollection<SymbolDisplayItem> Symbols { get; } = new();

        public ICommand RefreshCommand { get; }
        public ICommand AddAccountCommand { get; }
        public ICommand AddTradeCommand { get; }
        public ICommand DeleteTradeCommand { get; }
        public ICommand ToggleFavoriteCommand { get; }
        public ICommand DepositCommand { get; }
        public ICommand WithdrawCommand { get; }
        public ICommand LogoutCommand { get; }

        public TraderDashboardViewModel(ApiService apiService)
        {
            _apiService = apiService;
            RefreshCommand = new Command(async () => await LoadDataAsync());
            AddAccountCommand = new Command(async () => await AddAccountAsync());
            AddTradeCommand = new Command(async () => await AddTradeAsync());
            DeleteTradeCommand = new Command<int>(async (tradeId) => await DeleteTradeAsync(tradeId));
            ToggleFavoriteCommand = new Command<int>(async (tradeId) => await ToggleFavoriteAsync(tradeId));
            DepositCommand = new Command<int>(async (accountId) => await DoDepositAsync(accountId));
            WithdrawCommand = new Command<int>(async (accountId) => await DoWithdrawAsync(accountId));
            LogoutCommand = new Command(async () => await ExecuteLogoutAsync());
        }

        private async Task ExecuteLogoutAsync()
        {
            bool confirm = await Shell.Current.DisplayAlert("Uitloggen", "Weet u zeker dat u wilt uitloggen?", "Ja, Uitloggen", "Annuleren");
            if (!confirm) return;

            Preferences.Remove("UserId");
            Preferences.Remove("UserEmail");
            Preferences.Remove("UserRole");

            _userId = string.Empty;
            Accounts.Clear();
            Trades.Clear();
            Symbols.Clear();
            TotalBalance = 0;
            TotalProfitLoss = 0;

            (Shell.Current as AppShell)?.UpdateRoleNavigation();
            await Shell.Current.GoToAsync("//LoginPage");
        }

        private async Task AddAccountAsync()
        {
            if (string.IsNullOrEmpty(UserId)) return;
            
            var accountName = await Shell.Current.DisplayPromptAsync("Nieuw Account", "Voer een naam in voor het account:", "Toevoegen", "Annuleren");
            
            if (!string.IsNullOrWhiteSpace(accountName))
            {
                var success = await _apiService.AddAccountAsync(new ApiService.CreateAccountRequest 
                { 
                    UserId = UserId, 
                    Name = accountName 
                });
                
                if (success)
                {
                    await LoadDataAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Fout", "Kon account niet toevoegen (controleer of je online bent).", "OK");
                }
            }
        }

        private async Task AddTradeAsync()
        {
            if (string.IsNullOrEmpty(UserId)) return;
            await Shell.Current.GoToAsync($"AddTradePage?UserId={UserId}");
        }

        private async Task DeleteTradeAsync(int tradeId)
        {
            bool confirm = await Shell.Current.DisplayAlert("Verwijderen", "Weet je zeker dat je deze trade wil verwijderen?", "Ja", "Nee");
            if (!confirm) return;

            bool success = await _apiService.DeleteTradeAsync(tradeId);
            if (success)
                await LoadDataAsync();
            else
                await Shell.Current.DisplayAlert("Fout", "Verwijderen mislukt. Controleer of je online bent.", "OK");
        }

        private async Task ToggleFavoriteAsync(int tradeId)
        {
            bool success = await _apiService.ToggleFavoriteAsync(tradeId);
            if (success)
                await LoadDataAsync();
            else
                await Shell.Current.DisplayAlert("Fout", "Favoriet wijzigen mislukt. Controleer of je online bent.", "OK");
        }

        private async Task DoDepositAsync(int accountId)
        {
            var input = await Shell.Current.DisplayPromptAsync("Storting", "Bedrag om te storten (€):", "Storten", "Annuleren", keyboard: Keyboard.Numeric);
            if (string.IsNullOrWhiteSpace(input)) return;
            if (!decimal.TryParse(input.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
            {
                await Shell.Current.DisplayAlert("Fout", "Voer een geldig bedrag in.", "OK");
                return;
            }
            bool success = await _apiService.DepositAsync(accountId, amount);
            if (success)
                await LoadDataAsync();
            else
                await Shell.Current.DisplayAlert("Fout", "Storting mislukt. Controleer of je online bent.", "OK");
        }

        private async Task DoWithdrawAsync(int accountId)
        {
            var input = await Shell.Current.DisplayPromptAsync("Opname", "Bedrag om op te nemen (€):", "Opnemen", "Annuleren", keyboard: Keyboard.Numeric);
            if (string.IsNullOrWhiteSpace(input)) return;
            if (!decimal.TryParse(input.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
            {
                await Shell.Current.DisplayAlert("Fout", "Voer een geldig bedrag in.", "OK");
                return;
            }
            bool success = await _apiService.WithdrawAsync(accountId, amount);
            if (success)
                await LoadDataAsync();
            else
                await Shell.Current.DisplayAlert("Fout", "Opname mislukt. Controleer of je online bent of voldoende saldo hebt.", "OK");
        }

        public async Task LoadDataAsync()
        {
            if (string.IsNullOrEmpty(UserId)) return;
            if (IsBusy) return;

            IsBusy = true;
            StatusMessage = string.Empty;

            try
            {
                var data = await _apiService.GetDashboardAsync(UserId);
                bool isOffline = false;

                if (data == null)
                {
                    isOffline = true;
                    StatusMessage = "Offline modus: Er wordt gebruik gemaakt van lokaal opgeslagen gegevens.";
                    
                    // Fallback to local SQLite database
                    try 
                    {
                        using var db = DatabaseService.CreateDbContext();
                        var accounts = db.Accounts.Where(a => a.UserId == UserId).ToList();
                        var accountIds = accounts.Select(a => a.AccountId).ToList();
                        var trades = db.Trades.Include(t => t.Symbol).Include(t => t.Account)
                                              .Where(t => accountIds.Contains(t.AccountId)).ToList();
                        
                        data = new ApiService.DashboardResponse();
                        data.TotalBalance = accounts.Sum(a => a.Balance);
                        
                        data.Accounts = accounts.Select(a => new ApiService.AccountDto {
                            AccountId = a.AccountId,
                            Name = a.Name,
                            AccountName = a.AccountName,
                            Balance = a.Balance,
                            Currency = a.Currency
                        }).ToList();
                        
                        data.Trades = trades.Select(t => new ApiService.TradeDto {
                            TradeId = t.TradeId,
                            SymbolCode = t.Symbol?.Code ?? "",
                            SymbolName = t.Symbol?.DisplayName ?? "",
                            Lots = t.Lots,
                            ProfitLoss = t.ProfitLoss,
                            AccountName = t.Account?.Name ?? "",
                            AccountId = t.AccountId
                        }).ToList();
                        
                        data.Symbols = db.Symbols.Select(s => new ApiService.SymbolDto {
                            SymbolId = s.Id,
                            Code = s.Code,
                            DisplayName = s.DisplayName
                        }).ToList();
                    } 
                    catch (Exception ex)
                    {
                        StatusMessage = "Kon ook lokale gegevens niet laden.";
                        System.Diagnostics.Debug.WriteLine($"[Dashboard Offline DB] Error: {ex}");
                        return;
                    }
                }
                else
                {
                    // Save to local SQLite database for offline usage
                    try 
                    {
                        using var db = DatabaseService.CreateDbContext();
                        
                        // Save Accounts
                        foreach (var acc in data.Accounts) 
                        {
                            var existing = db.Accounts.Find(acc.AccountId);
                            if (existing == null) 
                            {
                                db.Accounts.Add(new Account {
                                    AccountId = acc.AccountId,
                                    Name = acc.Name,
                                    AccountName = acc.AccountName,
                                    Balance = acc.Balance,
                                    Currency = acc.Currency,
                                    UserId = UserId
                                });
                            } 
                            else 
                            {
                                existing.Name = acc.Name;
                                existing.AccountName = acc.AccountName;
                                existing.Balance = acc.Balance;
                                existing.Currency = acc.Currency;
                            }
                        }
                        
                        // Save Trades
                        var symbols = db.Symbols.ToList();
                        foreach (var t in data.Trades) 
                        {
                            var existing = db.Trades.Find(t.TradeId);
                            var sym = symbols.FirstOrDefault(s => s.Code == t.SymbolCode);
                            if (sym != null) 
                            {
                                if (existing == null) 
                                {
                                    db.Trades.Add(new Trade {
                                        TradeId = t.TradeId,
                                        SymbolId = sym.Id,
                                        Lots = t.Lots,
                                        ProfitLoss = t.ProfitLoss,
                                        AccountId = t.AccountId
                                    });
                                } 
                                else 
                                {
                                    existing.Lots = t.Lots;
                                    existing.ProfitLoss = t.ProfitLoss;
                                    existing.AccountId = t.AccountId;
                                    existing.SymbolId = sym.Id;
                                }
                            }
                        }
                        
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Dashboard Save DB] Error: {ex}");
                    }
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    TotalBalance = data.TotalBalance;

                    Accounts.Clear();
                    foreach (var a in data.Accounts)
                        Accounts.Add(new AccountDisplayItem
                        {
                            AccountId = a.AccountId,
                            Name = a.Name,
                            Balance = a.Balance,
                            Currency = a.Currency
                        });

                    Trades.Clear();
                    decimal pl = 0;
                    foreach (var t in data.Trades)
                    {
                        pl += t.ProfitLoss;
                        Trades.Add(new TradeDisplayItem
                        {
                            TradeId = t.TradeId,
                            SymbolCode = t.SymbolCode,
                            SymbolName = t.SymbolName,
                            Lots = t.Lots,
                            ProfitLoss = t.ProfitLoss,
                            AccountName = t.AccountName,
                            IsFavorite = t.IsFavorite
                        });
                    }
                    TotalProfitLoss = pl;
                    TotalTradesCount = data.Trades.Count;

                    Symbols.Clear();
                    foreach (var s in data.Symbols)
                        Symbols.Add(new SymbolDisplayItem
                        {
                            SymbolId = s.SymbolId,
                            Code = s.Code,
                            DisplayName = s.DisplayName
                        });
                });
            }
            catch (Exception ex)
            {
                StatusMessage = $"Fout: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[Dashboard] Error: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TradeDisplayItem
    {
        public int TradeId { get; set; }
        public string SymbolCode { get; set; } = string.Empty;
        public string SymbolName { get; set; } = string.Empty;
        public decimal Lots { get; set; }
        public decimal ProfitLoss { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
    }

    public class AccountDisplayItem
    {
        public int AccountId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = "EUR";
    }

    public class SymbolDisplayItem
    {
        public int SymbolId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }
}
