using ChatServer.Data;
using ChatServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Controllers;

[Route("/")]
public class HomeController : Controller
{
    private readonly ChatDbContext _dbContext;

    public HomeController(ChatDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [Route("/login")]
    public async Task<IActionResult> Login(string username, string passwordHash)
    {
        var userId = await _dbContext.Users
            .Where(u => u.UserName == username && u.PasswordHash == passwordHash)
            .Select(u => u.Id)
            .FirstOrDefaultAsync();
        if (userId == 0) return Unauthorized();
        return Ok(userId);
    }

    [HttpGet]
    [Route("/chat")]
    public async Task<IActionResult> Chat()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await ChatHandler.Handle(webSocket, _dbContext);
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
        }

        return new EmptyResult();
    }
}