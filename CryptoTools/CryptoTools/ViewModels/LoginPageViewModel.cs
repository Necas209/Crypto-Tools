using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using CryptoLib.Extensions;
using CryptoLib.Models;
using CryptoTools.Views;

namespace CryptoTools.ViewModels;

public class LoginPageViewModel : BaseViewModel
{
    public event Action<string>? OnError;

    private static async Task StartWebSocketListenerAsync()
    {
        var serverUri = new Uri("wss://cryptotools.azurewebsites.net/chat");
        await App.ClientWebSocket.ConnectAsync(serverUri, CancellationToken.None);
    }

    public async void Login(string UserName, SecureString securePassword)
    {
        using var client = new HttpClient();
        var passwordHash = securePassword.Hash("SHA256");
        var response = await client.PostAsJsonAsync("https://cryptotools.azurewebsites.net/login",
            new User
            {
                UserName = UserName,
                PasswordHash = passwordHash
            }
        );

        if (!response.IsSuccessStatusCode)
        {
            OnError?.Invoke("Login failed. Server responded with: " + response.StatusCode);
            return;
        }

        App.UserId = Convert.ToInt32(await response.Content.ReadAsStringAsync());
        App.UserName = UserName;

        // Start listening for messages
        await StartWebSocketListenerAsync();

        App.ShowApp?.Invoke();
    }
}