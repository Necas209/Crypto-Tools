using System.ComponentModel.DataAnnotations;

namespace CryptoLib.Models;

public class HashingAlgorithm
{
    [Key] public int Id { get; init; }

    [Required] public required string Name { get; init; }
}