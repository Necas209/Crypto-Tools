using System.Security.Cryptography;

namespace CryptoLib.Services;

public static class SignatureService
{
    public static byte[] SignData(byte[] data, RSA rsa)
    {
        return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    public static bool VerifySignature(byte[] data, byte[] signature, RSA rsa)
    {
        return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
}