namespace CryptoLib.Models;

public class HashEntry
{
    public int UserId { get; set; }

    public string Hash { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public int HashingAlgorithmId { get; set; }

    public User? User { get; set; }

    public HashingAlgorithm? HashingAlgorithm { get; set; }
}