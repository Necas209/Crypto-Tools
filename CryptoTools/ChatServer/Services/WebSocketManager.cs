using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace ChatServer.Services;

public class WebSocketManager
{
    private readonly ConcurrentDictionary<string, WebSocketConnection> _webSockets = new();
    private int _counter;

    public WebSocketConnection AddWebSocket(WebSocket webSocket)
    {
        var webSocketConnection = new WebSocketConnection
        {
            ConnectionId = Interlocked.Increment(ref _counter).ToString(),
            WebSocket = webSocket
        };

        _webSockets.TryAdd(webSocketConnection.ConnectionId, webSocketConnection);
        return webSocketConnection;
    }

    public async void RemoveWebSocket(WebSocketConnection webSocketConnection)
    {
        if (_webSockets.TryRemove(webSocketConnection.ConnectionId, out _))
            await BroadcastMessage($"{webSocketConnection.UserName} has left the chat.");
    }

    public async Task BroadcastMessage(string message)
    {
        foreach (var connection in _webSockets.Values)
            await SendMessage(message, connection.WebSocket);
    }

    private static async Task SendMessage(string message, WebSocket webSocket)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
            CancellationToken.None);
    }
}