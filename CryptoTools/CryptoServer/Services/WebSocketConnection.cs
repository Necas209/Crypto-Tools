using System.Net.WebSockets;

namespace CryptoServer.Services;

public class WebSocketConnection
{
    public string UserName { get; set; } = null!;
    public string ConnectionId { get; init; } = null!;
    public WebSocket WebSocket { get; init; } = null!;
}