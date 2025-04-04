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
        ViewModel = new LoginViewModel { OnError = ShowError };
        InitializeComponent();
        AppWindow.ResizeClient(new SizeInt32(300, 400));
    }

    public LoginViewModel ViewModel { get; }

    private async void ShowError(string message)
    {
        try
        {
            var dialog = new MessageDialog(message, "Error");
            var handle = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(dialog, handle);
            await dialog.ShowAsync();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }

    private async void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var loggedIn = await ViewModel.Login();
            if (!loggedIn) return;

            _app.LaunchMainWindow();
            Close();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }


    private void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        var registerWindow = new RegisterWindow();
        registerWindow.Activate();
        Close();
    }

    private void Pb_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Enter)
            return;

        BtnLogin_Click(sender, e);
        e.Handled = true;
    }
}