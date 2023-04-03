using System.IO;
using CryptoTools.Data;
using Microsoft.EntityFrameworkCore;

namespace CryptoTools;

public partial class App
{
    private App()
    {
        InitializeComponent();
        // Create local database
        using var db = new CryptoDbContext();
        var directory = Path.GetDirectoryName(CryptoDbContext.DbPath);
        if (directory is not null) Directory.CreateDirectory(directory);
        // Only in Development, remove in Production
        if (File.Exists(CryptoDbContext.DbPath)) File.Delete(CryptoDbContext.DbPath);
        db.Database.EnsureCreated();
        db.Database.Migrate();
        db.Seed();
    }
}