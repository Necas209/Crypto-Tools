using System.Security.Cryptography;
using CryptoLib.Models;
using CryptoServer.Utils;
using Microsoft.AspNetCore.Mvc;

namespace CryptoServer.Controllers;

[Route("/")]
public class SignController : Controller
{
    private const string RsaKeys = "rsa.xml";
    private readonly RSA _rsa = RSA.Create();

    public SignController()
    {
        if (System.IO.File.Exists(RsaKeys))
        {
            using var sr = new StreamReader(RsaKeys);
            var xml = sr.ReadToEnd();
            _rsa.FromXmlString(xml);
        }
        else
        {
            using var sw = new StreamWriter(RsaKeys);
            sw.Write(_rsa.ToXmlString(true));
        }
    }

    [HttpPost]
    [Route("/sign")]
    public IActionResult Sign([FromBody] SignatureRequest request)
    {
        var token = Request.Headers["X-Access-Token"].ToString();
        if (TokenUtils.ValidateAccessToken(token) is null)
            return Unauthorized();

        var hash = Convert.FromBase64String(request.Hash);
        var signature = _rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var base64Signature = Convert.ToBase64String(signature);
        return Ok(base64Signature);
    }

    [HttpPost]
    [Route("/verify")]
    public IActionResult Verify([FromBody] VerifyRequest request)
    {
        var token = Request.Headers["X-Access-Token"].ToString();
        if (TokenUtils.ValidateAccessToken(token) is null)
            return Unauthorized();

        var hash = Convert.FromBase64String(request.Hash);
        var signature = Convert.FromBase64String(request.Signature);
        var verified = _rsa.VerifyHash(hash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return verified ? Ok() : BadRequest("Signature is invalid.");
    }
}