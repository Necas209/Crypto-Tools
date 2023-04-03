using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CryptoLib.Extensions;
using CryptoLib.Models;

namespace CryptoTools.ViewModels;

public class ChatPageViewModel
{
    public ObservableCollection<string> ChatMessages { get; } = new();
    public event Action? OnMessageReceived;

    public async Task ReceiveMessages()
    {
        while (App.ClientWebSocket.State == WebSocketState.Open)
        {
            var buffer = new ArraySegment<byte>(new byte[4096]);
            var result = await App.ClientWebSocket.ReceiveAsync(buffer, CancellationToken.None);
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
            App.UserId,
            Message = message
        });
        var buffer = Encoding.UTF8.GetBytes(json);
        await App.ClientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
            CancellationToken.None);
    }
}