using Windows.Graphics;
using CryptoTools.ViewModels;
using Microsoft.UI.Xaml;

namespace CryptoTools.Views;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        AppWindow.ResizeClient(WindowSize);
        ViewModel.ShowLogin += ShowLogin;
    }

    private MainViewModel ViewModel { get; } = new();

    private void ShowLogin()
    {
        var loginWindow = new LoginWindow();
        loginWindow.Activate();
        Close();
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Logout();
    }

    private static readonly SizeInt32 WindowSize = new(1100, 750);
}