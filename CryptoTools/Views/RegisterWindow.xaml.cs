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
        ViewModel = new RegisterViewModel { OnError = ShowError, RegisterSuccess = RegisterSuccess };
        InitializeComponent();
        AppWindow.ResizeClient(new SizeInt32(300, 400));
    }

    public RegisterViewModel ViewModel { get; }

    private async void RegisterSuccess()
    {
        try
        {
            var dialog = new MessageDialog("You can now log in with your new account.", "Registration successful");
            var handle = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(dialog, handle);
            await dialog.ShowAsync();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }

    private async void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var registered = await ViewModel.Register();
            if (!registered) return;

            var loginWindow = new LoginWindow();
            loginWindow.Activate();
            Close();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }

    private async void ShowError(string message)
    {
        try
        {
            var dialog = new MessageDialog(message, "Registration failed");
            var handle = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(dialog, handle);
            await dialog.ShowAsync();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }
}