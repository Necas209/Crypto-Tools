using System.ComponentModel.DataAnnotations;

namespace CryptoLib.Models;

public class User
{
    [Key] public int Id { get; init; }

    [Required] public required string UserName { get; init; }

    [Required] public required string PasswordHash { get; init; }

    [Required] public required string PasswordSalt { get; init; }
}