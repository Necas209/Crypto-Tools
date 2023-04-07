using System.Security.Cryptography;
using CryptoLib.Models;
using Microsoft.AspNetCore.Mvc;

namespace CryptoServer.Controllers;

[Route("/")]
public class SignController : Controller
{
    private readonly RSA _rsa = RSA.Create();
    private readonly RSA _rsa2 = RSA.Create(4096);

    public SignController()
    {
        // Create keys folder if it doesn't exist
        Directory.CreateDirectory("keys");
        // Check if keys exist
        if (Directory.GetFiles("keys").Length == 2)
        {
            // Read keys from files
            ReadRsaContent(_rsa, "keys\\sign.xml");
            ReadRsaContent(_rsa2, "keys\\encrypt.xml");
        }
        else
        {
            // Create keys and save them to files
            SaveRsaContent(_rsa, "keys\\sign.xml");
            SaveRsaContent(_rsa2, "keys\\encrypt.xml");
        }
    }

    private static void ReadRsaContent(AsymmetricAlgorithm rsa, string fileName)
    {
        using var sr = new StreamReader(fileName);
        var xml = sr.ReadToEnd();
        rsa.FromXmlString(xml);
    }

    private static void SaveRsaContent(AsymmetricAlgorithm rsa, string fileName)
    {
        using var sw = new StreamWriter(fileName);
        sw.Write(rsa.ToXmlString(true));
    }

    [HttpPost]
    [Route("/sign")]
    public IActionResult SignData([FromBody] SignatureRequest request)
    {
        var data = Convert.FromBase64String(request.Data);
        var signature = _rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        // Encrypt signature with private key
        var encryptedSignature = _rsa2.Encrypt(signature, RSAEncryptionPadding.Pkcs1);
        // Encrypt data with private key
        var encryptedData = _rsa2.Encrypt(data, RSAEncryptionPadding.Pkcs1);
        // Combine encrypted data and encrypted signature
        var buffer = new byte[encryptedData.Length + encryptedSignature.Length];
        encryptedData.CopyTo(buffer, 0);
        encryptedSignature.CopyTo(buffer, encryptedData.Length);
        var base64 = Convert.ToBase64String(buffer);
        return Ok(base64);
    }

    [HttpPost]
    [Route("/verify")]
    public IActionResult VerifySignature([FromBody] SignatureRequest request)
    {
        var buffer = Convert.FromBase64String(request.Data);
        var data = buffer[..512];
        var signature = buffer[512..];
        // Decrypt signature with public key
        var decryptedSignature = _rsa2.Decrypt(signature, RSAEncryptionPadding.Pkcs1);
        var decryptedData = _rsa2.Decrypt(data, RSAEncryptionPadding.Pkcs1);
        // Verify signature
        var isVerified = _rsa.VerifyData(decryptedData, decryptedSignature, HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        return Ok(isVerified);
    }
}