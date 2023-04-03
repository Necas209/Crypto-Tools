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
        // start a new thread to receive messages from the server
        StartReceivingMessages();
    }

    private async void StartReceivingMessages()
    {
        await _viewModel.ReceiveMessages();
    }

    private void SendButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(MessageTextBox.Text)) return;
        ChatPageViewModel.SendMessage(MessageTextBox.Text);
        MessageTextBox.Text = string.Empty;
    }

    private void UpdateChat()
    {
        Dispatcher.Invoke(() => ChatListBox.ScrollIntoView(ChatListBox.Items[^1]));
    }

    private void MessageTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        SendButton.IsDefault = true;
    }

    private void MessageTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        SendButton.IsDefault = false;
    }
}