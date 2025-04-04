using System;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Popups;
using CommunityToolkit.WinUI;
using CryptoTools.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using WinRT.Interop;

namespace CryptoTools.Views;

public partial class ChatPage
{
    public ChatPage()
    {
        ViewModel = new ChatViewModel { OnMessageReceived = UpdateChat, OnConnectionClosed = OnConnectionClosed };
        InitializeComponent();
        DispatcherQueue.EnqueueAsync(async Task () => await ViewModel.ReceiveMessages());
    }

    public ChatViewModel ViewModel { get; }

    private async void OnConnectionClosed()
    {
        try
        {
            var dialog = new MessageDialog("The connection to the chat server was closed.", "Connection closed");
            var handle = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(dialog, handle);
            await dialog.ShowAsync();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }

    private void UpdateChat()
    {
        if (ChatListBox.Items.Count == 0)
            return;

        DispatcherQueue.TryEnqueue(() => { ChatListBox.ScrollIntoView(ChatListBox.Items[^1]); });
    }

    private async void BtSendMessage_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var message = TbMessage.Text;
            if (string.IsNullOrWhiteSpace(message))
                return;

            await ViewModel.SendMessage(message);
            TbMessage.Text = string.Empty;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }

    private void TbMessage_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Enter)
            return;

        BtSendMessage_Click(sender, e);
        e.Handled = true;
    }
}