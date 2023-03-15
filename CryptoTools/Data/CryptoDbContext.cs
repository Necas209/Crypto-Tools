using CryptoTools.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoTools.Data;

public class CryptoDbContext : DbContext
{
    public DbSet<HashEntry> HashEntries { get; set; } = null!;
    public DbSet<HashingAlgorithm> HashingAlgorithms { get; set; } = null!;

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(
            "Data Source=cryptodb.sqlite");
    }
}