using System.Security.Cryptography;
using CryptoLib.Models;
using CryptoServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace CryptoServer.Controllers;

[Route("/")]
public class SignatureController : Controller
{
    private readonly RSA _rsa = RSA.Create();
    private readonly RSA _rsa2 = RSA.Create(4096);

    public SignatureController()
    {
        // Create keys folder if it doesn't exist
        Directory.CreateDirectory("keys");
        // Check if keys exist
        if (Directory.GetFiles("keys").Length == 2)
        {
            // Read keys from files
            SignatureService.ReadRsaContent(_rsa, "keys\\sign.xml");
            SignatureService.ReadRsaContent(_rsa2, "keys\\encrypt.xml");
        }
        else
        {
            // Create keys and save them to files
            SignatureService.SaveRsaContent(_rsa, "keys\\sign.xml");
            SignatureService.SaveRsaContent(_rsa2, "keys\\encrypt.xml");
        }
    }

    [HttpPost]
    [Route("/sign")]
    public async Task<IActionResult> Sign([FromBody] SignatureRequest request)
    {
        var data = Convert.FromBase64String(request.Data);
        var signature = _rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        // Encrypt signature with private key
        var encryptedSignature = _rsa2.Encrypt(signature, RSAEncryptionPadding.Pkcs1);
        // Encrypt data with private key
        var encryptedData = _rsa2.Encrypt(data, RSAEncryptionPadding.Pkcs1);
        // Combine encrypted data and encrypted signature
        using var ms = new MemoryStream();
        await using var bw = new BinaryWriter(ms);
        bw.Write(encryptedData);
        bw.Write(encryptedSignature);
        var buffer = ms.ToArray();
        var base64 = Convert.ToBase64String(buffer);
        return Ok(base64);
    }

    [HttpPost]
    [Route("/verify")]
    public IActionResult Verify([FromBody] SignatureRequest request)
    {
        var buffer = Convert.FromBase64String(request.Data);
        using var ms = new MemoryStream(buffer);
        using var br = new BinaryReader(ms);
        var data = br.ReadBytes(512);
        var signature = br.ReadBytes(512);
        // Decrypt signature with public key
        var decryptedSignature = _rsa2.Decrypt(signature, RSAEncryptionPadding.Pkcs1);
        var decryptedData = _rsa2.Decrypt(data, RSAEncryptionPadding.Pkcs1);
        var isVerified = _rsa.VerifyData(decryptedData, decryptedSignature, HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        return Ok(isVerified);
    }
}