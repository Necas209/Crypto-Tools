using System;
using Windows.Graphics;
using CryptoTools.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CryptoTools.Views;

public partial class LoginWindow
{
    private readonly App _app = (App)Application.Current;

    public LoginWindow()
    {
        InitializeComponent();
        AppWindow.ResizeClient(new SizeInt32(300, 400));
        ViewModel.ShowApp = ShowApp;
        ViewModel.OnError = ShowError;
    }

    public LoginViewModel ViewModel { get; } = new();

    private void ShowApp()
    {
        var mainWindow = new MainWindow();
        _app.MainWindow = mainWindow;
        mainWindow.Activate();
        Close();
    }

    private static async void ShowError(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "Error",
            Content = message,
            CloseButtonText = "Ok"
        };
        await dialog.ShowAsync();
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.Login();
    }

    private void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        var registerWindow = new RegisterWindow();
        registerWindow.Activate();
        Close();
    }
}