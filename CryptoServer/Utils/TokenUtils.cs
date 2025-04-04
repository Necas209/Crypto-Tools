using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace CryptoServer.Utils;

public static class TokenUtils
{
    private static readonly byte[] Key = RandomNumberGenerator.GetBytes(32);

    public static string GenerateAccessToken(string userName)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, "User")
            ]),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(token);
        return accessToken;
    }

    public static bool ValidateAccessToken(string accessToken, string username)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(accessToken,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                }, out _);

            return principal.Identity is ClaimsIdentity { IsAuthenticated: true } identity && identity.Name == username;
        }
        catch
        {
            return false;
        }
    }

    public static string? ValidateAccessToken(string accessToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(accessToken,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                }, out _);
            return principal.Identity?.Name;
        }
        catch
        {
            return null;
        }
    }
}