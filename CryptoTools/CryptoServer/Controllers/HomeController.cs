using System.Security.Cryptography;
using CryptoLib.Models;
using CryptoServer.Data;
using CryptoServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CryptoServer.Controllers;

[Route("/")]
public class HomeController : Controller
{
    private readonly CryptoDbContext _dbContext;
    private readonly RSA _rsa = RSA.Create();
    private readonly RSA _rsa2 = RSA.Create(4096);

    public HomeController(CryptoDbContext dbContext)
    {
        _dbContext = dbContext;
        ConfigureRsa();
    }

    private void ConfigureRsa()
    {
        Directory.CreateDirectory("keys");
        if (Directory.GetFiles("keys").Length == 2)
        {
            ReadRsaContent(_rsa, "keys\\sign.xml");
            ReadRsaContent(_rsa2, "keys\\encrypt.xml");
        }
        else
        {
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
    [Route("/login")]
    public async Task<IActionResult> Login([FromBody] User user)
    {
        var userId = await _dbContext.Users
            .Where(u => u.UserName == user.UserName && u.PasswordHash == user.PasswordHash)
            .Select(u => u.Id)
            .FirstOrDefaultAsync();
        if (userId == 0) return Unauthorized();
        return Ok(userId);
    }

    [HttpGet]
    [Route("/chat")]
    public async Task<IActionResult> Chat()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await ChatHandler.Handle(webSocket, _dbContext);
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
        }

        return new EmptyResult();
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