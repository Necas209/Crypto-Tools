using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using CryptoLib.Models;
using CryptoTools.Extensions;

namespace CryptoTools.ViewModels;

public class RegisterViewModel : ViewModelBase
{
    public Action<string>? OnError;
    public Action? ShowLogin;

    public string UserName { get; set; } = string.Empty;


    public async Task Register(SecureString securePassword, SecureString confirmPassword)
    {
        if (UserName.Length < 5)
        {
            OnError?.Invoke("Username must be at least 5 characters long.");
            return;
        }

        if (securePassword.Length < 8)
        {
            OnError?.Invoke("Password must be at least 8 characters long.");
            return;
        }

        if (securePassword.IsEqualTo(confirmPassword) == false)
        {
            OnError?.Invoke("Passwords do not match.");
            return;
        }

        using var client = new HttpClient();
        var response = await client.PostAsJsonAsync($"{Model.ServerUrl}/register",
            new LoginRequest
            {
                UserName = UserName,
                Password = securePassword.ToPlainText()
            }
        );

        if (!response.IsSuccessStatusCode)
        {
            OnError?.Invoke("Registration failed. Server responded with: " + response.StatusCode);
            return;
        }

        MessageBox.Show("Registration successful. You can now log in.", "Success", MessageBoxButton.OK,
            MessageBoxImage.Information);
        ShowLogin?.Invoke();
    }
}