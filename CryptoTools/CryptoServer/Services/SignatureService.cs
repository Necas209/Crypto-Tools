using System.Security.Cryptography;

namespace CryptoServer.Services;

public static class SignatureService
{
    public static void ReadRsaContent(AsymmetricAlgorithm rsa, string fileName)
    {
        using var sr = new StreamReader(fileName);
        var xml = sr.ReadToEnd();
        rsa.FromXmlString(xml);
    }

    public static void SaveRsaContent(AsymmetricAlgorithm rsa, string fileName)
    {
        using var sw = new StreamWriter(fileName);
        sw.Write(rsa.ToXmlString(true));
    }
}