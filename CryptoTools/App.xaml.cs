using System.IO;
using CryptoTools.Data;
using Microsoft.EntityFrameworkCore;

namespace CryptoTools;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private App()
    {
        InitializeComponent();
        using var db = new CryptoDbContext();
        var directory = Path.GetDirectoryName(CryptoDbContext.DbPath);
        if (directory is not null) Directory.CreateDirectory(directory);
        db.Database.EnsureCreated();
        db.Database.Migrate();
        db.Seed();
    }
}