using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using InvestTrack.Mobile.Services;
using InvestTrack.Model.Data;
using InvestTrack.Model.Identity;
using InvestTrack.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace InvestTrack.Mobile.ViewModels
{
    public class AdminDashboardViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private bool _isBusy;
        private string _statusMessage = string.Empty;
        private int _totalUsersCount;
        private int _totalAccountsCount;
        private int _totalTradesCount;
        private int _totalSymbolsCount;

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

        public int TotalUsersCount
        {
            get => _totalUsersCount;
            set { _totalUsersCount = value; OnPropertyChanged(); }
        }

        public int TotalAccountsCount
        {
            get => _totalAccountsCount;
            set { _totalAccountsCount = value; OnPropertyChanged(); }
        }

        public int TotalTradesCount
        {
            get => _totalTradesCount;
            set { _totalTradesCount = value; OnPropertyChanged(); }
        }

        public int TotalSymbolsCount
        {
            get => _totalSymbolsCount;
            set { _totalSymbolsCount = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ApiService.AdminUserItem> Users { get; } = new();
        public ObservableCollection<ApiService.AdminAccountItem> Accounts { get; } = new();
        public ObservableCollection<ApiService.AdminTradeItem> Trades { get; } = new();
        public ObservableCollection<ApiService.SymbolDto> Symbols { get; } = new();

        public ICommand RefreshCommand { get; }
        public ICommand CreateUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand AddSymbolCommand { get; }
        public ICommand DeleteSymbolCommand { get; }
        public ICommand DeleteAccountCommand { get; }
        public ICommand DeleteTradeCommand { get; }
        public ICommand LogoutCommand { get; }

        public AdminDashboardViewModel(ApiService apiService)
        {
            _apiService = apiService;
            RefreshCommand = new Command(async () => await LoadDataAsync());
            CreateUserCommand = new Command(async () => await CreateUserAsync());
            DeleteUserCommand = new Command<string>(async (id) => await DeleteUserAsync(id));
            AddSymbolCommand = new Command(async () => await AddSymbolAsync());
            DeleteSymbolCommand = new Command<int>(async (id) => await DeleteSymbolAsync(id));
            DeleteAccountCommand = new Command<int>(async (id) => await DeleteAccountAsync(id));
            DeleteTradeCommand = new Command<int>(async (id) => await DeleteTradeAsync(id));
            LogoutCommand = new Command(async () => await ExecuteLogoutAsync());
        }

        public async Task LoadDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusMessage = string.Empty;

            try
            {
                var data = await _apiService.GetAdminDashboardAsync();

                if (data == null)
                {
                    StatusMessage = "Offline modus: Lokale database gegevens worden weergegeven.";
                    // Fallback to local SQLite database
                    try
                    {
                        using var db = DatabaseService.CreateDbContext();
                        var localUsers = db.Users.ToList();
                        var localRoles = db.Roles.ToList();
                        var userRoles = db.UserRoles.ToList();

                        var userList = new List<ApiService.AdminUserItem>();
                        foreach (var u in localUsers)
                        {
                            var ur = userRoles.FirstOrDefault(r => r.UserId == u.Id);
                            var roleName = "Trader";
                            if (ur != null)
                            {
                                var r = localRoles.FirstOrDefault(x => x.Id == ur.RoleId);
                                if (r != null) roleName = r.Name ?? "Trader";
                            }
                            userList.Add(new ApiService.AdminUserItem
                            {
                                Id = u.Id,
                                UserName = u.UserName ?? "",
                                Email = u.Email ?? "",
                                FullName = u.FullName ?? u.UserName ?? "",
                                Role = roleName
                            });
                        }

                        var accounts = db.Accounts.Where(a => !a.IsDeleted).ToList();
                        var trades = db.Trades.Include(t => t.Symbol).Include(t => t.Account).Where(t => !t.IsDeleted).ToList();
                        var symbols = db.Symbols.OrderBy(s => s.Code).ToList();

                        data = new ApiService.AdminDashboardResponse
                        {
                            Users = userList,
                            Accounts = accounts.Select(a => new ApiService.AdminAccountItem
                            {
                                AccountId = a.AccountId,
                                Name = a.Name,
                                Currency = a.Currency,
                                Balance = a.Balance,
                                UserId = a.UserId,
                                UserEmail = "(Lokaal)"
                            }).ToList(),
                            Trades = trades.Select(t => new ApiService.AdminTradeItem
                            {
                                TradeId = t.TradeId,
                                SymbolCode = t.Symbol?.Code ?? "N/B",
                                Lots = t.Lots,
                                ProfitLoss = t.ProfitLoss,
                                AccountName = t.Account?.Name ?? "Onbekend"
                            }).ToList(),
                            Symbols = symbols.Select(s => new ApiService.SymbolDto
                            {
                                SymbolId = s.Id,
                                Code = s.Code,
                                DisplayName = s.DisplayName,
                                Category = s.Category
                            }).ToList()
                        };
                    }
                    catch (Exception ex)
                    {
                        StatusMessage = $"Kon lokale gegevens niet laden: {ex.Message}";
                        return;
                    }
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Users.Clear();
                    foreach (var u in data.Users) Users.Add(u);
                    TotalUsersCount = data.Users.Count;

                    Accounts.Clear();
                    foreach (var a in data.Accounts) Accounts.Add(a);
                    TotalAccountsCount = data.Accounts.Count;

                    Trades.Clear();
                    foreach (var t in data.Trades) Trades.Add(t);
                    TotalTradesCount = data.Trades.Count;

                    Symbols.Clear();
                    foreach (var s in data.Symbols) Symbols.Add(s);
                    TotalSymbolsCount = data.Symbols.Count;
                });
            }
            catch (Exception ex)
            {
                StatusMessage = $"Fout: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CreateUserAsync()
        {
            var email = await Shell.Current.DisplayPromptAsync("Nieuwe Gebruiker", "Voer het e-mailadres in:", "Volgende", "Annuleren", keyboard: Keyboard.Email);
            if (string.IsNullOrWhiteSpace(email)) return;

            var password = await Shell.Current.DisplayPromptAsync("Nieuwe Gebruiker", "Voer een wachtwoord in (bijv. Password123!):", "Volgende", "Annuleren");
            if (string.IsNullOrWhiteSpace(password)) return;

            var role = await Shell.Current.DisplayActionSheet("Selecteer rol", "Annuleren", null, "Trader", "Admin");
            if (string.IsNullOrWhiteSpace(role) || role == "Annuleren") return;

            var fullName = await Shell.Current.DisplayPromptAsync("Nieuwe Gebruiker", "Volledige naam (optioneel):", "Aanmaken", "Overslaan");

            var success = await _apiService.AdminCreateUserAsync(new ApiService.CreateUserRequest
            {
                Email = email.Trim(),
                Password = password.Trim(),
                Role = role,
                FullName = fullName
            });

            if (success)
            {
                await Shell.Current.DisplayAlert("Succes", $"Gebruiker '{email}' succesvol aangemaakt!", "OK");
                await LoadDataAsync();
            }
            else
            {
                await Shell.Current.DisplayAlert("Fout", "Gebruiker aanmaken mislukt. Controleer of je online bent.", "OK");
            }
        }

        private async Task DeleteUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return;

            bool confirm = await Shell.Current.DisplayAlert("Gebruiker Verwijderen", "Weet u zeker dat u deze gebruiker wilt verwijderen?", "Ja, Verwijderen", "Annuleren");
            if (!confirm) return;

            var success = await _apiService.AdminDeleteUserAsync(userId);
            if (success)
            {
                await LoadDataAsync();
            }
            else
            {
                await Shell.Current.DisplayAlert("Fout", "Gebruiker verwijderen mislukt. Controleer of je online bent.", "OK");
            }
        }

        private async Task AddSymbolAsync()
        {
            var code = await Shell.Current.DisplayPromptAsync("Nieuw Symbool", "Code (bijv. BTCUSD, AAPL):", "Volgende", "Annuleren");
            if (string.IsNullOrWhiteSpace(code)) return;

            var name = await Shell.Current.DisplayPromptAsync("Nieuw Symbool", "Weergavenaam (bijv. Bitcoin / US Dollar):", "Volgende", "Annuleren");
            if (string.IsNullOrWhiteSpace(name)) return;

            var category = await Shell.Current.DisplayActionSheet("Selecteer Categorie", "Annuleren", null, "Crypto", "Forex", "Aandelen", "Commodities");
            if (string.IsNullOrWhiteSpace(category) || category == "Annuleren") category = "Algemeen";

            var success = await _apiService.AdminAddSymbolAsync(new ApiService.AddSymbolRequest
            {
                Code = code.Trim().ToUpper(),
                DisplayName = name.Trim(),
                Category = category
            });

            if (success)
            {
                await Shell.Current.DisplayAlert("Succes", $"Symbool '{code}' toegevoegd!", "OK");
                await LoadDataAsync();
            }
            else
            {
                await Shell.Current.DisplayAlert("Fout", "Symbool toevoegen mislukt. Controleer of je online bent.", "OK");
            }
        }

        private async Task DeleteSymbolAsync(int symbolId)
        {
            bool confirm = await Shell.Current.DisplayAlert("Symbool Verwijderen", "Weet u zeker dat u dit symbool wilt verwijderen?", "Ja", "Nee");
            if (!confirm) return;

            var success = await _apiService.AdminDeleteSymbolAsync(symbolId);
            if (success)
                await LoadDataAsync();
            else
                await Shell.Current.DisplayAlert("Fout", "Symbool verwijderen mislukt.", "OK");
        }

        private async Task DeleteAccountAsync(int accountId)
        {
            bool confirm = await Shell.Current.DisplayAlert("Account Verwijderen", "Weet u zeker dat u dit account wilt soft-deleten?", "Ja", "Nee");
            if (!confirm) return;

            var success = await _apiService.AdminDeleteAccountAsync(accountId);
            if (success)
                await LoadDataAsync();
            else
                await Shell.Current.DisplayAlert("Fout", "Account verwijderen mislukt.", "OK");
        }

        private async Task DeleteTradeAsync(int tradeId)
        {
            bool confirm = await Shell.Current.DisplayAlert("Trade Verwijderen", "Weet u zeker dat u deze trade wilt verwijderen?", "Ja", "Nee");
            if (!confirm) return;

            var success = await _apiService.AdminDeleteTradeAsync(tradeId);
            if (success)
                await LoadDataAsync();
            else
                await Shell.Current.DisplayAlert("Fout", "Trade verwijderen mislukt.", "OK");
        }

        private async Task ExecuteLogoutAsync()
        {
            bool confirm = await Shell.Current.DisplayAlert("Uitloggen", "Weet u zeker dat u wilt uitloggen?", "Ja, Uitloggen", "Annuleren");
            if (!confirm) return;

            Preferences.Remove("UserId");
            Preferences.Remove("UserEmail");
            Preferences.Remove("UserRole");

            Users.Clear();
            Accounts.Clear();
            Trades.Clear();
            Symbols.Clear();

            (Shell.Current as AppShell)?.UpdateRoleNavigation();
            await Shell.Current.GoToAsync("//LoginPage");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
