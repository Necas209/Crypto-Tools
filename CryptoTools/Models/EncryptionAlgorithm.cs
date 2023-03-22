using System.ComponentModel.DataAnnotations;

namespace CryptoTools.Models;

public class EncryptionAlgorithm
{
    [Key] public int Id { get; set; }

    [Required] public string Name { get; set; } = string.Empty;

    [Required] public EncryptionType EncryptionType { get; set; }
}

public enum EncryptionType
{
  Symmetric,
  Asymmetric
}