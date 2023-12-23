namespace CryptoLib.Models;

public class VerifyRequest
{
    public required string Hash { get; init; }

    public required string Signature { get; init; }
}