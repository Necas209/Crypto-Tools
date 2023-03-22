using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoTools.Models;

public class HashingAlgorithm
{
    public HashingAlgorithm()
    {
        Name = string.Empty;
        HashEntries = new HashSet<HashEntry>();
    }

    [Key] public int Id { get; set; }

    [Required] public string Name { get; set; }

    [InverseProperty("HashingAlgorithm")] public ICollection<HashEntry> HashEntries { get; set; }
}