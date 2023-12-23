namespace CryptoLib.Models;

public class ChatMessage
{
    public required string UserName { get; set; }

    public required byte[] Message { get; init; }

    public byte[] Hmac { get; init; } = Array.Empty<byte>();

    public byte[] SymmetricKey { get; set; } = Array.Empty<byte>();

    public byte[] HmacKey { get; set; } = Array.Empty<byte>();
}