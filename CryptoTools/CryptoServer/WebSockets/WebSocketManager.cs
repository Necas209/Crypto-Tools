using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace CryptoServer.WebSockets;

public class WebSocketManager
{
    private readonly ConcurrentDictionary<string, WebSocketConnection> _webSockets = new();
    private int _counter;

    public IEnumerable<WebSocketConnection> WebSockets => _webSockets.Values;

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

    public void RemoveWebSocket(WebSocketConnection connection)
    {
        _webSockets.TryRemove(connection.ConnectionId, out _);
    }
}