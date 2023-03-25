using ChatServer.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Data;

public class CryptoDbContext : DbContext
{
    public CryptoDbContext(DbContextOptions<CryptoDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
}