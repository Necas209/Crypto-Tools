using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using CryptoLib.Extensions;
using CryptoLib.Models;

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
        var passwordHash = securePassword.Hash("SHA256");
        var response = await client.PostAsJsonAsync("https://cryptotools.azurewebsites.net/register",
            new User
            {
                UserName = UserName,
                PasswordHash = passwordHash
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