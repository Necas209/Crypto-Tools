using System;
using System.Diagnostics.CodeAnalysis;
using CryptoTools.Views;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace CryptoTools;

public partial class App
{
    public readonly Model Model = new();
    private MainWindow? _mainWindow;

    public App() => InitializeComponent();

    public IntPtr Handle => WindowNative.GetWindowHandle(_mainWindow);

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var loginWindow = new LoginWindow();
        loginWindow.Activate();
    }

    [MemberNotNull(nameof(_mainWindow))]
    public void LaunchMainWindow()
    {
        _mainWindow = new MainWindow();
        _mainWindow.Activate();
    }
}