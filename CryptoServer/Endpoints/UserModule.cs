using CryptoLib;
using CryptoLib.Models;
using CryptoServer.Data;
using CryptoServer.Utils;
using CryptoServer.WebSockets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CryptoServer.Endpoints;

public static class UserModule
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("register", async (CryptoDbContext dbContext, RegisterRequest request) =>
        {
            var userExists = await dbContext.Users.AnyAsync(x => x.UserName == request.UserName);
            if (userExists)
                return Results.Conflict();

            var (hash, salt) = PasswordUtils.HashPassword(request.Password);
            var user = new User
            {
                UserName = request.UserName,
                PasswordHash = hash,
                PasswordSalt = salt
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
            return Results.Ok();
        });

        builder.MapPost("/login",
            async (HttpContext _, CryptoDbContext dbContext, ChatHandler chatHandler, LoginRequest request) =>
            {
                var user = await dbContext.Users
                    .Where(x => x.UserName == request.UserName)
                    .SingleOrDefaultAsync();
                if (user is null)
                    return Results.NotFound();

                var hash = PasswordUtils.GenerateHash(request.Password, user.PasswordSalt);
                if (user.PasswordHash != hash)
                    return Results.Unauthorized();

                if (chatHandler.UserIsOnline(user.UserName))
                    return Results.Conflict();

                var accessToken = TokenUtils.GenerateAccessToken(user.UserName);
                return Results.Ok(new LoginResponse(accessToken));
            });

        builder.MapGet("/is-logged-in", ([FromHeader(Name = "X-Access-Token")] string accessToken) =>
        {
            var userName = TokenUtils.ValidateAccessToken(accessToken);
            return userName is null ? Results.Unauthorized() : Results.Ok();
        });
    }
}