using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CryptoTools.Services;

namespace CryptoTools.ViewModels;

public class ChatPageViewModel
{
    private ClientWebSocket _clientWebSocket = new();
    private int _userId;
    public ObservableCollection<string> ChatMessages { get; } = new();
    public string UserName { get; set; } = string.Empty;

    public event Action<bool>? EnterChat;
    public event Action<bool>? LeaveChat;
    public event Action? OnMessageReceived;
    public event Action<string>? OnError;

    private async Task StartWebSocketListenerAsync()
    {
        var serverUri = new Uri("wss://cryptotools.azurewebsites.net/chat");
        await _clientWebSocket.ConnectAsync(serverUri, CancellationToken.None);
        // start a new thread to receive messages from the server
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
        var json = JsonSerializer.Serialize(new
        {
            UserId = _userId,
            Message = message
        });
        var buffer = Encoding.UTF8.GetBytes(json);
        await _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
            CancellationToken.None);
    }

    public async void Login(SecureString securePassword)
    {
        using var client = new HttpClient();
        var passwordHash = securePassword.Hash("SHA256");
        var response = await client.GetAsync(
            $"https://cryptotools.azurewebsites.net/login?username={UserName}&passwordHash={passwordHash}"
        );
        if (!response.IsSuccessStatusCode)
        {
            OnError?.Invoke("Login failed. Server responded with: " + response.StatusCode);
            return;
        }

        _userId = Convert.ToInt32(await response.Content.ReadAsStringAsync());
        // Start listening for messages
        EnterChat?.Invoke(true);
        await StartWebSocketListenerAsync();
    }

    public async void Logout()
    {
        LeaveChat?.Invoke(false);
        await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Logout", CancellationToken.None);
        _clientWebSocket.Dispose();
        _clientWebSocket = new ClientWebSocket();
        // Reset the UI
        // ChatMessages.Clear();
    }
}