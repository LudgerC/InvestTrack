using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using InvestTrack.Desktop.Views.Admin;
using InvestTrack.Model.Data;
using InvestTrack.Model.Identity;
using InvestTrack.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;



namespace InvestTrack.Desktop.Views.Admin
{
    public partial class AdminDashboard : Window
    {
        private readonly InvestTrackDbContext _db;

        public AdminDashboard()
        {
            InitializeComponent();
            _db = App.ServiceProvider.GetRequiredService<InvestTrackDbContext>();

            _userManager = App.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            _roleManager = App.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            LoadAllData();
        }

        private void LoadAllData()
        {
            LoadUsers();
            LoadAccounts();
            LoadTrades();
            LoadSymbols();
        }

        private void LoadUsers()
        {
            UsersGrid.ItemsSource = _db.Users
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email
                })
                .ToList();
        }

        private void LoadAccounts()
        {
            AccountsGrid.ItemsSource = _db.Accounts
                .Include(a => a.User)
                .Where(a => !a.IsDeleted)
                .Select(a => new
                {
                    a.AccountId,
                    a.Name,
                    a.Currency,
                    a.Balance,
                    a.UserId,
                    Email = a.User != null ? a.User.Email : "(geen e-mail)"
                })
                .ToList();
        }

        private void LoadTrades()
        {
            TradesGrid.ItemsSource = _db.Trades
                .Include(t => t.Symbol)
                .Include(t => t.Account)
                .Where(t => !t.IsDeleted)
                .Select(t => new
                {
                    t.TradeId,
                    Symbol = t.Symbol,
                    t.Lots,
                    t.ProfitLoss,
                    Account = t.Account
                })
                .ToList();
        }

        private void LoadSymbols()
        {
            SymbolsGrid.ItemsSource = _db.Symbols
                .OrderBy(s => s.Code)
                .ToList();
        }

        // === EVENT HANDLERS ===

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var login = new Views.Auth.LoginWindow(
                App.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.SignInManager<InvestTrack.Model.Identity.ApplicationUser>>(),
                App.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<InvestTrack.Model.Identity.ApplicationUser>>()
            );

            login.Show();
            this.Close();
        }

        // GEBRUIKERS
        private void ViewAccounts_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem == null)
            {
                MessageBox.Show("Selecteer een gebruiker om zijn accounts te bekijken.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            dynamic selectedUser = UsersGrid.SelectedItem;
            string userId = selectedUser.Id;

            var accounts = _db.Accounts
                .Where(a => a.UserId == userId && !a.IsDeleted)
                .Select(a => new { a.Name, a.Currency, a.Balance })
                .ToList();

            string message = accounts.Count == 0
                ? "Deze gebruiker heeft geen accounts."
                : string.Join("\n", accounts.Select(a => $"{a.Name} - {a.Currency} - {a.Balance:N2}"));

            MessageBox.Show(message, $"Accounts van gebruiker {selectedUser.UserName}");
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem == null)
            {
                MessageBox.Show("Selecteer een gebruiker om te verwijderen.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic selectedUser = UsersGrid.SelectedItem;
            string userId = selectedUser.Id;

            var user = _db.Users.Find(userId);
            if (user == null)
            {
                MessageBox.Show("Gebruiker niet gevonden.");
                return;
            }

            if (MessageBox.Show($"Weet je zeker dat je '{user.UserName}' wilt verwijderen?",
                "Bevestiging", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _db.Users.Remove(user);
                _db.SaveChanges();
                LoadUsers();
            }
        }

        // ACCOUNTS
        private void ViewTrades_Click(object sender, RoutedEventArgs e)
        {
            if (AccountsGrid.SelectedItem == null)
            {
                MessageBox.Show("Selecteer een account om trades te bekijken.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            dynamic selectedAccount = AccountsGrid.SelectedItem;
            int accountId = selectedAccount.AccountId;

            var trades = _db.Trades
                .Include(t => t.Symbol)
                .Where(t => t.AccountId == accountId && !t.IsDeleted)
                .Select(t => new { t.Symbol.Code, t.Lots, t.ProfitLoss })
                .ToList();

            string message = trades.Count == 0
                ? "Geen trades voor deze account."
                : string.Join("\n", trades.Select(t => $"{t.Code} - Lots: {t.Lots} - P/L: {t.ProfitLoss:N2}"));

            MessageBox.Show(message, $"Trades van account {selectedAccount.Name}");
        }

        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            if (AccountsGrid.SelectedItem == null)
            {
                MessageBox.Show("Selecteer een account om te verwijderen.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic selectedAccount = AccountsGrid.SelectedItem;
            int accountId = selectedAccount.AccountId;

            var account = _db.Accounts.Include(a => a.Trades).FirstOrDefault(a => a.AccountId == accountId);
            if (account == null)
            {
                MessageBox.Show("Account niet gevonden.");
                return;
            }

            if (MessageBox.Show($"Weet je zeker dat je de account '{account.Name}' wilt verwijderen (en alle trades)?",
                "Bevestiging", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                foreach (var trade in account.Trades)
                    trade.IsDeleted = true;

                account.IsDeleted = true;
                _db.SaveChanges();
                LoadAccounts();
                LoadTrades();
            }
        }

        // TRADES
        private void DeleteTrade_Click(object sender, RoutedEventArgs e)
        {
            if (TradesGrid.SelectedItem == null)
            {
                MessageBox.Show("Selecteer een trade om te verwijderen.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic selectedTrade = TradesGrid.SelectedItem;
            int tradeId = selectedTrade.TradeId;

            var trade = _db.Trades.FirstOrDefault(t => t.TradeId == tradeId);
            if (trade == null)
            {
                MessageBox.Show("Trade niet gevonden.");
                return;
            }

            if (MessageBox.Show("Weet je zeker dat je deze trade wilt verwijderen?",
                "Bevestiging", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                trade.IsDeleted = true;
                _db.SaveChanges();
                LoadTrades();
            }
        }

        // SYMBOLEN
        private void AddSymbol_Click(object sender, RoutedEventArgs e)
        {
            string code = Microsoft.VisualBasic.Interaction.InputBox("Voer de symboolcode in (bv. XAUUSD):", "Nieuw symbool", "");
            if (string.IsNullOrWhiteSpace(code)) return;

            string name = Microsoft.VisualBasic.Interaction.InputBox("Voer de naam in (bv. Goud):", "Nieuw symbool", "");
            string category = Microsoft.VisualBasic.Interaction.InputBox("Voer de categorie in (bv. Metals, Forex):", "Nieuw symbool", "");

            var symbol = new Symbol
            {
                Code = code.Trim().ToUpper(),
                DisplayName = name,
                Category = category,
                CreatedAt = DateTime.UtcNow
            };

            _db.Symbols.Add(symbol);
            _db.SaveChanges();
            LoadSymbols();
        }

        private void DeleteSymbol_Click(object sender, RoutedEventArgs e)
        {
            if (SymbolsGrid.SelectedItem == null)
            {
                MessageBox.Show("Selecteer een symbool om te verwijderen.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedSymbol = (Symbol)SymbolsGrid.SelectedItem;

            if (MessageBox.Show($"Weet je zeker dat je '{selectedSymbol.Code}' wilt verwijderen?",
                "Bevestiging", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _db.Symbols.Remove(selectedSymbol);
                _db.SaveChanges();
                LoadSymbols();
            }
        }

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private async void CreateUser_Click(object sender, RoutedEventArgs e)
        {
            CreateStatus.Text = "";

            var username = NewUserNameBox.Text?.Trim();
            var email = NewEmailBox.Text?.Trim();
            var password = NewPasswordBox.Password;

            var roleItem = RoleComboBox.SelectedItem as ComboBoxItem;
            var role = roleItem?.Content?.ToString() ?? "Trader";

            var resultText = await CreateUserWithRoleAsync(username, email, password, role);

            CreateStatus.Foreground = resultText.StartsWith("OK")
                ? System.Windows.Media.Brushes.Green
                : System.Windows.Media.Brushes.Red;

            CreateStatus.Text = resultText;

            if (resultText.StartsWith("OK"))
                LoadUsers();
        }

        private async Task<string> CreateUserWithRoleAsync(string? username, string? email, string? password, string role)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return "Vul e-mail en wachtwoord in.";

            email = email.Trim();
            username = string.IsNullOrWhiteSpace(username) ? email : username.Trim();

            // garantir que a role existe
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var roleCreate = await _roleManager.CreateAsync(new IdentityRole(role));
                if (!roleCreate.Succeeded)
                    return "Kon rol niet aanmaken: " + string.Join("; ", roleCreate.Errors.Select(e => e.Description));
            }

            // evitar duplicados
            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null)
                return "E-mailadres is al in gebruik.";

            var user = new ApplicationUser
            {
                UserName = email,   // igual ao teu RegisterWindow
                Email = email,
                FullName = username // se quiseres, ou mete outro campo para nome
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return string.Join("\n", result.Errors.Select(e => e.Description));

            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
                return "User aangemaakt, maar rol faalde: " + string.Join("; ", roleResult.Errors.Select(e => e.Description));

            return $"OK: {role} aangemaakt ({email})";
        }

    }
}
