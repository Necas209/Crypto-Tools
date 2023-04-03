using System;
using System.Collections.ObjectModel;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoTools.ViewModels;

public class ChatPageViewModel : BaseViewModel
{
    public ObservableCollection<string> ChatMessages { get; } = new();
    public event Action? OnMessageReceived;

    public async Task ReceiveMessages()
    {
        while (ClientWebSocket.State == WebSocketState.Open)
        {
            var buffer = new ArraySegment<byte>(new byte[4096]);
            var result = await ClientWebSocket.ReceiveAsync(buffer, CancellationToken.None);
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

    public static async void SendMessage(string message)
    {
        var json = JsonSerializer.Serialize(new
        {
            UserId,
            Message = message
        });
        var buffer = Encoding.UTF8.GetBytes(json);
        await ClientWebSocket.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );
    }
}