using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CryptoLib.Models;
using CryptoTools.Models;

namespace CryptoTools.ViewModels;

public class LoginViewModel : ViewModelBase
{
    public Action<string> OnError;
    public Action ShowApp;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public async Task Login()
    {
        using var client = new HttpClient();
        var response = await client.PostAsJsonAsync($"{Model.ServerUrl}/login",
            new LoginRequest
            {
                UserName = UserName,
                Password = Password
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
        // Save the token to the local storage
        await Model.SaveToken();
        // Open the app
        await OpenApp();
    }

    private async Task OpenApp()
    {
        // Open connection to the chat server
        await Model.OpenConnection();
        // Get the encryption and hashing algorithms
        await Model.GetAlgorithms();
        ShowApp?.Invoke();
    }
}