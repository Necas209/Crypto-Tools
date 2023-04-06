using CryptoLib.Models;
using CryptoServer.WebSockets;
using Microsoft.AspNetCore.Mvc;

namespace CryptoServer.Controllers;

[Route("/")]
public class ChatController : Controller
{
    private readonly ChatHandler _chatHandler;

    public ChatController(ChatHandler chatHandler)
    {
        _chatHandler = chatHandler;
    }

    [HttpPost]
    [Route("/keys")]
    public IActionResult ExchangeKeys([FromBody] ChatRequest request)
    {
        _chatHandler.AddUser(request.UserName, request.PublicKey);
        var serverPublicKey = _chatHandler.GetPublicKey();
        var base64PublicKey = Convert.ToBase64String(serverPublicKey);
        return Ok(base64PublicKey);
    }

    [HttpGet]
    [Route("/chat")]
    public async Task<IActionResult> Chat()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            // Get the client's username from the WebSocket opening handshake
            var username = HttpContext.Request.Headers["X-Username"].ToString();
            await _chatHandler.Handle(webSocket, username);
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
        }

        return new EmptyResult();
    }
}