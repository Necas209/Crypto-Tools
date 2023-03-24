using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CryptoTools.Data;

namespace CryptoTools.ViewModels;

public class BaseViewModel : INotifyPropertyChanged
{
    protected readonly CryptoDbContext Context = new();
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}