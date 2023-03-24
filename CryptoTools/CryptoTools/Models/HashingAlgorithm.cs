using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoTools.Models;

public class HashingAlgorithm
{
    [Key] public int Id { get; set; }

    [Required] public string Name { get; set; } = string.Empty;

    [InverseProperty("HashingAlgorithm")] public ICollection<HashEntry>? HashEntries { get; set; }
}