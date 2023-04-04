using CryptoLib.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoServer.Data;

public class CryptoDbContext : DbContext
{
    public CryptoDbContext(DbContextOptions<CryptoDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;

    public DbSet<HashEntry> HashEntries { get; set; } = null!;

    public DbSet<HashingAlgorithm> HashingAlgorithms { get; set; } = null!;

    public DbSet<EncryptionAlgorithm> EncryptionAlgorithms { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HashEntry>()
            .HasKey(e => new { e.UserId, e.FileName });
    }
}