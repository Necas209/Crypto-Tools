using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using CryptoTools.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinRT.Interop;

namespace CryptoTools.Views;

public partial class EncryptionPage
{
    private readonly App _app = (App)Application.Current;

    private readonly DispatcherTimer _dispatcherTimer = new()
    {
        Interval = new TimeSpan(0, 0, 5)
    };

    public EncryptionPage()
    {
        InitializeComponent();
        ViewModel.DisplayMessage = ShowMessage;
        _dispatcherTimer.Tick += (_, _) =>
        {
            Message.Visibility = Visibility.Collapsed;
            _dispatcherTimer.Stop();
        };
    }

    public EncryptionViewModel ViewModel { get; } = new();

    private void ShowMessage(string message, Color color)
    {
        if (_dispatcherTimer.IsEnabled) _dispatcherTimer.Stop();
        Message.Text = message;
        // Change the color of the text
        Message.Foreground = new SolidColorBrush(color);
        // Timer to change the visibility of the text
        Message.Visibility = Visibility.Visible;
        _dispatcherTimer.Start();
    }

    private async void BtEncrypt_OnClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.Desktop,
            FileTypeFilter = { "*" }
        };
        InitializeWithWindow.Initialize(picker, _app.Hwnd);
        var file = await picker.PickSingleFileAsync();
        if (file is null) return;
        ViewModel.EncryptFile(file.Path);
    }

    private async void BtEncrypt_OnDrop(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;
        btn.Background = new SolidColorBrush(Colors.Transparent);
        btn.BorderBrush = new SolidColorBrush(Colors.DimGray);

        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            if (items.Count != 1) return;
            if (items[0] is not StorageFile file) return;
            // Register the file
            ViewModel.EncryptFile(file.Path);
        }
        else
        {
            if (_dispatcherTimer.IsEnabled) _dispatcherTimer.Stop();
            ShowMessage("This is not a file", Colors.Red);
        }
    }

    private void BtEncrypt_OnDragEnter(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;

        btn.Background = new SolidColorBrush(Colors.LightSlateGray);
        btn.BorderBrush = new SolidColorBrush(Colors.DarkCyan);
    }

    private void BtEncrypt_OnDragLeave(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;

        btn.Background = new SolidColorBrush(Colors.Transparent);
        btn.BorderBrush = new SolidColorBrush(Colors.DimGray);
    }

    private async void BtDecrypt_OnClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.Desktop,
            FileTypeFilter = { ".enc" }
        };
        InitializeWithWindow.Initialize(picker, _app.Hwnd);
        var file = await picker.PickSingleFileAsync();
        if (file is null) return;
        ViewModel.DecryptFile(file.Path);
    }

    private async void BtDecrypt_OnDrop(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;
        btn.Background = new SolidColorBrush(Colors.Transparent);
        btn.BorderBrush = new SolidColorBrush(Colors.DimGray);

        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            ShowMessage("This is not a file!", Colors.Red);
            return;
        }

        var items = await e.DataView.GetStorageItemsAsync();
        if (items.Count != 1) return;
        if (items[0] is not StorageFile file) return;
        if (file.FileType != ".enc")
        {
            ShowMessage("This is not an encrypted file!", Colors.Red);
            return;
        }

        // Decrypt the file
        ViewModel.DecryptFile(file.Path);
    }

    private void BtDecrypt_OnDragEnter(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;

        btn.Background = new SolidColorBrush(Colors.LightSlateGray);
        btn.BorderBrush = new SolidColorBrush(Colors.DarkCyan);
    }

    private void BtDecrypt_OnDragLeave(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;

        btn.Background = new SolidColorBrush(Colors.Transparent);
        btn.BorderBrush = new SolidColorBrush(Colors.DimGray);
    }
}