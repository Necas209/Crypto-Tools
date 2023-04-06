using System.Security.Cryptography;

namespace CryptoTools.Utils;

public static class HmacUtils
{
    public static byte[] GenerateHmacKey(int keyLength = 32)
    {
        using var rng = RandomNumberGenerator.Create();
        var key = new byte[keyLength];
        rng.GetBytes(key);
        return key;
    }

    public static byte[] ComputeHmac(byte[] data, byte[] key)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(data);
    }
}