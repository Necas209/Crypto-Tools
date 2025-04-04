namespace CryptoLib.Models;

public sealed record ChatMessage(string UserName, byte[] Message)
{
    public ChatMessage(string userName, byte[] message, byte[] hmac, byte[] symmetricKey, byte[] hmacKey)
        : this(userName, message)
    {
        Hmac = hmac;
        SymmetricKey = symmetricKey;
        HmacKey = hmacKey;
    }

    public byte[] Hmac { get; init; } = [];

    public byte[] SymmetricKey { get; init; } = [];

    public byte[] HmacKey { get; init; } = [];
}