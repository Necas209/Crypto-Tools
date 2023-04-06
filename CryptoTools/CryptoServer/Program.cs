using CryptoLib.Models;
using CryptoServer.Data;
using CryptoServer.WebSockets;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<CryptoDbContext>(
    options => options.UseSqlServer(
        Environment.GetEnvironmentVariable("SQLCONNSTR_CryptoDb")
    )
);
builder.Services.AddSingleton<ChatHandler>();

var app = builder.Build();

app.UseWebSockets();
app.UseRouting();
app.MapControllers();

app.MapGet("hash", async (CryptoDbContext context) =>
{
    var algorithms = await context.HashingAlgorithms.ToListAsync();
    return Results.Ok(algorithms);
});

app.MapGet("hash/{userId:int}/{fileName}", async (CryptoDbContext context, int userId, string fileName) =>
{
    var entry = await context.HashEntries.FindAsync(userId, fileName);
    return entry == null ? Results.NotFound() : Results.Ok(entry);
});

app.MapPost("hash", async (CryptoDbContext context, HashEntry entry) =>
{
    var existing = await context.HashEntries.FindAsync(entry.UserId, entry.FileName);
    if (existing != null) context.HashEntries.Remove(existing);
    context.HashEntries.Add(entry);
    await context.SaveChangesAsync();
    return Results.Ok();
});

app.MapGet("encrypt", async (CryptoDbContext context) =>
{
    var algorithms = await context.EncryptionAlgorithms.ToListAsync();
    return Results.Ok(algorithms);
});

app.Run();