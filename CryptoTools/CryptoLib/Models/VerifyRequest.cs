namespace CryptoLib.Models;

public class VerifyRequest
{
    public string Hash { get; init; } = string.Empty;

    public string Signature { get; init; } = string.Empty;
}