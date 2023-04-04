using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CryptoLib.Models;

namespace CryptoTools.ViewModels;

public class BaseViewModel : INotifyPropertyChanged
{
    protected static readonly string AppFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CryptoTools");

    public static List<EncryptionAlgorithm> EncryptionAlgorithms { get; private set; } = new();

    public static List<HashingAlgorithm> HashingAlgorithms { get; private set; } = new();

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

    protected static async Task Initialize()
    {
        using var client = new HttpClient();
        EncryptionAlgorithms =
            await client.GetFromJsonAsync<List<EncryptionAlgorithm>>("https://cryptotools.azurewebsites.net/encrypt")
            ?? throw new InvalidOperationException("Unable to retrieve encryption algorithms");
        HashingAlgorithms =
            await client.GetFromJsonAsync<List<HashingAlgorithm>>("https://cryptotools.azurewebsites.net/hash")
            ?? throw new InvalidOperationException("Unable to retrieve hashing algorithms");
    }
}