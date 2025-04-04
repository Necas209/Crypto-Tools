using System;
using Windows.Graphics;
using CryptoTools.ViewModels;
using Microsoft.UI.Xaml;

namespace CryptoTools.Views;

public partial class MainWindow
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        _viewModel = new MainViewModel();
        InitializeComponent();
        AppWindow.ResizeClient(new SizeInt32(1100, 750));
    }

    private async void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _viewModel.Logout();
            var loginWindow = new LoginWindow();
            loginWindow.Activate();
            Close();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }
}