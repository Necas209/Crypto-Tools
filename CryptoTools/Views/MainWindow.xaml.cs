using System.Windows;
using CryptoTools.ViewModels;

namespace CryptoTools.Views;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = (MainViewModel)DataContext;
        _viewModel.ShowLogin += ShowLogin;
    }

    private void ShowLogin()
    {
        var loginWindow = new LoginWindow();
        loginWindow.Show();
        Close();
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Logout();
    }
}