namespace CryptoTools.Models;

public sealed class HashEntry
{
    public int Id { get; set; }
    public string Hash { get; set; } = "";
    public string FileName { get; set; } = "";
    public int AlgorithmId { get; set; }
    public HashingAlgorithm? HashingAlgorithm { get; set; }
}