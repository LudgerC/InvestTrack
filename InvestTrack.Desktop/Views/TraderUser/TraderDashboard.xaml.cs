using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using InvestTrack.Model.Data;
using InvestTrack.Model.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestTrack.Desktop.Views.TraderUser
{
    public partial class TraderDashboard : Window
    {
        private readonly InvestTrackDbContext _context;

        public TraderDashboard()
        {
            InitializeComponent();
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
            AccountsGrid.ItemsSource = _context.Accounts.AsNoTracking().ToList();
            TradesGrid.ItemsSource = _context.Trades.AsNoTracking().ToList();
            FavoritesList.ItemsSource = _context.FavoriteTrades.AsNoTracking().ToList();
        }

        private void AddAccount_Click(object sender, RoutedEventArgs e)
        {
            var account = new Account
            {
                Name = "Nieuw Account",
                Currency = "EUR",
                Balance = 0
            };

            _context.Accounts.Add(account);
            _context.SaveChanges();
            LoadData();

            MessageBox.Show("Nieuw account toegevoegd!", "InvestTrack", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
