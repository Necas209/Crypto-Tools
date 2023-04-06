using System.Linq;
using System.Security.Cryptography;

namespace CryptoTools.Utils;

public static class AesUtils
{
    public static byte[] Encrypt(byte[] data, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();
        var iv = aes.IV;
        using var encryptor = aes.CreateEncryptor();
        var encryptedData = encryptor.TransformFinalBlock(data, 0, data.Length);
        return iv.Concat(encryptedData).ToArray();
    }

    public static byte[] Decrypt(byte[] data, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        var iv = data.Take(16).ToArray();
        var encryptedData = data.Skip(16).ToArray();
        using var decrypt = aes.CreateDecryptor(key, iv);
        return decrypt.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
    }
}