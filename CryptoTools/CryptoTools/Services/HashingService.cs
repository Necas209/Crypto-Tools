using System;
using System.IO;
using System.Security.Cryptography;

namespace CryptoTools.Services;

public static class HashingService
{
    public static HashAlgorithm GetHashAlgorithm(string name)
    {
        return name switch
        {
            "MD5" => MD5.Create(),
            "SHA1" => SHA1.Create(),
            "SHA256" => SHA256.Create(),
            "SHA384" => SHA384.Create(),
            "SHA512" => SHA512.Create(),
            _ => throw new CryptographicException("Invalid hashing algorithm")
        };
    }

    public static string GetFileHash(string file, string algorithm)
    {
        using var hashAlgorithm = GetHashAlgorithm(algorithm);
        using var stream = File.OpenRead(file);
        var hash = hashAlgorithm.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    public static string GetHash(string text, string algorithmName)
    {
        using var hashAlgorithm = GetHashAlgorithm(algorithmName);
        var hash = hashAlgorithm.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}