using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security;
using System.Threading.Tasks;
using CryptoLib.Extensions;
using CryptoLib.Models;

namespace CryptoTools.ViewModels;

public class LoginViewModel : ViewModelBase
{
    public Action<string>? OnError;
    public Action? ShowApp;

    public async Task Login(string userName, SecureString securePassword)
    {
        using var client = new HttpClient();
        var passwordHash = securePassword.Hash("SHA256");
        var response = await client.PostAsJsonAsync("https://cryptotools.azurewebsites.net/login",
            new User
            {
                UserName = userName,
                PasswordHash = passwordHash
            }
        );

        if (!response.IsSuccessStatusCode)
        {
            OnError?.Invoke("Login failed. Server responded with: " + response.StatusCode);
            return;
        }

        var userId = Convert.ToInt32(await response.Content.ReadAsStringAsync());
        UserId = userId;
        Model.UserName = userName;
        // Open connection to the chat server
        await Model.OpenConnection();
        // Get the encryption and hashing algorithms
        await Model.GetAlgorithms();
        ShowApp?.Invoke();
    }
}