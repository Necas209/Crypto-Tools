using System.ComponentModel.DataAnnotations;

namespace CryptoLib.Models;

public class HashEntry
{
    public int UserId { get; set; }

    public required int HashingAlgorithmId { get; init; }

    [Required] public required string Hash { get; init; }

    [Required] public required string FileName { get; init; }
}