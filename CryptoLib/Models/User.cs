using System.ComponentModel.DataAnnotations;

namespace CryptoLib.Models;

public class User
{
    [Key] public int Id { get; set; }

    [Required] public string UserName { get; init; } = string.Empty;

    [Required] public string PasswordHash { get; init; } = string.Empty;

    [Required] public string PasswordSalt { get; init; } = string.Empty;
}