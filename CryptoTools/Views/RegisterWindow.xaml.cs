using System;
using Windows.Graphics;
using Windows.UI.Popups;
using CryptoTools.ViewModels;
using Microsoft.UI.Xaml;
using WinRT.Interop;

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

    private async void RegisterSuccess()
    {
        var dialog = new MessageDialog("You can now log in with your new account.", "Registration successful");
        var hwnd = WindowNative.GetWindowHandle(this);
        InitializeWithWindow.Initialize(dialog, hwnd);
        await dialog.ShowAsync();
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.Register();
    }

    private async void ShowError(string message)
    {
        var dialog = new MessageDialog(message, "Registration failed");
        var hwnd = WindowNative.GetWindowHandle(this);
        InitializeWithWindow.Initialize(dialog, hwnd);
        await dialog.ShowAsync();
    }

    private void ShowLogin()
    {
        var loginWindow = new LoginWindow();
        loginWindow.Activate();
        Close();
    }
}