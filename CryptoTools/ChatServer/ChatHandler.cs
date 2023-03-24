using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace ChatServer;

public static class ChatHandler
{
    private static readonly WebSocketManager WebSocketManager = new();

    public static async Task Handle(WebSocket webSocket)
    {
        var webSocketConnection = WebSocketManager.AddWebSocket(webSocket);
        await SendWelcomeMessage(webSocketConnection);
        try
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                await WebSocketManager.BroadcastMessage(message);

                buffer = new byte[1024 * 4];
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket connection closed: {ex.Message}");
        }
        finally
        {
            WebSocketManager.RemoveWebSocket(webSocketConnection.Id);
        }
    }

    private static async Task SendWelcomeMessage(WebSocketConnection webSocketConnection)
    {
        const string welcomeMessage = "Welcome to the chat room!";
        await SendMessage(welcomeMessage, webSocketConnection.WebSocket);
        foreach (var connection in WebSocketManager.WebSockets)
            if (connection != webSocketConnection)
            {
                var joinedMessage = $"{webSocketConnection.Id} has joined the chat.";
                await SendMessage(joinedMessage, connection.WebSocket);
            }
    }

    private static async Task SendMessage(string message, WebSocket webSocket)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
            CancellationToken.None);
    }
}

public class WebSocketConnection
{
    public string Id { get; init; } = null!;
    public WebSocket WebSocket { get; init; } = null!;
}

public class WebSocketManager
{
    private readonly ConcurrentDictionary<string, WebSocketConnection> _webSockets = new();
    private int _counter;

    public IEnumerable<WebSocketConnection> WebSockets => _webSockets.Values;

    public WebSocketConnection AddWebSocket(WebSocket webSocket)
    {
        var webSocketConnection = new WebSocketConnection
        {
            Id = Interlocked.Increment(ref _counter).ToString(),
            WebSocket = webSocket
        };

        _webSockets.TryAdd(webSocketConnection.Id, webSocketConnection);
        return webSocketConnection;
    }

    public async void RemoveWebSocket(string id)
    {
        if (_webSockets.TryRemove(id, out _))
            await BroadcastMessage($"{id} has left the chat.");
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