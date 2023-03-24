using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoTools.ViewModels;

public class ChatPageViewModel
{
    private readonly ClientWebSocket _clientWebSocket = new();
    public ObservableCollection<string> ChatMessages { get; } = new();
    public event Action? EnterChat;
    public event Action? OnMessageReceived;

    private async Task StartWebSocketListenerAsync()
    {
        var serverUri = new Uri("wss://cryptotools.azurewebsites.net/chat");
        await _clientWebSocket.ConnectAsync(serverUri, CancellationToken.None);
        await ReceiveMessages();
    }

    private async Task ReceiveMessages()
    {
        while (_clientWebSocket.State == WebSocketState.Open)
        {
            var buffer = new ArraySegment<byte>(new byte[4096]);
            var result = await _clientWebSocket.ReceiveAsync(buffer, CancellationToken.None);
            var message = Encoding.UTF8.GetString(buffer.Array ?? Array.Empty<byte>(), 0, result.Count);
            // add the received message to the UI
            AddMessage(message);
            // wait for a short period before polling for more messages
            await Task.Delay(100);
        }
    }

    private void AddMessage(string message)
    {
        ChatMessages.Add(message);
        OnMessageReceived?.Invoke();
    }

    public async void SendMessage(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        await _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
            CancellationToken.None);
    }

    public async void Login(string username, SecureString securePassword)
    {
        // Create a new HttpClient object
        var client = new HttpClient();
        // TODO: Use a more secure way to encode the password, e.g. using a hash function
        var password = new NetworkCredential(string.Empty, securePassword).Password;
        // Send a GET request to the login endpoint
        var response = await client.GetAsync(
            $"https://cryptotools.azurewebsites.net/login?username={username}&password={password}"
        );
        if (!response.IsSuccessStatusCode) return;
        // Start listening for messages
        EnterChat?.Invoke();
        await StartWebSocketListenerAsync();
    }
}