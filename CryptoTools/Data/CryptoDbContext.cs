using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CryptoTools.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoTools.Data;

public class CryptoDbContext : DbContext
{
    public static readonly string DbPath =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CryptoTools",
            "cryptodb.sqlite"
        );

    public DbSet<HashEntry> HashEntries { get; set; } = null!;
    public DbSet<HashingAlgorithm> HashingAlgorithms { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }

    public void Seed()
    {
        if (HashingAlgorithms.Any()) return;
        var algorithms = new List<HashingAlgorithm>
        {
            new() { Name = "MD5" },
            new() { Name = "SHA1" },
            new() { Name = "SHA256" },
            new() { Name = "SHA384" },
            new() { Name = "SHA512" }
        };
        HashingAlgorithms.AddRange(algorithms);
        SaveChanges();
    }
}