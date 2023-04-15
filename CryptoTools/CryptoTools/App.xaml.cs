using System;
using CryptoTools.Models;
using CryptoTools.Views;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace CryptoTools;

public partial class App
{
    public readonly Model Model = new();

    public App()
    {
        InitializeComponent();
    }

    public MainWindow MainWindow { get; set; }

    public IntPtr Hwnd => WindowNative.GetWindowHandle(MainWindow);

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var loginWindow = new LoginWindow();
        loginWindow.Activate();
    }
}