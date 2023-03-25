using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using CryptoLib.Models;
using CryptoServer.Data;

namespace CryptoServer.Services;

public static class ChatHandler
{
    private static readonly WebSocketManager WebSocketManager = new();

    public static async Task Handle(WebSocket webSocket, CryptoDbContext dbContext)
    {
        var isFirstMessage = true;
        var webSocketConnection = WebSocketManager.AddWebSocket(webSocket);
        await SendWelcomeMessage(webSocketConnection);
        try
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var chatMessage = JsonSerializer.Deserialize<ChatMessage>(message);
                var user = await dbContext.Users.FindAsync(chatMessage!.UserId);
                if (isFirstMessage)
                {
                    webSocketConnection.UserName = user!.UserName;
                    isFirstMessage = false;
                    await WebSocketManager.BroadcastMessage($"{user.UserName} has joined the chat.");
                }
                message = $"{user!.UserName}: {chatMessage.Message}";
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
            WebSocketManager.RemoveWebSocket(webSocketConnection);
        }
    }

    private static async Task SendWelcomeMessage(WebSocketConnection webSocketConnection)
    {
        const string welcomeMessage = "Welcome to the chat room!";
        await SendMessage(welcomeMessage, webSocketConnection.WebSocket);
    }

    private static async Task SendMessage(string message, WebSocket webSocket)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
            CancellationToken.None);
    }
}