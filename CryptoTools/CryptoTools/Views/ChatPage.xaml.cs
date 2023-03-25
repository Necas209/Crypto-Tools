using System.Windows;
using CryptoTools.ViewModels;

namespace CryptoTools.Views;

public partial class ChatPage
{
    private readonly ChatPageViewModel _viewModel;

    public ChatPage()
    {
        InitializeComponent();
        _viewModel = (ChatPageViewModel)DataContext;
        _viewModel.OnMessageReceived += UpdateChat;
        _viewModel.EnterChat += UpdateView;
        _viewModel.LeaveChat += UpdateView;
        _viewModel.OnError += ShowError;
    }

    private static void ShowError(string message)
    {
        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void SendButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(UserNameTextBox.Text) || string.IsNullOrWhiteSpace(MessageTextBox.Text)) return;
        _viewModel.SendMessage(MessageTextBox.Text);
        MessageTextBox.Text = string.Empty;
    }

    private void UpdateChat()
    {
        Dispatcher.Invoke(() => ChatListBox.ScrollIntoView(ChatListBox.Items[^1]));
    }

    private void UpdateView(bool isLoggedIn)
    {
        MessageTextBox.IsEnabled = isLoggedIn;
        SendButton.IsEnabled = isLoggedIn;
        LoginButton.IsEnabled = !isLoggedIn;
        LogoutButton.IsEnabled = isLoggedIn;
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Login(PasswordBox.SecurePassword);
    }

    private void UserNameTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        LoginButton.IsDefault = true;
    }

    private void UserNameTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        LoginButton.IsDefault = false;
    }

    private void PasswordBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        LoginButton.IsDefault = true;
    }

    private void PasswordBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        LoginButton.IsDefault = false;
    }

    private void MessageTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        SendButton.IsDefault = true;
    }

    private void MessageTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        SendButton.IsDefault = false;
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Logout();
    }
}