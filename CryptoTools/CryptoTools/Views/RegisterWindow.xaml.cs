using System;
using Windows.Graphics;
using CryptoTools.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CryptoTools.Views;

public partial class RegisterWindow
{
    public RegisterWindow()
    {
        InitializeComponent();
        AppWindow.ResizeClient(new SizeInt32(300, 400));
        ViewModel.OnError = ShowError;
        ViewModel.ShowLogin = ShowLogin;
        ViewModel.RegisterSuccess = RegisterSuccess;
    }

    public RegisterViewModel ViewModel { get; } = new();

    private static async void RegisterSuccess()
    {
        var dialog = new ContentDialog
        {
            Title = "Registration successful",
            Content = "You can now log in with your new account.",
            CloseButtonText = "OK"
        };
        await dialog.ShowAsync();
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.Register();
    }

    private static async void ShowError(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "Registration failed",
            Content = message,
            CloseButtonText = "OK"
        };
        await dialog.ShowAsync();
    }

    private void ShowLogin()
    {
        var loginWindow = new LoginWindow();
        loginWindow.Activate();
        Close();
    }
}