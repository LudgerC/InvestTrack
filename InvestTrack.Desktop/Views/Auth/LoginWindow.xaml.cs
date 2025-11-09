using System.Windows;
using Microsoft.AspNetCore.Identity;
using InvestTrack.Model.Identity;
using InvestTrack.Desktop.Views.Admin;


namespace InvestTrack.Desktop.Views.Auth
{
    public partial class LoginWindow : Window
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginWindow(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            InitializeComponent();
            _signInManager = signInManager;
            _userManager = userManager;
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailBox.Text?.Trim() ?? "";
            var password = PasswordBox.Password ?? "";

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ErrorText.Text = "Onbekende gebruiker.";
                return;
            }

            var valid = await _userManager.CheckPasswordAsync(user, password);
            if (!valid)
            {
                ErrorText.Text = "Fout wachtwoord.";
                return;
            }

            // check rol
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Trader"))
            {
                var dashboard = new Views.TraderUser.TraderDashboard(user.Id);
                dashboard.Show();
            }
            else if (roles.Contains("Admin"))
            {
                var adminDashboard = new Views.Admin.AdminDashboard();
                adminDashboard.Show();
            }

            Close();
        }

        private void OpenRegister_Click(object sender, RoutedEventArgs e)
        {
            var reg = new RegisterWindow(_userManager);
            reg.Owner = this;
            reg.ShowDialog();
        }
    }
}
