using CryptoLib.Models;
using CryptoServer.Utils;
using CryptoServer.WebSockets;
using Microsoft.AspNetCore.Mvc;

namespace CryptoServer.Controllers;

[Route("/")]
public class ChatController(ChatHandler chatHandler) : Controller
{
    [HttpPost]
    [Route("/exchange")]
    public IActionResult ExchangeKeys([FromBody] ExchangeRequest request)
    {
        var token = HttpContext.Request.Headers["X-Access-Token"].ToString();
        var userName = TokenUtils.ValidateAccessToken(token);
        if (userName is null)
            return Unauthorized();

        chatHandler.AddUser(userName, request.PublicKey);
        var serverPublicKey = chatHandler.GetPublicKey();
        var base64PublicKey = Convert.ToBase64String(serverPublicKey);
        return Ok(base64PublicKey);
    }

    [HttpGet]
    [Route("/chat")]
    public async Task<IActionResult> Chat()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return new EmptyResult();
        }

        var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var token = HttpContext.Request.Headers["X-Access-Token"].ToString();
        var username = TokenUtils.ValidateAccessToken(token);
        if (username is null)
            return Unauthorized();

        await chatHandler.Handle(webSocket, username, token);
        return new EmptyResult();
    }
}