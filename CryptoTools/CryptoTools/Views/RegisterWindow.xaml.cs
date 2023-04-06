using System.Windows;
using CryptoTools.ViewModels;

namespace CryptoTools.Views;

public partial class RegisterWindow
{
    private readonly RegisterViewModel _viewModel;

    public RegisterWindow()
    {
        InitializeComponent();
        _viewModel = (RegisterViewModel)DataContext;
        _viewModel.OnError = ShowError;
        _viewModel.ShowLogin = ShowLogin;
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.Register(PasswordBox.SecurePassword, ConfirmPasswordBox.SecurePassword);
    }

    private static void ShowError(string message)
    {
        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void ShowLogin()
    {
        var loginWindow = new LoginWindow();
        loginWindow.Show();
        Close();
    }
}