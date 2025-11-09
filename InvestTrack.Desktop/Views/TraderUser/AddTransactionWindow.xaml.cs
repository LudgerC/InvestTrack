using System;
using System.Globalization;
using System.Windows;
using InvestTrack.Model.Models;

namespace InvestTrack.Desktop.Views.TraderUser
{
    public partial class AddTransactionWindow : Window
    {
        public Transaction? CreatedTransaction { get; private set; }

        private readonly int _accountId;

        public AddTransactionWindow(int accountId)
        {
            InitializeComponent();
            _accountId = accountId;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(AmountBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
            {
                MessageBox.Show("Voer een geldig bedrag in.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (WithdrawRadio.IsChecked == true)
                amount *= -1; // Negatief bij withdrawal

            CreatedTransaction = new Transaction
            {
                AccountId = _accountId,
                Amount = amount,
                Type = amount > 0 ? "Deposit" : "Withdrawal",
                Note = NoteBox.Text.Trim(),
                CreatedAt = DateTime.UtcNow
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
