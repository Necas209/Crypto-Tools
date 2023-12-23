using System.Net.WebSockets;

namespace CryptoServer.WebSockets;

public class WebSocketConnection
{
    public required WebSocket WebSocket { get; init; }

    public required string UserName { get; init; }

    public required string Token { get; init; }
}