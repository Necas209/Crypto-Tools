using System.Security.Cryptography;
using CryptoLib;
using CryptoServer.Utils;
using Microsoft.AspNetCore.Mvc;

namespace CryptoServer.Endpoints;

public static class SignModule
{
    public static void MapSignEndpoints(this IEndpointRouteBuilder routes)
    {
        const string rsaKeys = "rsa.xml";
        var rsa = RSA.Create();

        if (File.Exists(rsaKeys))
        {
            using var sr = new StreamReader(rsaKeys);
            var xml = sr.ReadToEnd();
            rsa.FromXmlString(xml);
        }
        else
        {
            using var sw = new StreamWriter(rsaKeys);
            sw.Write(rsa.ToXmlString(true));
        }

        routes.MapPost("/sign", ([FromHeader(Name = "X-Access-Token")] string accessToken, SignatureRequest request) =>
        {
            if (TokenUtils.ValidateAccessToken(accessToken) is null)
                return Results.Unauthorized();

            var hash = Convert.FromBase64String(request.Hash);
            var signature = rsa.SignHash(hash, HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
            var base64Signature = Convert.ToBase64String(signature);
            return Results.Ok(base64Signature);
        });

        routes.MapPost("/verify", ([FromHeader(Name = "X-Access-Token")] string accessToken, VerifyRequest request) =>
        {
            if (TokenUtils.ValidateAccessToken(accessToken) is null)
                return Results.Unauthorized();

            var hash = Convert.FromBase64String(request.Hash);
            var signature = Convert.FromBase64String(request.Signature);
            var verified = rsa.VerifyHash(hash, signature,
                HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return verified
                ? Results.Ok()
                : Results.BadRequest("Signature is invalid.");
        });
    }
}