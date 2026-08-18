using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using InvestTrack.Mobile.Services;
using InvestTrack.Model.Data;
using InvestTrack.Model.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Storage;

namespace InvestTrack.Mobile.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _fullName = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private bool _isBusy;
        private bool _isLoggedIn;
        private bool _isRegisterMode;
        private string _loggedInEmail = string.Empty;
        private string _loggedInRole = string.Empty;

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasError));
            }
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set
            {
                _successMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSuccess));
            }
        }

        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
        public bool HasSuccess => !string.IsNullOrWhiteSpace(SuccessMessage);

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set { _isLoggedIn = value; OnPropertyChanged(); }
        }

        public bool IsRegisterMode
        {
            get => _isRegisterMode;
            set
            {
                _isRegisterMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLoginMode));
            }
        }

        public bool IsLoginMode => !IsRegisterMode;

        public string LoggedInEmail
        {
            get => _loggedInEmail;
            set { _loggedInEmail = value; OnPropertyChanged(); }
        }

        public string LoggedInRole
        {
            get => _loggedInRole;
            set { _loggedInRole = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand GoToDashboardCommand { get; }
        public ICommand SwitchToRegisterCommand { get; }
        public ICommand SwitchToLoginCommand { get; }

        public LoginViewModel(ApiService apiService)
        {
            _apiService = apiService;
            LoginCommand = new Command(async () => await ExecuteLoginAsync());
            RegisterCommand = new Command(async () => await ExecuteRegisterAsync());
            LogoutCommand = new Command(ExecuteLogout);
            GoToDashboardCommand = new Command(async () => await ExecuteGoToDashboardAsync());
            SwitchToRegisterCommand = new Command(() =>
            {
                IsRegisterMode = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;
            });
            SwitchToLoginCommand = new Command(() =>
            {
                IsRegisterMode = false;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;
            });
        }

        public void CheckLoginStatus()
        {
            var userId = Preferences.Get("UserId", string.Empty);
            var email = Preferences.Get("UserEmail", string.Empty);
            var role = Preferences.Get("UserRole", string.Empty);

            if (!string.IsNullOrEmpty(userId))
            {
                IsLoggedIn = true;
                LoggedInEmail = email;
                LoggedInRole = role;
            }
            else
            {
                IsLoggedIn = false;
                LoggedInEmail = string.Empty;
                LoggedInRole = string.Empty;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                (Shell.Current as AppShell)?.UpdateRoleNavigation();
            });
        }

        private async Task ExecuteGoToDashboardAsync()
        {
            var role = Preferences.Get("UserRole", "Trader");
            var userId = Preferences.Get("UserId", string.Empty);

            if (role == "Admin")
            {
                await Shell.Current.GoToAsync("//AdminDashboardPage");
            }
            else
            {
                await Shell.Current.GoToAsync($"//TraderDashboardPage?UserId={userId}");
            }
        }

        private void ExecuteLogout()
        {
            Preferences.Remove("UserId");
            Preferences.Remove("UserEmail");
            Preferences.Remove("UserRole");

            IsLoggedIn = false;
            LoggedInEmail = string.Empty;
            LoggedInRole = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
            FullName = string.Empty;
            ConfirmPassword = string.Empty;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                (Shell.Current as AppShell)?.UpdateRoleNavigation();
            });
        }

        private async Task ExecuteLoginAsync()
        {
            if (IsBusy) return;
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Vul uw e-mailadres en wachtwoord in.";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            try
            {
                var result = await _apiService.LoginAsync(Email.Trim(), Password);

                if (!result.Success)
                {
                    // Fallback to local SQLite DB for login
                    try
                    {
                        using var db = DatabaseService.CreateDbContext();
                        var user = db.Users.FirstOrDefault(u => u.Email == Email.Trim());

                        if (user != null)
                        {
                            var hasher = new PasswordHasher<ApplicationUser>();
                            var verificationResult = hasher.VerifyHashedPassword(user, user.PasswordHash ?? "", Password);

                            if (verificationResult == PasswordVerificationResult.Success)
                            {
                                var userRole = db.UserRoles.FirstOrDefault(ur => ur.UserId == user.Id);
                                var roleName = "Trader";
                                if (userRole != null)
                                {
                                    var role = db.Roles.Find(userRole.RoleId);
                                    if (role != null) roleName = role.Name ?? "Trader";
                                }

                                result = new ApiService.LoginResponse
                                {
                                    Success = true,
                                    UserId = user.Id,
                                    Email = user.Email ?? "",
                                    Role = roleName
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Offline Login] Error: {ex}");
                    }

                    if (!result.Success)
                    {
                        ErrorMessage = result.Message + " (Controleer of gegevens kloppen of server online is).";
                        return;
                    }
                }

                // Sla de ingelogde gebruiker op
                Preferences.Set("UserId", result.UserId);
                Preferences.Set("UserEmail", result.Email);
                Preferences.Set("UserRole", result.Role);

                CheckLoginStatus();

                if (result.Role == "Admin")
                {
                    await Shell.Current.GoToAsync("//AdminDashboardPage");
                }
                else
                {
                    await Shell.Current.GoToAsync($"//TraderDashboardPage?UserId={result.UserId}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Fout bij inloggen: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExecuteRegisterAsync()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Vul alle verplichte velden in.";
                return;
            }

            if (Password.Length < 6)
            {
                ErrorMessage = "Het wachtwoord moet minimaal 6 tekens lang zijn.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(ConfirmPassword) && Password != ConfirmPassword)
            {
                ErrorMessage = "De wachtwoorden komen niet overeen.";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            try
            {
                var result = await _apiService.RegisterAsync(Email.Trim(), Password, FullName.Trim());

                if (!result.Success)
                {
                    ErrorMessage = result.Message;
                    return;
                }

                Preferences.Set("UserId", result.UserId);
                Preferences.Set("UserEmail", result.Email);
                Preferences.Set("UserRole", result.Role);

                CheckLoginStatus();

                await Shell.Current.DisplayAlert("Welkom", "Uw account is succesvol aangemaakt!", "Doorgaan");

                await Shell.Current.GoToAsync($"//TraderDashboardPage?UserId={result.UserId}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Fout bij registreren: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
