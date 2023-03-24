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
        _viewModel.EnterChat += AllowMessageSending;
    }

    private void SendButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(UserNameTextBox.Text) || string.IsNullOrWhiteSpace(MessageTextBox.Text)) return;

        var message = $"{UserNameTextBox.Text}: {MessageTextBox.Text}";
        _viewModel.SendMessage(message);

        MessageTextBox.Text = string.Empty;
    }

    private void UpdateChat()
    {
        Dispatcher.Invoke(() => { ChatListBox.ScrollIntoView(ChatListBox.Items[^1]); });
    }

    private void AllowMessageSending()
    {
        MessageTextBox.IsEnabled = true;
        SendButton.IsEnabled = true;
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Login(UserNameTextBox.Text, PasswordBox.SecurePassword);
    }
}