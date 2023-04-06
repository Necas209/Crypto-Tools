using CryptoLib.Models;
using CryptoServer.Data;
using CryptoServer.WebSockets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CryptoServer.Controllers;

[Route("/")]
public class UserController : Controller
{
    private readonly CryptoDbContext _context;
    private readonly ChatHandler _chatHandler;

    public UserController(CryptoDbContext context, ChatHandler chatHandler)
    {
        _context = context;
        _chatHandler = chatHandler;
    }

    [HttpPost]
    [Route("/login")]
    public async Task<IActionResult> Login([FromBody] User user)
    {
        var userFromDb = await _context.Users
            .Where(x => x.UserName == user.UserName)
            .SingleOrDefaultAsync();
        if (userFromDb == null) return NotFound();
        if (userFromDb.PasswordHash != user.PasswordHash) return Unauthorized();
        if (_chatHandler.UserIsOn(userFromDb.UserName)) return Conflict();
        return Ok(userFromDb.Id);
    }

    [HttpPost]
    [Route("/register")]
    public async Task<User?> Register([FromBody] User user)
    {
        var userExists = await _context.Users.AnyAsync(x => x.UserName == user.UserName);
        if (userExists) return null;
        _context.Users.Add(user); // Add the user to the database
        await _context.SaveChangesAsync();
        return user;
    }
}