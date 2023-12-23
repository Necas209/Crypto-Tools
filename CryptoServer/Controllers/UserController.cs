using CryptoLib.Models;
using CryptoServer.Data;
using CryptoServer.Utils;
using CryptoServer.WebSockets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CryptoServer.Controllers;

[Route("/")]
public class UserController(CryptoDbContext context, ChatHandler chatHandler) : Controller
{
    [HttpPost]
    [Route("/login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await context.Users
            .Where(x => x.UserName == request.UserName)
            .SingleOrDefaultAsync();
        if (user is null)
            return NotFound();

        var hash = PasswordUtils.GenerateHash(request.Password, user.PasswordSalt);
        if (user.PasswordHash != hash)
            return Unauthorized();

        if (chatHandler.UserIsOnline(user.UserName))
            return Conflict();

        var accessToken = TokenUtils.GenerateAccessToken(user.UserName);
        return Ok(new LoginResponse { AccessToken = accessToken });
    }

    [HttpPost]
    [Route("/register")]
    public async Task<IActionResult> Register([FromBody] LoginRequest request)
    {
        var userExists = await context.Users.AnyAsync(x => x.UserName == request.UserName);
        if (userExists)
            return Conflict();

        var (hash, salt) = PasswordUtils.HashPassword(request.Password);
        var user = new User
        {
            UserName = request.UserName,
            PasswordHash = hash,
            PasswordSalt = salt
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    [Route("/is-logged-in")]
    public IActionResult IsLoggedIn()
    {
        var accessToken = Request.Headers["X-Access-Token"].ToString();
        var userName = TokenUtils.ValidateAccessToken(accessToken);
        return userName is null ? Unauthorized() : Ok();
    }
}