using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media;

namespace CryptoTools.ViewModels;

public class SignaturePageViewModel
{
    public delegate void DisplayMessageDelegate(string message, Color color);

    public event DisplayMessageDelegate? DisplayMessage;

    private readonly RSA _rsa = RSA.Create();
    private readonly SHA256 _sha256 = SHA256.Create();
    private static Random random = new();

    private static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static byte[] CombineThreeByteArray(byte[] first, byte[] second, byte[] third)
    {
        var ret = new byte[first.Length + second.Length + third.Length];
        Buffer.BlockCopy(first, 0, ret, 0, first.Length);
        Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
        Buffer.BlockCopy(third, 0, ret, first.Length + second.Length, third.Length);
        return ret;
    }

    public void SignFiles(IEnumerable<string> dialogFileNames)
    {
        foreach (var fileName in dialogFileNames)
        {
            SignFile(fileName);
        }
    }

    private void SignFile(string fileName)
    {
        var randomString = RandomString(256);
        var randomBytes = Encoding.ASCII.GetBytes(randomString);
        var file = File.ReadAllBytes(fileName);
        var hash = _sha256.ComputeHash(randomBytes);
        var signature = _rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var signatureFileBytes = CombineThreeByteArray(randomBytes, signature, file);
        var signatureFile = fileName + ".sign";
        File.WriteAllBytes(signatureFile, signatureFileBytes);
    }

    public void VerificationSign(string file)
    {
        // Verify the signature of the file
        var signatureFileBytes = File.ReadAllBytes(file);

        // Get the random string from the signature
        var randomString = signatureFileBytes.Take(256).ToArray();

        // Get the signature from the signature
        var signature = signatureFileBytes.Skip(256).Take(256).ToArray();
        // Get the file from the signature
        var fileBytes = signatureFileBytes.Skip(256 + signature.Length).ToArray();

        // Verify the signature
        var hash = _sha256.ComputeHash(randomString);

        var isSignatureValid = _rsa.VerifyHash(hash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        if (isSignatureValid)
            DisplayMessage?.Invoke("Signature is valid.", Colors.Green);
        else
            DisplayMessage?.Invoke("Signature is invalid.", Colors.Coral);
    }
}