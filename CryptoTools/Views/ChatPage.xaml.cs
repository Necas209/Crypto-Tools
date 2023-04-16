using System;
using CryptoTools.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CryptoTools.Views;

public partial class ChatPage
{
    public ChatPage()
    {
        InitializeComponent();
        ViewModel.OnMessageReceived = UpdateChat;
        ViewModel.OnConnectionClosed = OnConnectionClosed;
        // start a new thread to receive messages from the server
        StartReceivingMessages();
    }

    public ChatViewModel ViewModel { get; } = new();

    private static async void OnConnectionClosed()
    {
        ContentDialog dialog = new()
        {
            Title = "Connection closed",
            Content = "The connection to the chat server was closed.",
            CloseButtonText = "OK"
        };
        await dialog.ShowAsync();
    }

    private void UpdateChat()
    {
        if (ChatListBox.Items.Count > 0)
        {
            DispatcherQueue.TryEnqueue(() => { ChatListBox.ScrollIntoView(ChatListBox.Items[^1]); });
        }
    }

    private async void StartReceivingMessages()
    {
        await ViewModel.ReceiveMessages();
    }

    private void SendButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(MessageTextBox.Text)) return;
        ViewModel.SendMessage(MessageTextBox.Text);
        MessageTextBox.Text = string.Empty;
    }
}