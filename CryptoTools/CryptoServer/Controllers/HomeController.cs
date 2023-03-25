using CryptoLib.Models;
using CryptoServer.Data;
using CryptoServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CryptoServer.Controllers;

[Route("/")]
public class HomeController : Controller
{
    private readonly CryptoDbContext _dbContext;

    public HomeController(CryptoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    [Route("/login")]
    public async Task<IActionResult> Login([FromBody] User user)
    {
        var userId = await _dbContext.Users
            .Where(u => u.UserName == user.UserName && u.PasswordHash == user.PasswordHash)
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