using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CryptoTools.Services;

public class EncryptService
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

    public static byte[] Encrypt(byte[] bytes, string algorithm)
    {
        using var encryptor = GetAlgorithm(algorithm);
        encryptor.GenerateKey();
        //encryptor.GenerateIV();
        encryptor.Mode = CipherMode.ECB;
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write);
        cs.Write(bytes, 0, bytes.Length);
        return ms.ToArray();
    }

    public static byte[] EncryptImage(byte[] imageBytes, string algorithmName)
    {
        using var algorithm = GetAlgorithm(algorithmName);
        // Set the encryption key and generate an Initialization Vector
        algorithm.GenerateKey();
        algorithm.GenerateIV();
        algorithm.Mode = CipherMode.CBC;

        using var memoryStream = new MemoryStream();
        // Save the IV at the beginning of the stream
        memoryStream.Write(algorithm.IV, 0, algorithm.IV.Length);

        using (var cryptoStream =
               new CryptoStream(memoryStream, algorithm.CreateEncryptor(), CryptoStreamMode.Write))
        {
            // Encrypt the image bytes
            cryptoStream.Write(imageBytes, 0, imageBytes.Length);
        }

        return memoryStream.ToArray();
    }
}