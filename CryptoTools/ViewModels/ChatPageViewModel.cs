using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CryptoTools.ViewModels;

public class ChatPageViewModel
{
    private readonly ConcurrentQueue<string> _chatMessages = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public event EventHandler? OnMessageReceived;

    public ObservableCollection<string> ChatMessages { get; set; } = new();

    public void UpdateChatMessages(object state)
    {
        while (_chatMessages.TryDequeue(out var message))
        {
            ChatMessages.Add(message);
            OnMessageReceived?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task StartWebSocketListenerAsync()
    {
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();

        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            var context = await listener.GetContextAsync();
            if (context.Request.IsWebSocketRequest)
            {
                var webSocketContext = await context.AcceptWebSocketAsync(null);
                var webSocket = webSocketContext.WebSocket;

                // Handle incoming messages
                var buffer = new byte[1024];
                var result =
                    await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);
                while (!result.CloseStatus.HasValue && !_cancellationTokenSource.IsCancellationRequested)
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _chatMessages.Enqueue(message); // add the message to the queue
                    }

                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer),
                        _cancellationTokenSource.Token);
                }

                if (result.CloseStatus != null)
                    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription,
                        _cancellationTokenSource.Token);
            }
            else
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }

        listener.Stop();
    }

    public void CancelChat()
    {
        _cancellationTokenSource.Cancel();
    }

    public async void SendMessage(byte[] buffer)
    {
        using var client = new ClientWebSocket();
        await client.ConnectAsync(new Uri("ws://localhost:8080/"), _cancellationTokenSource.Token);
        await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
            _cancellationTokenSource.Token);
    }
}