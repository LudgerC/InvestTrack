using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using InvestTrack.Model.Data;
using InvestTrack.Model.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestTrack.Desktop.Views.TraderUser
{
    public partial class AddTradeWindow : Window
    {
        private readonly InvestTrackDbContext _context;
        public Trade? CreatedTrade { get; private set; }

        public AddTradeWindow(string userId)
        {
            InitializeComponent();

            var options = new DbContextOptionsBuilder<InvestTrackDbContext>()
                .UseSqlite("Data Source=investtrack.db")
                .Options;

            _context = new InvestTrackDbContext(options);

            // Accounts van gebruiker laden
            AccountBox.ItemsSource = _context.Accounts
                .Where(a => a.UserId == userId)
                .ToList();

            // Symbolen laden
            SymbolBox.ItemsSource = _context.Symbols
                .OrderBy(s => s.DisplayName)
                .ToList();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (AccountBox.SelectedItem is not Account account)
            {
                MessageBox.Show("Selecteer een account.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SymbolBox.SelectedItem is not Symbol symbol)
            {
                MessageBox.Show("Selecteer een symbool.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(LotsBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var lots) || lots <= 0)
            {
                MessageBox.Show("Voer een geldig aantal lots in.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(ProfitLossBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var profitLoss))
            {
                MessageBox.Show("Voer een geldig winst/verliesbedrag in.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CreatedTrade = new Trade
            {
                AccountId = account.AccountId,
                SymbolId = symbol.Id,
                Lots = lots,
                ProfitLoss = profitLoss
            };

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
