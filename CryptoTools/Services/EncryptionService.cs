using System.IO;
using System.Security.Cryptography;

#pragma warning disable SYSLIB0022

namespace CryptoTools.Services;

public static class EncryptionService
{
    private static SymmetricAlgorithm GetAlgorithm(string name)
    {
        return name switch
        {
            "AES" => Aes.Create(),
            "DES" => DES.Create(),
            "RC2" => RC2.Create(),
            "Rijndael" => Rijndael.Create(),
            "TripleDES" => TripleDES.Create(),
            _ => throw new CryptographicException("Invalid encryption algorithm")
        };
    }

    public static byte[] EncryptData(byte[] bytes, string algorithmName)
    {
        using var algorithm = GetAlgorithm(algorithmName);
        // Set the encryption key and generate an Initialization Vector
        algorithm.GenerateKey();
        algorithm.GenerateIV();
        algorithm.Mode = CipherMode.CBC;
        using var memoryStream = new MemoryStream();
        // Save the IV at the beginning of the stream
        memoryStream.Write(algorithm.IV, 0, algorithm.IV.Length);
        // Create cryptographic stream
        var encryptor = algorithm.CreateEncryptor();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        // Encrypt the bytes
        cryptoStream.Write(bytes, 0, bytes.Length);
        return memoryStream.ToArray();
    }
}