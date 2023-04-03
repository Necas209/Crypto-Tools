using System;
using System.Windows;
using CryptoTools.ViewModels;

namespace CryptoTools.Views;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = (MainWindowViewModel)DataContext;

        _viewModel.ShowLogin += ShowLogin;
        App.ShowLogin += LoginFunc;
        App.ShowApp += AppFunc;

        if (App.UserId == 0)
        {
            ShowLogin(true);
        }
        else
        {
            ShowApp(true);
        }
    }

    private void LoginFunc()
    {
        ShowLogin(true);
    }

    private void AppFunc()
    {
        ShowApp(true);
    }

    private void ShowLogin(bool obj)
    {
        MainFrame.Source = new Uri("LoginPage.xaml", UriKind.Relative);
        LogoutButton.Visibility = Visibility.Hidden;
    }

    private void ShowApp(bool obj)
    {
        MainFrame.Source = new Uri("MainWindowContentPage.xaml", UriKind.Relative);
        LogoutButton.Visibility = Visibility.Visible;
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Logout();
    }
}