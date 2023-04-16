namespace CryptoLib.Models;

public class ChatMessage
{
    public string UserName { get; set; } = string.Empty;

    public byte[] Message { get; set; } = Array.Empty<byte>();

    public byte[] Hmac { get; set; } = Array.Empty<byte>();

    // The encrypted symmetric key used to encrypt the message
    public byte[] SymmetricKey { get; set; } = Array.Empty<byte>();

    // The encrypted HMAC key used to verify the integrity of the message
    public byte[] HmacKey { get; set; } = Array.Empty<byte>();
}