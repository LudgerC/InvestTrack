using System.Windows;
using InvestTrack.Model.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace InvestTrack.Desktop.Views.Auth
{
    public partial class RegisterWindow : Window
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public RegisterWindow(UserManager<ApplicationUser> userManager)
        {
            InitializeComponent();
            _userManager = userManager;
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailBox.Text?.Trim() ?? "";
            var password = PasswordBox.Password ?? "";
            var name = FullNameBox.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ErrorText.Text = "Vul alle velden in.";
                return;
            }

            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                ErrorText.Text = "E-mailadres is al in gebruik.";
                return;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = name
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Trader");
                MessageBox.Show("Registratie succesvol! Je kunt nu inloggen.", "Gelukt");
                Close();
            }
            else
            {
                ErrorText.Text = string.Join("\n", result.Errors.Select(e => e.Description));
            }
        }
        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow(
                App.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>(),
                App.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>()
            );

            login.Show();
            this.Close();
        }
    }
}
