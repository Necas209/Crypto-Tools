using System;
using System.Net.WebSockets;
using System.Threading;

namespace CryptoTools.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    public Action? ShowLogin { get; set; }

    public async void Logout()
    {
        await ClientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Logout",
            CancellationToken.None);
        ClientWebSocket.Dispose();
        ClientWebSocket = new ClientWebSocket();
        ShowLogin?.Invoke();
    }
}