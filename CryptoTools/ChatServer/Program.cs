using ChatServer;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseWebSockets();
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/chat")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await ChatHandler.Handle(webSocket);
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }
    else
    {
        await next();
    }
});

// Answer login requests
app.MapGet("/login", (string username, string password) =>
{
    // TODO: Check credentials against a database
    if (username == "admin" && password == "admin") return Results.Ok();

    return Results.Unauthorized();
});

app.Run();