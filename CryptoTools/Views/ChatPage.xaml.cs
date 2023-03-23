using System;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using CryptoTools.ViewModels;

namespace CryptoTools.Views;

public partial class ChatPage
{

    private readonly ChatPageViewModel _viewModel;
    
    private readonly Timer _chatTimer;
    
    public ChatPage()
    {
        InitializeComponent();

        _viewModel = (ChatPageViewModel) DataContext;
        _viewModel.OnMessageReceived += (_, _) => ScrollChatMessages();
        _chatTimer = new Timer(UpdateChatMessages!, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(500));

        EventManager.RegisterClassHandler(typeof(Page), LoadedEvent, new RoutedEventHandler(Page_Loaded));
        EventManager.RegisterClassHandler(typeof(Page), UnloadedEvent, new RoutedEventHandler(Page_Closing));
    }
    
    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        MessageTextBox.Text = "ola";
        //await _viewModel.StartWebSocketListenerAsync();
    }
    
    private void SendButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameTextBox.Text) || string.IsNullOrWhiteSpace(MessageTextBox.Text))
        {
            return;
        }

        var message = $"{NameTextBox.Text}: {MessageTextBox.Text}";
        var buffer = Encoding.UTF8.GetBytes(message);

        _viewModel.SendMessage(buffer);

        MessageTextBox.Text = string.Empty;
    }
    
    
    
    private void UpdateChatMessages(object state)
    {
        _viewModel.UpdateChatMessages(state);
    }
    
    private void ScrollChatMessages()
    {
        ChatListBox.ScrollIntoView(ChatListBox.Items[^1]);
    }
    
    private void Page_Closing(object sender, RoutedEventArgs e)
    {
        _viewModel.CancelChat();
        _chatTimer.Dispose();
    }
    
}