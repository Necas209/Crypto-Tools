using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using CryptoTools.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using WinRT.Interop;

namespace CryptoTools.Views;

public partial class EncryptionPage
{
    private readonly App _app = (App)Application.Current;
    private readonly DispatcherTimer _dispatcherTimer = new() { Interval = new TimeSpan(0, 0, 5) };

    public EncryptionPage()
    {
        ViewModel = new EncryptionViewModel { DisplayMessage = ShowMessage };
        InitializeComponent();
        _dispatcherTimer.Tick += (_, _) =>
        {
            Message.Visibility = Visibility.Collapsed;
            _dispatcherTimer.Stop();
        };
    }

    public EncryptionViewModel ViewModel { get; }

    private void ShowMessage(string message, Color color)
    {
        if (_dispatcherTimer.IsEnabled) _dispatcherTimer.Stop();
        Message.Text = message;
        Message.Foreground = new SolidColorBrush(color);
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
        InitializeWithWindow.Initialize(picker, _app.Handle);

        var file = await picker.PickSingleFileAsync();
        if (file is null) return;
        ViewModel.EncryptFile(file.Path);
    }

    private async void BtEncrypt_OnDrop(object sender, DragEventArgs e)
    {
        BtEncrypt.Background = new SolidColorBrush(Colors.Transparent);
        BtEncrypt.BorderBrush = new SolidColorBrush(Colors.DimGray);

        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            if (_dispatcherTimer.IsEnabled) _dispatcherTimer.Stop();
            ShowMessage("This is not a file", Colors.Red);
            return;
        }

        var items = await e.DataView.GetStorageItemsAsync();
        if (items is not [StorageFile file]) return;
        ViewModel.EncryptFile(file.Path);
    }

    private void BtEncrypt_OnDragEnter(object sender, DragEventArgs e)
    {
        BtEncrypt.Background = new SolidColorBrush(Colors.LightSlateGray);
        BtEncrypt.BorderBrush = new SolidColorBrush(Colors.DarkCyan);
    }

    private void BtEncrypt_OnDragLeave(object sender, DragEventArgs e)
    {
        BtEncrypt.Background = new SolidColorBrush(Colors.Transparent);
        BtEncrypt.BorderBrush = new SolidColorBrush(Colors.DimGray);
    }

    private async void BtDecrypt_OnClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.Desktop,
            FileTypeFilter = { ".enc" }
        };
        InitializeWithWindow.Initialize(picker, _app.Handle);

        var file = await picker.PickSingleFileAsync();
        if (file is null) return;
        ViewModel.DecryptFile(file.Path);
    }

    private async void BtDecrypt_OnDrop(object sender, DragEventArgs e)
    {
        BtDecrypt.Background = new SolidColorBrush(Colors.Transparent);
        BtDecrypt.BorderBrush = new SolidColorBrush(Colors.DimGray);

        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            ShowMessage("This is not a file!", Colors.Red);
            return;
        }

        var items = await e.DataView.GetStorageItemsAsync();
        if (items is not [StorageFile { FileType: ".enc" } file])
        {
            ShowMessage("This is not an encrypted file!", Colors.Red);
            return;
        }

        ViewModel.DecryptFile(file.Path);
    }

    private void BtDecrypt_OnDragEnter(object sender, DragEventArgs e)
    {
        BtDecrypt.Background = new SolidColorBrush(Colors.LightSlateGray);
        BtDecrypt.BorderBrush = new SolidColorBrush(Colors.DarkCyan);
    }

    private void BtDecrypt_OnDragLeave(object sender, DragEventArgs e)
    {
        BtDecrypt.Background = new SolidColorBrush(Colors.Transparent);
        BtDecrypt.BorderBrush = new SolidColorBrush(Colors.DimGray);
    }

    private void OnDragOver(object sender, DragEventArgs e) => e.AcceptedOperation = DataPackageOperation.Copy;
}