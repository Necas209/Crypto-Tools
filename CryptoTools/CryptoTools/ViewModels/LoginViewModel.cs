using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using CryptoLib.Models;
using CryptoTools.Extensions;

namespace CryptoTools.ViewModels;

public class LoginViewModel : ViewModelBase
{
    public Action<string>? OnError;
    public Action? ShowApp;

    public async Task Login(string userName, SecureString securePassword)
    {
        using var client = new HttpClient();
        var userNameBytes = Encoding.UTF8.GetBytes(userName);
        var plainTextPassword = securePassword.ToPlainText();
        var passwordBytes = Encoding.UTF8.GetBytes(plainTextPassword);
        var response = await client.PostAsJsonAsync($"{Model.ServerUrl}/login",
            new LoginRequest
            {
                UserName = Convert.ToBase64String(userNameBytes),
                Password = Convert.ToBase64String(passwordBytes)
            }
        );
        if (!response.IsSuccessStatusCode)
        {
            OnError?.Invoke("Login failed. Server responded with: " + response.StatusCode);
            return;
        }

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (loginResponse == null)
        {
            OnError?.Invoke("Login failed. Server responded with: " + response.StatusCode);
            return;
        }

        Model.AccessToken = loginResponse.AccessToken;
        // Open connection to the chat server
        await Model.OpenConnection();
        // Get the encryption and hashing algorithms
        await Model.GetAlgorithms();
        ShowApp?.Invoke();
    }
}