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

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        if (!await Model.LoadToken())
        {
            var loginWindow = new LoginWindow();
            loginWindow.Activate();
            return;
        }

        // Open connection to the chat server
        await Model.OpenConnection();
        // Get the encryption and hashing algorithms
        await Model.GetAlgorithms();
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }
}