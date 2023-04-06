using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CryptoTools.ViewModels;

public class ChatViewModel : ViewModelBase
{
    public Action? OnMessageReceived;
    public ObservableCollection<string> ChatMessages { get; } = new();

    public async Task ReceiveMessages()
    {
        while (Model.IsConnected)
        {
            // receive a message from the server
            var message = await Model.ReceiveMessage();
            // add the received message to the UI
            if (!string.IsNullOrWhiteSpace(message)) AddMessage(message);
            // wait for a short period before polling for more messages
            await Task.Delay(100);
        }
    }

    private void AddMessage(string message)
    {
        ChatMessages.Add(message);
        OnMessageReceived?.Invoke();
    }

    public async void SendMessage(string message)
    {
        await Model.SendMessage(message);
    }
}