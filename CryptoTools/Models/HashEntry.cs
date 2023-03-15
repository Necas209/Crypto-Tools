using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoTools.Models;

public sealed class HashEntry
{
    [Key] public int Id { get; set; }

    [Required] public string Hash { get; set; } = "";

    [Required] public string FileName { get; set; } = "";

    [ForeignKey("HashingAlgorithm")]
    [Required]
    public int HashingAlgorithmId { get; set; }

    [InverseProperty("HashEntries")]
    public HashingAlgorithm HashingAlgorithm { get; set; } = null!;
}