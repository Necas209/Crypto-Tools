using System.Net.WebSockets;

namespace CryptoServer.WebSockets;

public class WebSocketConnection
{
    public string UserName { get; set; } = null!;
    public string ConnectionId { get; init; } = null!;
    public WebSocket WebSocket { get; init; } = null!;
}