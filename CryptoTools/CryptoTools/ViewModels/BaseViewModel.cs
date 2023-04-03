using System.Collections.Generic;
using System.ComponentModel;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using CryptoTools.Data;

namespace CryptoTools.ViewModels;

public class BaseViewModel : INotifyPropertyChanged
{
    protected readonly CryptoDbContext Context = new();
    protected static int UserId { get; set; }
    protected static ClientWebSocket ClientWebSocket { get; set; } = new();
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(propertyName);
    }
}