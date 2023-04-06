using System.Net.WebSockets;

namespace CryptoServer.WebSockets;

public class WebSocketConnection
{
    public string UserName { get; init; } = string.Empty;
    public WebSocket WebSocket { get; init; } = null!;
}