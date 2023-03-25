namespace CryptoServer.Models;

public class ChatMessage
{
    public int UserId { get; init; }
    public string Message { get; init; } = null!;
}