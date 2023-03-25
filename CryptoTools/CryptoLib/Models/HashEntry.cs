using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoLib.Models;

public sealed class HashEntry
{
    [Key] public int Id { get; set; }

    [Required] public string Hash { get; set; } = string.Empty;

    [Required] public string FileName { get; set; } = string.Empty;

    [ForeignKey("HashingAlgorithm")]
    [Required]
    public int HashingAlgorithmId { get; set; }

    [InverseProperty("HashEntries")] public HashingAlgorithm? HashingAlgorithm { get; set; }
}