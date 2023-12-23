using System;

namespace CryptoTools.ViewModels;

public class MainViewModel : ViewModelBase
{
    public Action? ShowLogin { get; set; }

    public async void Logout()
    {
        await Model.CloseConnection();
        ShowLogin?.Invoke();
    }
}