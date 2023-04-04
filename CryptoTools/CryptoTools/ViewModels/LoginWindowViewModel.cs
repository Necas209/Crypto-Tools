using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using CryptoLib.Extensions;
using CryptoLib.Models;

namespace CryptoTools.ViewModels;

public class LoginWindowViewModel : BaseViewModel
{
    public Action<string>? OnError;
    public Action? ShowApp;

    private static async Task StartWebSocketListenerAsync()
    {
        var serverUri = new Uri("wss://cryptotools.azurewebsites.net/chat");
        await ClientWebSocket.ConnectAsync(serverUri, CancellationToken.None);
    }

    public async void Login(string userName, SecureString securePassword)
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

        UserId = Convert.ToInt32(await response.Content.ReadAsStringAsync());
        // Start listening for messages
        await StartWebSocketListenerAsync();
        await Initialize();
        ShowApp?.Invoke();
    }
}