using System.Windows;
using System.Windows.Controls;
using CryptoTools.ViewModels;

namespace CryptoTools.Views;

public partial class LoginPage
{
    private readonly LoginPageViewModel _viewModel;

    public LoginPage()
    {
        InitializeComponent();

        _viewModel = (LoginPageViewModel)DataContext;

        _viewModel.OnError += ShowError;
    }

    private static void ShowError(string message)
    {
        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Login(UserNameTextBox.Text, PasswordBox.SecurePassword);
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
}