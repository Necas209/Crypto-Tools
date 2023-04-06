using CryptoLib.Models;
using CryptoServer.Data;
using CryptoServer.Utils;
using CryptoServer.WebSockets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CryptoServer.Controllers;

[Route("/")]
public class ChatController : Controller
{
    private readonly ChatHandler _chatHandler;
    private readonly CryptoDbContext _context;

    public ChatController(ChatHandler chatHandler, CryptoDbContext context)
    {
        _chatHandler = chatHandler;
        _context = context;
    }

    [HttpPost]
    [Route("/exchange")]
    public async Task<IActionResult> ExchangeKeys([FromBody] ExchangeRequest request)
    {
        var token = HttpContext.Request.Headers["X-Access-Token"].ToString();
        var userName = TokenUtils.ValidateAccessToken(token);
        if (userName == null) return Unauthorized();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
        if (user == null) return Unauthorized();
        // Add the user to the chat handler
        _chatHandler.AddUser(userName, request.PublicKey);
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
            var token = HttpContext.Request.Headers["X-Access-Token"].ToString();
            var username = TokenUtils.ValidateAccessToken(token);
            if (username == null) return Unauthorized();
            await _chatHandler.Handle(webSocket, username, token);
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
        }

        return new EmptyResult();
    }
}