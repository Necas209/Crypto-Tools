using CryptoLib.Models;
using CryptoServer.Data;
using CryptoServer.Utils;
using CryptoServer.WebSockets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CryptoServer.Controllers;

[Route("/")]
public class UserController : Controller
{
    private readonly ChatHandler _chatHandler;
    private readonly CryptoDbContext _context;

    public UserController(CryptoDbContext context, ChatHandler chatHandler)
    {
        _context = context;
        _chatHandler = chatHandler;
    }

    [HttpPost]
    [Route("/login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .Where(x => x.UserName == request.UserName)
            .SingleOrDefaultAsync();
        if (user == null) return NotFound();
        var hash = PasswordUtils.GenerateHash(request.Password, user.PasswordSalt);
        if (user.PasswordHash != hash) return Unauthorized();
        if (_chatHandler.UserIsOn(user.UserName)) return Conflict();
        // Generate a random access token
        var accessToken = TokenUtils.GenerateAccessToken(user.UserName);
        return Ok(new LoginResponse { AccessToken = accessToken });
    }

    [HttpPost]
    [Route("/register")]
    public async Task<IActionResult> Register([FromBody] LoginRequest request)
    {
        // Check if the username is already taken
        var userExists = await _context.Users.AnyAsync(x => x.UserName == request.UserName);
        if (userExists) return Conflict();
        // Hash the password using SHA256 and store the hash and salt
        var (hash, salt) = PasswordUtils.HashPassword(request.Password);
        var user = new User
        {
            UserName = request.UserName,
            PasswordHash = hash,
            PasswordSalt = salt
        };
        _context.Users.Add(user); // Add the user to the database
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    [Route("/is-logged-in")]
    public IActionResult IsLoggedIn()
    {
        var accessToken = Request.Headers["X-Access-Token"].ToString();
        var userName = TokenUtils.ValidateAccessToken(accessToken);
        if (userName == null) return Unauthorized();
        return Ok();
    }
}