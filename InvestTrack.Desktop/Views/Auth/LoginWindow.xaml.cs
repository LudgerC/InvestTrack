using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using InvestTrack.Model.Identity;

namespace InvestTrack.Desktop.Views.Auth
{
    public partial class LoginWindow : Window
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        // Parameterloze constructor (haalt services uit App.ServiceProvider)
        public LoginWindow()
        {
            InitializeComponent();
            _signInManager = App.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
            _userManager = App.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailBox.Text?.Trim() ?? "";
            var pwd = PasswordBox.Password ?? "";

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ErrorText.Text = "Onbekende gebruiker.";
                return;
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, pwd, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                ErrorText.Text = "Login mislukt.";
            }
        }

        private void OpenRegister_Click(object sender, RoutedEventArgs e)
        {
            var reg = new RegisterWindow();
            reg.Owner = this;
            reg.ShowDialog();
        }
    }
}
