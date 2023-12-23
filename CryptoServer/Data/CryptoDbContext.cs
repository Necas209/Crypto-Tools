using CryptoLib.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoServer.Data;

public class CryptoDbContext(DbContextOptions<CryptoDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; init; } = null!;

    public DbSet<HashEntry> HashEntries { get; init; } = null!;

    public DbSet<HashingAlgorithm> HashingAlgorithms { get; init; } = null!;

    public DbSet<EncryptionAlgorithm> EncryptionAlgorithms { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HashEntry>()
            .HasKey(e => new { e.UserId, e.FileName });
    }
}