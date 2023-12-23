using CryptoLib.Models;
using CryptoServer.Data;
using CryptoServer.Utils;
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

app.MapGet("hash/{fileName}", async (HttpContext context, CryptoDbContext dbContext, string fileName) =>
{
    var token = context.Request.Headers["X-Access-Token"].ToString();
    var userName = TokenUtils.ValidateAccessToken(token);
    if (userName == null) return Results.Unauthorized();
    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.UserName == userName);
    if (user == null) return Results.Unauthorized();
    var userId = user.Id;
    var entry = await dbContext.HashEntries.FindAsync(userId, fileName);
    return entry == null ? Results.NotFound() : Results.Ok(entry);
});

app.MapPost("hash", async (HttpContext context, CryptoDbContext dbContext, HashEntry entry) =>
{
    var token = context.Request.Headers["X-Access-Token"].ToString();
    var userName = TokenUtils.ValidateAccessToken(token);
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

app.MapGet("encrypt", async (CryptoDbContext context) =>
{
    var algorithms = await context.EncryptionAlgorithms.ToListAsync();
    return Results.Ok(algorithms);
});

app.Run();