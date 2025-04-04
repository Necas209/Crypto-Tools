using CryptoServer.Data;
using Microsoft.EntityFrameworkCore;

namespace CryptoServer.Endpoints;

public static class EncryptModule
{
    public static void MapEncryptEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("encrypt", async (CryptoDbContext context) =>
        {
            var algorithms = await context.EncryptionAlgorithms.ToListAsync();
            return Results.Ok(algorithms);
        });
    }
}