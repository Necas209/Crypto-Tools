using System.ComponentModel.DataAnnotations;

namespace CryptoLib.Models;

public class HashingAlgorithm
{
    [Key] public int Id { get; set; }

    [Required] public string Name { get; set; } = string.Empty;
}