using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;

namespace InvestTrack.Mobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        // Op Android emulator: 10.0.2.2 = jouw Windows-PC (host machine)
        // Op Windows: gewoon localhost
        private static string BaseUrl
        {
            get
            {
                if (DeviceInfo.Platform == DevicePlatform.Android)
                    return "http://10.0.2.2:5113";
                return "http://localhost:5113";
            }
        }

        public ApiService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
        }

        // ──────────────────────────────────────────────
        // AUTH
        // ──────────────────────────────────────────────

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class LoginResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }

        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            try
            {
                var payload = new LoginRequest { Email = email, Password = password };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{BaseUrl}/api/auth/login", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<LoginResponse>(responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result ?? new LoginResponse { Success = false, Message = "Onbekende fout." };
            }
            catch (HttpRequestException ex)
            {
                return new LoginResponse { Success = false, Message = $"Server niet bereikbaar ({ex.Message}). Zorg dat de web-app actief is op {BaseUrl}." };
            }
            catch (TaskCanceledException ex)
            {
                return new LoginResponse { Success = false, Message = $"Verbinding timed out ({ex.Message}). Controleer {BaseUrl}." };
            }
            catch (Exception ex)
            {
                return new LoginResponse { Success = false, Message = $"Fout: {ex.Message}" };
            }
        }

        public class RegisterRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string? FullName { get; set; }
        }

        public async Task<LoginResponse> RegisterAsync(string email, string password, string? fullName)
        {
            try
            {
                var payload = new RegisterRequest { Email = email, Password = password, FullName = fullName };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{BaseUrl}/api/auth/register", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<LoginResponse>(responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result ?? new LoginResponse { Success = false, Message = "Onbekende fout bij registratie." };
            }
            catch (HttpRequestException ex)
            {
                return new LoginResponse { Success = false, Message = $"Server niet bereikbaar ({ex.Message}). Zorg dat de web-app actief is op {BaseUrl}." };
            }
            catch (TaskCanceledException ex)
            {
                return new LoginResponse { Success = false, Message = $"Verbinding timed out ({ex.Message})." };
            }
            catch (Exception ex)
            {
                return new LoginResponse { Success = false, Message = $"Fout: {ex.Message}" };
            }
        }

        // ──────────────────────────────────────────────
        // TRADER DASHBOARD
        // ──────────────────────────────────────────────

        public class AccountDto
        {
            public int AccountId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string AccountName { get; set; } = string.Empty;
            public decimal Balance { get; set; }
            public string Currency { get; set; } = "EUR";
        }

        public class TradeDto
        {
            public int TradeId { get; set; }
            public string SymbolCode { get; set; } = string.Empty;
            public string SymbolName { get; set; } = string.Empty;
            public decimal Lots { get; set; }
            public decimal ProfitLoss { get; set; }
            public string AccountName { get; set; } = string.Empty;
            public int AccountId { get; set; }
            public bool IsFavorite { get; set; }
        }

        public class SymbolDto
        {
            public int Id { get; set; }
            public int SymbolId
            {
                get => _symbolId != 0 ? _symbolId : Id;
                set => _symbolId = value;
            }
            private int _symbolId;
            public string Code { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public string Category { get; set; } = "Algemeen";
        }

        public class DashboardResponse
        {
            public decimal TotalBalance { get; set; }
            public List<AccountDto> Accounts { get; set; } = new();
            public List<TradeDto> Trades { get; set; } = new();
            public List<SymbolDto> Symbols { get; set; } = new();
        }

        public async Task<DashboardResponse?> GetDashboardAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/api/trader/dashboard?userId={userId}");
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<DashboardResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return null;
            }
        }
        public async Task<List<TradeDto>> GetFavoritesAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/api/trader/favorites?userId={userId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<TradeDto>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Favorites API] Error: {ex.Message}");
            }
            return new();
        }

        public async Task<List<InvestTrack.Model.Contracts.SymbolDto>> GetSymbolsListAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/api/symbols");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<List<InvestTrack.Model.Contracts.SymbolDto>>(json, options) ?? new();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Symbols API] Error: {ex.Message}");
            }
            return new();
        }

        public class CreateAccountRequest
        {
            public string UserId { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Currency { get; set; } = "EUR";
            public decimal InitialBalance { get; set; } = 1000.00m;
        }

        public async Task<bool> AddAccountAsync(CreateAccountRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{BaseUrl}/api/trader/account", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public class CreateTradeRequest
        {
            public int AccountId { get; set; }
            public int SymbolId { get; set; }
            public string? SymbolCode { get; set; }
            public decimal Lots { get; set; }
            public decimal ProfitLoss { get; set; }
        }

        public async Task<(bool Success, string Error)> AddTradeAsync(CreateTradeRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{BaseUrl}/api/trader/trade", content);
                if (response.IsSuccessStatusCode) return (true, string.Empty);
                var body = await response.Content.ReadAsStringAsync();
                return (false, $"Server fout {(int)response.StatusCode}: {body}");
            }
            catch (Exception ex)
            {
                return (false, $"Verbindingsfout: {ex.Message}");
            }
        }

        public class TransactionRequest
        {
            public int AccountId { get; set; }
            public decimal Amount { get; set; }
            public string? Note { get; set; }
        }

        public async Task<bool> DepositAsync(int accountId, decimal amount)
        {
            try
            {
                var json = JsonSerializer.Serialize(new TransactionRequest { AccountId = accountId, Amount = amount });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{BaseUrl}/api/trader/deposit", content);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> WithdrawAsync(int accountId, decimal amount)
        {
            try
            {
                var json = JsonSerializer.Serialize(new TransactionRequest { AccountId = accountId, Amount = amount });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{BaseUrl}/api/trader/withdraw", content);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> DeleteTradeAsync(int tradeId)
        {
            try
            {
                var json = JsonSerializer.Serialize(tradeId);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{BaseUrl}/api/trader/deletetrade", content);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> ToggleFavoriteAsync(int tradeId)
        {
            try
            {
                var json = JsonSerializer.Serialize(tradeId);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{BaseUrl}/api/trader/togglefavorite", content);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        // ================= ADMIN APIS =================

        public class AdminDashboardResponse
        {
            public List<AdminUserItem> Users { get; set; } = new();
            public List<AdminAccountItem> Accounts { get; set; } = new();
            public List<AdminTradeItem> Trades { get; set; } = new();
            public List<SymbolDto> Symbols { get; set; } = new();
        }

        public class AdminUserItem
        {
            public string Id { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }

        public class AdminAccountItem
        {
            public int AccountId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Currency { get; set; } = string.Empty;
            public decimal Balance { get; set; }
            public string UserId { get; set; } = string.Empty;
            public string UserEmail { get; set; } = string.Empty;
        }

        public class AdminTradeItem
        {
            public int TradeId { get; set; }
            public string SymbolCode { get; set; } = string.Empty;
            public decimal Lots { get; set; }
            public decimal ProfitLoss { get; set; }
            public string AccountName { get; set; } = string.Empty;
        }

        public class CreateUserRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Role { get; set; } = "Trader";
            public string? FullName { get; set; }
        }

        public class AddSymbolRequest
        {
            public string Code { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public string Category { get; set; } = "Algemeen";
        }

        public async Task<AdminDashboardResponse?> GetAdminDashboardAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/api/AdminApi/dashboard");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<AdminDashboardResponse>(content, options);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminApi] Dashboard error: {ex.Message}");
            }
            return null;
        }

        public async Task<bool> AdminCreateUserAsync(CreateUserRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{BaseUrl}/api/AdminApi/create-user", content);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> AdminDeleteUserAsync(string userId)
        {
            try
            {
                var json = JsonSerializer.Serialize(userId);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{BaseUrl}/api/AdminApi/delete-user", content);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> AdminAddSymbolAsync(AddSymbolRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{BaseUrl}/api/AdminApi/add-symbol", content);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> AdminDeleteSymbolAsync(int symbolId)
        {
            try
            {
                var json = JsonSerializer.Serialize(symbolId);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{BaseUrl}/api/AdminApi/delete-symbol", content);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> AdminDeleteAccountAsync(int accountId)
        {
            try
            {
                var json = JsonSerializer.Serialize(accountId);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{BaseUrl}/api/AdminApi/delete-account", content);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> AdminDeleteTradeAsync(int tradeId)
        {
            try
            {
                var json = JsonSerializer.Serialize(tradeId);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{BaseUrl}/api/AdminApi/delete-trade", content);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }
    }
}
