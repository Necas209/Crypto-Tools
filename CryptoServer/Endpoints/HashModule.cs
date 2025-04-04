using CryptoLib.Models;
using CryptoServer.Data;
using CryptoServer.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CryptoServer.Endpoints;

public static class HashModule
{
    public static void MapHashEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("hash", async (CryptoDbContext context) =>
        {
            var algorithms = await context.HashingAlgorithms.ToListAsync();
            return Results.Ok(algorithms);
        });

        app.MapGet("hash/{fileName}", async ([FromHeader(Name = "X-Access-Token")] string accessToken,
            CryptoDbContext dbContext, string fileName) =>
        {
            var userName = TokenUtils.ValidateAccessToken(accessToken);
            if (userName == null) return Results.Unauthorized();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null) return Results.Unauthorized();
            var userId = user.Id;
            var entry = await dbContext.HashEntries.FindAsync(userId, fileName);
            return entry == null ? Results.NotFound() : Results.Ok(entry);
        });

        app.MapPost("hash",
            async ([FromHeader(Name = "X-Access-Token")] string accessToken, CryptoDbContext dbContext,
                HashEntry entry) =>
            {
                var userName = TokenUtils.ValidateAccessToken(accessToken);
                if (userName is null)
                    return Results.Unauthorized();

                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserName == userName);
                if (user is null)
                    return Results.Unauthorized();

                entry.UserId = user.Id;
                var existing = await dbContext.HashEntries.FindAsync(entry.UserId, entry.FileName);
                if (existing != null)
                    dbContext.HashEntries.Remove(existing);
                dbContext.HashEntries.Add(entry);
                await dbContext.SaveChangesAsync();
                return Results.Ok();
            });
    }
}