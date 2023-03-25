using ChatServer.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Data;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
}