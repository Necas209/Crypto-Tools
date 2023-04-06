using System.Windows;
using CryptoTools.ViewModels;

namespace CryptoTools.Views;

public partial class LoginWindow
{
    private readonly LoginViewModel _viewModel;

    public LoginWindow()
    {
        InitializeComponent();
        _viewModel = (LoginViewModel)DataContext;
        _viewModel.ShowApp = ShowApp;
        _viewModel.OnError = ShowError;
    }

    private void ShowApp()
    {
        var mainWindow = new MainWindow();
        mainWindow.Show();
        Close();
    }

    private static void ShowError(string message)
    {
        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.Login(UserNameTextBox.Text, PasswordBox.SecurePassword);
    }

    private void UserNameTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        LoginButton.IsDefault = true;
    }

    private void UserNameTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        LoginButton.IsDefault = false;
    }

    private void PasswordBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        LoginButton.IsDefault = true;
    }

    private void PasswordBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        LoginButton.IsDefault = false;
    }

    private void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        var registerWindow = new RegisterWindow();
        registerWindow.Show();
        Close();
    }
}