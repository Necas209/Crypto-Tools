using CryptoServer.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<CryptoDbContext>(
    options => options.UseSqlServer(
        Environment.GetEnvironmentVariable("SQLCONNSTR_CryptoDb")
    )
);

var app = builder.Build();

app.UseWebSockets();
app.UseRouting();
app.MapControllers();

app.Run();