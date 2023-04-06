using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace CryptoServer.WebSockets;

public class WebSocketManager
{
    private readonly ConcurrentDictionary<string, WebSocketConnection> _webSockets = new();

    public IEnumerable<WebSocketConnection> WebSockets => _webSockets.Values;

    public WebSocketConnection AddWebSocket(WebSocket webSocket, string userName)
    {
        var webSocketConnection = new WebSocketConnection
        {
            WebSocket = webSocket,
            UserName = userName
        };

        _webSockets.TryAdd(userName, webSocketConnection);
        return webSocketConnection;
    }

    public void RemoveWebSocket(WebSocketConnection connection)
    {
        _webSockets.TryRemove(connection.UserName, out _);
    }
}