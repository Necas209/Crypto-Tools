using System.Windows;
using CryptoTools.ViewModels;

namespace CryptoTools.Views;

public partial class ChatPage
{
    private readonly ChatViewModel _viewModel;

    public ChatPage()
    {
        InitializeComponent();
        _viewModel = (ChatViewModel)DataContext;
        _viewModel.OnMessageReceived = UpdateChat;
        _viewModel.OnConnectionClosed = OnConnectionClosed;
        // start a new thread to receive messages from the server
        StartReceivingMessages();
    }

    private static void OnConnectionClosed()
    {
        MessageBox.Show("Connection to the chat server was closed.");
    }

    private void UpdateChat()
    {
        Dispatcher.Invoke(() => ChatListBox.ScrollIntoView(ChatListBox.Items[^1]));
    }

    private async void StartReceivingMessages()
    {
        await _viewModel.ReceiveMessages();
    }

    private void SendButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(MessageTextBox.Text)) return;
        _viewModel.SendMessage(MessageTextBox.Text);
        MessageTextBox.Text = string.Empty;
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