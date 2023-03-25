using CryptoServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoServer.Data;

public class CryptoDbContext : DbContext
{
    public CryptoDbContext(DbContextOptions<CryptoDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
}