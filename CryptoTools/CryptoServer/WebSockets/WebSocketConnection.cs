using System.Net.WebSockets;

namespace CryptoServer.WebSockets;

public class WebSocketConnection
{
    public WebSocket WebSocket { get; init; } = null!;

    public string UserName { get; init; } = string.Empty;

    public string Token { get; init; } = string.Empty;
}