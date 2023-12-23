using System;
using Windows.Graphics;
using Windows.System;
using Windows.UI.Popups;
using CryptoTools.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using WinRT.Interop;

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

    private async void ShowError(string message)
    {
        var dialog = new MessageDialog(message, "Error");
        var hwnd = WindowNative.GetWindowHandle(this);
        InitializeWithWindow.Initialize(dialog, hwnd);
        await dialog.ShowAsync();
    }

    private async void LoginBt_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.Login();
    }

    private void RegisterBt_Click(object sender, RoutedEventArgs e)
    {
        var registerWindow = new RegisterWindow();
        registerWindow.Activate();
        Close();
    }

    private void Pb_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Enter)
            return;

        LoginBt_Click(sender, e);
        e.Handled = true;
    }
}