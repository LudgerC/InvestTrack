using System;
using System.Linq;
using System.Windows;
using InvestTrack.Model.Data;
using InvestTrack.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InvestTrack.Desktop.Views.TraderUser
{
    public partial class TraderDashboard : Window
    {
        private readonly string _userId;
        private readonly InvestTrackDbContext _context;

        public TraderDashboard(string userId)
        {
            InitializeComponent();
            _userId = userId;
            _context = CreateDbContext();
            LoadData();
        }

        private static InvestTrackDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<InvestTrackDbContext>()
                .UseSqlite("Data Source=investtrack.db")
                .Options;

            return new InvestTrackDbContext(options);
        }

        private void LoadData()
        {
            // Accounts
            AccountsGrid.ItemsSource = _context.Accounts
                .AsNoTracking()
                .Where(a => a.UserId == _userId)
                .ToList();

            // Filters en tabellen die hiervan afhangen
            LoadAccountsFilter();   // vult AccountFilter (met "Alle accounts")
            LoadTrades();           // vult TradesGrid volgens de (nieuwe) filter
            LoadSymbols();          // vult Symbolen-tab (alles of volgens filter)
            LoadFavoritesAccountFilter();
            LoadFavorites();

        }

        private void AddAccount_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddAccountWindow(_userId) { Owner = this };

            if (addWindow.ShowDialog() == true && addWindow.CreatedAccount != null)
            {
                _context.Accounts.Add(addWindow.CreatedAccount);
                _context.SaveChanges();
                LoadData();

                MessageBox.Show("Nieuw account toegevoegd!", "InvestTrack",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var login = new Views.Auth.LoginWindow(
                App.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.SignInManager<Model.Identity.ApplicationUser>>(),
                App.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Model.Identity.ApplicationUser>>()
            );

            login.Show();
            Close();
        }

        private void CategoryFilter_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CategoryFilter.SelectedItem is string selectedCategory)
                LoadSymbols(selectedCategory);
        }

        // ===============================
        //     TRANSACTIES & TRADES
        // ===============================

        private void Deposit_Click(object sender, RoutedEventArgs e)
        {
            if (AccountsGrid.SelectedItem is not Account selectedAccount)
            {
                MessageBox.Show("Selecteer eerst een account.", "InvestTrack",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = new AddTransactionWindow(selectedAccount.AccountId) { Owner = this };

            if (window.ShowDialog() == true && window.CreatedTransaction != null)
            {
                _context.Transactions.Add(window.CreatedTransaction);

                // gebruik tracked entity uit context
                var account = _context.Accounts.First(a => a.AccountId == selectedAccount.AccountId);
                account.Balance += window.CreatedTransaction.Amount;

                _context.SaveChanges();
                LoadData();

                MessageBox.Show("Deposit succesvol toegevoegd!", "InvestTrack",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Withdraw_Click(object sender, RoutedEventArgs e)
        {
            if (AccountsGrid.SelectedItem is not Account selectedAccount)
            {
                MessageBox.Show("Selecteer eerst een account.", "InvestTrack",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = new AddTransactionWindow(selectedAccount.AccountId) { Owner = this };

            // standaard op Withdrawal
            window.WithdrawRadio.IsChecked = true;
            window.DepositRadio.IsChecked = false;

            if (window.ShowDialog() == true && window.CreatedTransaction != null)
            {
                // saldo-check op huidige (notracked) selectie
                if (selectedAccount.Balance + window.CreatedTransaction.Amount < 0)
                {
                    MessageBox.Show("Onvoldoende saldo voor deze opname.", "InvestTrack",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _context.Transactions.Add(window.CreatedTransaction);

                // werk met tracked entity uit context
                var account = _context.Accounts.First(a => a.AccountId == selectedAccount.AccountId);
                account.Balance += window.CreatedTransaction.Amount; // amount is negatief bij withdrawal

                _context.SaveChanges();
                LoadData();

                MessageBox.Show("Withdrawal succesvol uitgevoerd!", "InvestTrack",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AddTrade_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddTradeWindow(_userId) { Owner = this };

            if (window.ShowDialog() == true && window.CreatedTrade != null)
            {
                _context.Trades.Add(window.CreatedTrade);

                var account = _context.Accounts.First(a => a.AccountId == window.CreatedTrade.AccountId);
                account.Balance += window.CreatedTrade.ProfitLoss;

                _context.SaveChanges();
                LoadTrades(); // respecteert huidige AccountFilter
                LoadData();   // herlaadt accountsaldo’s

                MessageBox.Show("Trade succesvol toegevoegd!", "InvestTrack",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void TradesGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (TradesGrid.SelectedItem == null) return;

            var tradeRow = TradesGrid.SelectedItem;
            var tradeIdProp = tradeRow.GetType().GetProperty("TradeId");

            if (tradeIdProp == null) return;

            int tradeId = (int)tradeIdProp.GetValue(tradeRow)!;

            var trade = _context.Trades
                .Include(t => t.Account)
                .Include(t => t.Symbol)
                .FirstOrDefault(t => t.TradeId == tradeId);

            if (trade == null) return;

            var confirm = MessageBox.Show(
                $"Weet je zeker dat je de trade ({trade.Symbol.Code}) wilt verwijderen?",
                "Bevestiging",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                trade.Account!.Balance -= trade.ProfitLoss;
                _context.Trades.Remove(trade);
                _context.SaveChanges();

                LoadTrades();
                LoadData();

                MessageBox.Show("Trade verwijderd en saldo aangepast.", "InvestTrack",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }



        private void LoadTrades()
        {
            var selectedAccountId = (int?)AccountFilter.SelectedValue;

            var tradesQuery = _context.Trades
                .Include(t => t.Symbol)
                .Include(t => t.Account)
                .AsNoTracking()
                .Where(t => t.Account.UserId == _userId); // 🔒 enkel trades van deze gebruiker

            if (selectedAccountId.HasValue && selectedAccountId.Value != 0)
                tradesQuery = tradesQuery.Where(t => t.AccountId == selectedAccountId.Value);

            TradesGrid.ItemsSource = tradesQuery
                .Select(t => new
                {
                    t.TradeId,
                    SymbolCode = t.Symbol.Code,
                    SymbolName = t.Symbol.DisplayName,
                    Lots = t.Lots,
                    ProfitLoss = t.ProfitLoss,
                    AccountName = t.Account.Name,
                    IsFavorite = _context.FavoriteTrades.Any(f => f.TradeId == t.TradeId)
                })
                .ToList();


        }

        private void LoadAccountsFilter()
        {
            var accounts = _context.Accounts
                .AsNoTracking()
                .Where(a => a.UserId == _userId)
                .ToList();

            // “Alle accounts” bovenaan
            accounts.Insert(0, new Account { AccountId = 0, Name = "Alle accounts", UserId = _userId });

            AccountFilter.ItemsSource = accounts;
            AccountFilter.SelectedIndex = 0;
        }

        private void AccountFilter_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LoadTrades();
        }

        // ======== Symbolen (enkel 1 definitie!) ========
        private void LoadSymbols(string? category = null)
        {
            var symbolsQuery = _context.Symbols.AsNoTracking();

            if (!string.IsNullOrEmpty(category) && category != "Alle")
                symbolsQuery = symbolsQuery.Where(s => s.Category == category);

            SymbolsGrid.ItemsSource = symbolsQuery.ToList();

            if (CategoryFilter.Items.Count == 0)
            {
                var categories = _context.Symbols
                    .Select(s => s.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                categories.Insert(0, "Alle");
                CategoryFilter.ItemsSource = categories;
                CategoryFilter.SelectedIndex = 0;
            }
        }

        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext != null)
            {
                var data = element.DataContext;
                var tradeIdProp = data.GetType().GetProperty("TradeId");

                if (tradeIdProp != null)
                {
                    int tradeId = (int)tradeIdProp.GetValue(data)!;

                    var favorite = _context.FavoriteTrades.FirstOrDefault(f => f.TradeId == tradeId);

                    if (favorite == null)
                    {
                        _context.FavoriteTrades.Add(new FavoriteTrade { TradeId = tradeId });
                    }
                    else
                    {
                        _context.FavoriteTrades.Remove(favorite);
                    }

                    _context.SaveChanges();

                    // ✅ Dit moet blijven
                    LoadTrades();
                    LoadFavorites();
                }
            }
        }


        private void LoadFavoritesAccountFilter()
        {
            var accounts = _context.Accounts
                .AsNoTracking()
                .Where(a => a.UserId == _userId)
                .ToList();

            accounts.Insert(0, new Account { AccountId = 0, Name = "Alle accounts", UserId = _userId });

            FavoritesAccountFilter.ItemsSource = accounts;
            FavoritesAccountFilter.SelectedIndex = 0;
        }

        private void FavoritesAccountFilter_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LoadFavorites();
        }

        private void LoadFavorites()
        {
            var selectedAccountId = (int?)FavoritesAccountFilter.SelectedValue;

            var favoritesQuery = _context.FavoriteTrades
                .Include(f => f.Trade)
                .ThenInclude(t => t.Symbol)
                .Include(f => f.Trade)
                .ThenInclude(t => t.Account)
                .AsNoTracking()
                .Where(f => f.Trade.Account.UserId == _userId);

            if (selectedAccountId.HasValue && selectedAccountId.Value != 0)
                favoritesQuery = favoritesQuery.Where(f => f.Trade.AccountId == selectedAccountId.Value);

            FavoritesGrid.ItemsSource = favoritesQuery
                .Select(f => new
                {
                    f.Trade.TradeId,
                    SymbolCode = f.Trade.Symbol.Code,
                    SymbolName = f.Trade.Symbol.DisplayName,
                    Lots = f.Trade.Lots,
                    ProfitLoss = f.Trade.ProfitLoss,
                    AccountName = f.Trade.Account.Name
                })
                .ToList();
        }


    }
}
