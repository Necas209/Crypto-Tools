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
        using var encryptor = aes.CreateEncryptor();
        var encryptedData = encryptor.TransformFinalBlock(data, 0, data.Length);
        return aes.IV.Concat(encryptedData).ToArray();
    }

    public static byte[] Decrypt(byte[] data, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        var iv = data[..16];
        var encryptedData = data[16..];
        using var decrypt = aes.CreateDecryptor(key, iv);
        return decrypt.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
    }
}