using System.Net.WebSockets;

namespace ChatServer.Services;

public class WebSocketConnection
{
    public string UserName { get; set; } = null!;
    public string ConnectionId { get; init; } = null!;
    public WebSocket WebSocket { get; init; } = null!;
}