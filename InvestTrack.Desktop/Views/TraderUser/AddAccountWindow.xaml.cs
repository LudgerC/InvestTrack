using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using InvestTrack.Model.Models;

namespace InvestTrack.Desktop.Views.TraderUser
{
    public partial class AddAccountWindow : Window
    {
        public Account? CreatedAccount { get; private set; }

        private readonly string _userId;

        public AddAccountWindow(string userId)
        {
            InitializeComponent();
            _userId = userId;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text) || CurrencyBox.SelectedItem == null)
            {
                MessageBox.Show("Vul alle velden in.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(BalanceBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var balance))
            {
                MessageBox.Show("Voer een geldig bedrag in.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CreatedAccount = new Account
            {
                Name = NameBox.Text.Trim(),
                Currency = ((ComboBoxItem)CurrencyBox.SelectedItem).Content.ToString()!,
                Balance = balance,
                UserId = _userId
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
