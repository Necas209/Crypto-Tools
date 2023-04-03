using System.Security.Cryptography;
using System.Text;
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
    private readonly RSA _rsa;

    public HomeController(CryptoDbContext dbContext)
    {
        _dbContext = dbContext;
        if (Directory.Exists("keys"))
        {
            using var sr = new StreamReader("keys\\private.xml");
            var xml = sr.ReadToEnd();
            _rsa = RSA.Create();
            _rsa.FromXmlString(xml);
        }
        else
        {
            _rsa = RSA.Create(4096);
            Directory.CreateDirectory("keys");
            using var sw = new StreamWriter("keys\\private.xml");
            sw.Write(_rsa.ToXmlString(true));
        }
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
    public async Task<IActionResult> Sign([FromBody] SignRequest request)
    {
        var data = Encoding.ASCII.GetBytes(request.Data);
        var signature = _rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        // Encrypt signature with private key
        var encryptedSignature = _rsa.Encrypt(signature, RSAEncryptionPadding.Pkcs1);
        // Encrypt data with private key
        var encryptedData = _rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
        // Combine encrypted data and encrypted signature
        using var ms = new MemoryStream();
        await using var bw = new BinaryWriter(ms);
        bw.Write(encryptedData.Length);
        bw.Write(encryptedData);
        bw.Write(encryptedSignature.Length);
        bw.Write(encryptedSignature);
        var buffer = ms.ToArray();
        // Return encrypted data and encrypted signature
        return Ok(buffer);
    }

    [HttpPost]
    [Route("/verify")]
    public async Task<IActionResult> Verify([FromBody] SignRequest request)
    {
        var data = Encoding.ASCII.GetBytes(request.Data);
        var signature = Encoding.ASCII.GetBytes(request.Signature);
        // Decrypt signature with public key
        var decryptedSignature = _rsa.Decrypt(signature, RSAEncryptionPadding.Pkcs1);
        var decryptedData = _rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
        var isVerified = _rsa.VerifyData(decryptedData, decryptedSignature, HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        return await Task.FromResult(Ok(isVerified));
    }
}