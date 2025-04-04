using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CryptoTools.ViewModels;

public class ChatViewModel : ViewModelBase
{
    public Action? OnConnectionClosed;

    public Action? OnMessageReceived;

    public ObservableCollection<string> ChatMessages { get; } = [];

    public async Task ReceiveMessages()
    {
        while (Model.IsConnected)
        {
            // receive a message from the server
            var message = await Model.ReceiveMessage();
            if (message is null)
            {
                OnConnectionClosed?.Invoke();
                break;
            }

            if (message == "Logged out") return;

            ChatMessages.Add(message);
            OnMessageReceived?.Invoke();
            // wait for a short period before polling for more messages
            await Task.Delay(100);
        }

        await Model.CloseConnection();
    }

    public async Task SendMessage(string message) => await Model.SendMessage(message);
}