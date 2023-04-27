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
        AppWindow.ResizeClient(new SizeInt32(1100, 750));
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
}