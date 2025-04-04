using CryptoLib;
using CryptoServer.Utils;
using CryptoServer.WebSockets;
using Microsoft.AspNetCore.Mvc;

namespace CryptoServer.Endpoints;

public static class ChatModule
{
    public static void MapChatEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/exchange",
            ([FromHeader(Name = "X-Access-Token")] string accessToken, ChatHandler chatHandler,
                ExchangeRequest request) =>
            {
                var userName = TokenUtils.ValidateAccessToken(accessToken);
                if (userName is null)
                    return Results.Unauthorized();

                chatHandler.AddUser(userName, request.PublicKey);
                var serverPublicKey = chatHandler.GetPublicKey();
                var base64PublicKey = Convert.ToBase64String(serverPublicKey);
                return Results.Ok(base64PublicKey);
            });

        builder.MapGet("/chat",
            async (HttpContext context, CancellationToken token,
                [FromHeader(Name = "X-Access-Token")] string accessToken,
                ChatHandler chatHandler) =>
            {
                if (!context.WebSockets.IsWebSocketRequest)
                    return Results.BadRequest();

                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var username = TokenUtils.ValidateAccessToken(accessToken);
                if (username is null)
                    return Results.Unauthorized();

                await chatHandler.Handle(webSocket, username, accessToken, token);
                return Results.Ok();
            });
    }
}