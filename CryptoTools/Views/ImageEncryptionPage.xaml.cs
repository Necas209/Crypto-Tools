using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using CryptoTools.Utils;
using CryptoTools.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using WinRT.Interop;
using Color = Windows.UI.Color;

namespace CryptoTools.Views;

public partial class ImageEncryptionPage
{
    private readonly App _app = (App)Application.Current;
    private readonly DispatcherTimer _dispatcherTimer = new() { Interval = new TimeSpan(0, 0, 5) };

    public ImageEncryptionPage()
    {
        ViewModel = new ImageEncryptionViewModel();
        InitializeComponent();
        _dispatcherTimer.Tick += (_, _) =>
        {
            Message.Visibility = Visibility.Collapsed;
            _dispatcherTimer.Stop();
        };
    }

    public ImageEncryptionViewModel ViewModel { get; }

    private void ShowMessage(string message, Color color)
    {
        if (_dispatcherTimer.IsEnabled) _dispatcherTimer.Stop();
        Message.Text = message;
        Message.Foreground = new SolidColorBrush(color);
        Message.Visibility = Visibility.Visible;
        _dispatcherTimer.Start();
    }

    private async void BtImage_OnClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker
        {
            ViewMode = PickerViewMode.Thumbnail,
            SuggestedStartLocation = PickerLocationId.PicturesLibrary,
            FileTypeFilter = { ".png", ".jpeg", ".jpg", ".bmp" }
        };
        InitializeWithWindow.Initialize(picker, _app.Handle);

        var file = await picker.PickSingleFileAsync();
        if (file is null) return;

        OriginalImage.Source = await BitmapUtils.ToBitmapImage(file);
        await ViewModel.EncryptImage(file.Path);
    }

    private async void BtImage_OnDrop(object sender, DragEventArgs e)
    {
        BtImage.Background = new SolidColorBrush(Colors.Transparent);
        BtImage.BorderBrush = new SolidColorBrush(Colors.DimGray);

        var items = await e.DataView.GetStorageItemsAsync();
        if (items is not [StorageFile file])
        {
            ShowMessage("You can only encrypt one image at a time.", Colors.Red);
            return;
        }

        if (file is not { FileType: ".png" or ".jpeg" or ".jpg" or ".bmp" })
        {
            ShowMessage("This is not an image!", Colors.Red);
            return;
        }

        OriginalImage.Source = await BitmapUtils.ToBitmapImage(file);
        await ViewModel.EncryptImage(file.Path);
    }

    private void BtImage_OnDragEnter(object sender, DragEventArgs e)
    {
        BtImage.Background = new SolidColorBrush(Colors.LightSlateGray);
        BtImage.BorderBrush = new SolidColorBrush(Colors.DarkCyan);
    }

    private void BtImage_OnDragLeave(object sender, DragEventArgs e)
    {
        BtImage.Background = new SolidColorBrush(Colors.Transparent);
        BtImage.BorderBrush = new SolidColorBrush(Colors.DimGray);
    }

    private void BtImage_OnDragOver(object sender, DragEventArgs e) => e.AcceptedOperation = DataPackageOperation.Copy;
}