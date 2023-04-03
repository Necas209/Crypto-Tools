namespace CryptoLib.Models;

public class SignRequest
{
    public string Data { get; init; } = string.Empty;

    public string Signature { get; init; } = string.Empty;
}