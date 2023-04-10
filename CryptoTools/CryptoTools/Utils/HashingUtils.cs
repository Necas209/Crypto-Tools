using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CryptoTools.Utils;

public static class HashingUtils
{
    private static HashAlgorithm GetHashAlgorithm(string name)
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

    public static string ToHexString(byte[] bytes)
    {
        return Convert.ToHexString(bytes).ToLower();
    }

    public static byte[] HashFile(string file, string algorithm)
    {
        using var hashAlgorithm = GetHashAlgorithm(algorithm);
        using var stream = File.OpenRead(file);
        return hashAlgorithm.ComputeHash(stream);
    }

    public static byte[] Hash(string text, string algorithm)
    {
        using var hashAlgorithm = GetHashAlgorithm(algorithm);
        var textBytes = Encoding.UTF8.GetBytes(text);
        return hashAlgorithm.ComputeHash(textBytes);
    }
}