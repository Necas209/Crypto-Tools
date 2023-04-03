using System;
using System.Net.WebSockets;
using System.Threading;

namespace CryptoTools.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    public event Action<bool>? ShowLogin;

    public async void Logout()
    {
        await App.ClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Logout", CancellationToken.None);
        App.ClientWebSocket.Dispose();
        App.ClientWebSocket = new ClientWebSocket();
        ShowLogin?.Invoke(false);
    }
}