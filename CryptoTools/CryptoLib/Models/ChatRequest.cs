namespace CryptoLib.Models;

public class ChatRequest
{
    public string UserName { get; init; } = string.Empty;

    public byte[] PublicKey { get; init; } = Array.Empty<byte>();
}