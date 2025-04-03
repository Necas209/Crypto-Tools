using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace CryptoServer.WebSockets;

public record WebSocketConnection(WebSocket WebSocket, string UserName, string Jwt);

public class WebSocketManager
{
    private readonly ConcurrentDictionary<string, WebSocketConnection> _webSockets = new();

    public IEnumerable<WebSocketConnection> WebSockets => _webSockets.Values;

    public WebSocketConnection AddWebSocket(WebSocket webSocket, string userName, string jwt)
    {
        var webSocketConnection = new WebSocketConnection(webSocket, userName, jwt);
        if (!_webSockets.TryAdd(userName, webSocketConnection))
            throw new InvalidOperationException($"WebSocket for user {userName} already exists.");

        return webSocketConnection;
    }

    public void RemoveWebSocket(WebSocketConnection connection) => _webSockets.TryRemove(connection.UserName, out _);
}