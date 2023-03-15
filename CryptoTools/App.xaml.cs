using CryptoTools.Data;

namespace CryptoTools;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    public readonly CryptoDbContext Context = new();
}