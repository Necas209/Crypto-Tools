using System;
using System.Collections.Generic;
using System.IO;
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
    public DbSet<EncryptionAlgorithm> EncryptionAlgorithms { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }

    public void Seed()
    {
        var hashingAlgorithms = new List<HashingAlgorithm>
        {
            new() { Name = "MD5" },
            new() { Name = "SHA1" },
            new() { Name = "SHA256" },
            new() { Name = "SHA384" },
            new() { Name = "SHA512" }
        };
        HashingAlgorithms.AddRange(hashingAlgorithms);

        var asymmetricAlgorithms = new List<EncryptionAlgorithm>
        {
            new() { Name = "AES", EncryptionType = EncryptionType.Symmetric },
            new() { Name = "DES", EncryptionType = EncryptionType.Symmetric },
            new() { Name = "TripleDES", EncryptionType = EncryptionType.Symmetric },
            new() { Name = "RC2", EncryptionType = EncryptionType.Symmetric },
            new() { Name = "Rijndael", EncryptionType = EncryptionType.Symmetric },
        };
        EncryptionAlgorithms.AddRange(asymmetricAlgorithms);

        SaveChanges();
    }
}