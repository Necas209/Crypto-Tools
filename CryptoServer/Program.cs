using CryptoServer.Data;
using CryptoServer.Endpoints;
using CryptoServer.WebSockets;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CryptoDbContext>(
    options => options.UseSqlServer(
        Environment.GetEnvironmentVariable("SQLCONNSTR_CryptoDb")
    )
);
builder.Services.AddSingleton<ChatHandler>();

var app = builder.Build();

app.UseWebSockets();
app.UseHttpsRedirection();

app.MapUserEndpoints();
app.MapHashEndpoints();
app.MapSignEndpoints();
app.MapEncryptEndpoints();
app.MapChatEndpoints();

app.Run();
