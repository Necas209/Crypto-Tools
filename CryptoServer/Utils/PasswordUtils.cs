using System.Security.Cryptography;
using System.Text;

namespace CryptoServer.Utils;

public static class PasswordUtils
{
    public static (string Hash, string Salt) HashPassword(string password)
    {
        var salt = GenerateSalt();
        var hash = GenerateHash(password, salt);
        return (hash, salt);
    }

    private static string GenerateSalt(int length = 16)
    {
        var salt = RandomNumberGenerator.GetBytes(length);
        return Convert.ToBase64String(salt);
    }

    public static string GenerateHash(string password, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var combinedBytes = new byte[saltBytes.Length + passwordBytes.Length];
        Buffer.BlockCopy(saltBytes, 0, combinedBytes, 0, saltBytes.Length);
        Buffer.BlockCopy(passwordBytes, 0, combinedBytes, saltBytes.Length, passwordBytes.Length);
        var hashBytes = SHA256.HashData(combinedBytes);
        return Convert.ToBase64String(hashBytes);
    }
}