using System;
using Windows.System;
using Windows.UI.Popups;
using CryptoTools.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using WinRT.Interop;

namespace CryptoTools.Views;

public partial class ChatPage
{
    public ChatPage()
    {
        InitializeComponent();
        ViewModel.OnMessageReceived = UpdateChat;
        ViewModel.OnConnectionClosed = OnConnectionClosed;
        StartReceivingMessages();
    }

    public ChatViewModel ViewModel { get; } = new();

    private async void OnConnectionClosed()
    {
        var dialog = new MessageDialog("The connection to the chat server was closed.", "Connection closed");
        var hwnd = WindowNative.GetWindowHandle(this);
        InitializeWithWindow.Initialize(dialog, hwnd);
        await dialog.ShowAsync();
    }

    private void UpdateChat()
    {
        if (ChatListBox.Items.Count == 0)
            return;

        DispatcherQueue.TryEnqueue(() => { ChatListBox.ScrollIntoView(ChatListBox.Items[^1]); });
    }

    private async void StartReceivingMessages()
    {
        await ViewModel.ReceiveMessages();
    }

    private void BtSendMessage_Click(object sender, RoutedEventArgs e)
    {
        var message = MessageTb.Text;
        if (string.IsNullOrWhiteSpace(message))
            return;

        ViewModel.SendMessage(message);
        MessageTb.Text = string.Empty;
    }

    private void MessageTb_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Enter)
            return;

        BtSendMessage_Click(sender, e);
        e.Handled = true;
    }
}