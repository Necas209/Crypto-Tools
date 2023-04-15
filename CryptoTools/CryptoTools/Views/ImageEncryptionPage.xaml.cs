using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using CryptoTools.Utils;
using CryptoTools.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinRT.Interop;

namespace CryptoTools.Views;

public partial class ImageEncryptionPage
{
    private readonly App _app = (App)Application.Current;

    private readonly DispatcherTimer _dispatcherTimer = new()
    {
        Interval = new TimeSpan(0, 0, 5)
    };

    public ImageEncryptionPage()
    {
        InitializeComponent();
        _dispatcherTimer.Tick += (_, _) =>
        {
            Message.Visibility = Visibility.Collapsed;
            _dispatcherTimer.Stop();
        };
    }

    public ImageEncryptionViewModel ViewModel { get; } = new();

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

    private async void DropImage_OnClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker
        {
            ViewMode = PickerViewMode.Thumbnail,
            SuggestedStartLocation = PickerLocationId.PicturesLibrary,
            FileTypeFilter = { ".png", ".jpeg", ".jpg", ".bmp" }
        };
        InitializeWithWindow.Initialize(picker, _app.Hwnd);
        var file = await picker.PickSingleFileAsync();
        if (file is null) return;
        // Get the image format
        OriginalImage.Source = await BitmapUtils.ToBitmapImage(file);
        using var bitmap = ViewModel.EncryptImage(file.Path);
        var format = BitmapUtils.GetImageFormat(file.Path);
        EncryptedImage.Source = await BitmapUtils.ToBitmapImage(bitmap, format);
    }

    private async void DropImage_OnDrop(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;
        btn.Background = new SolidColorBrush(Colors.Transparent);
        btn.BorderBrush = new SolidColorBrush(Colors.DimGray);
        if (e.DataView.Contains(StandardDataFormats.Bitmap))
        {
            ShowMessage("This is not an image!", Colors.Red);
            return;
        }

        var items = await e.DataView.GetStorageItemsAsync();
        switch (items.Count)
        {
            case 0:
                return;
            case > 1:
                ShowMessage("You can only encrypt one image at a time.", Colors.Red);
                return;
        }

        if (items[0] is not StorageFile file) return;
        OriginalImage.Source = await BitmapUtils.ToBitmapImage(file);
        using var bitmap = ViewModel.EncryptImage(file.Path);
        var format = BitmapUtils.GetImageFormat(file.Path);
        EncryptedImage.Source = await BitmapUtils.ToBitmapImage(bitmap, format);
    }

    private void DropImage_OnDragEnter(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;

        btn.Background = new SolidColorBrush(Colors.LightSlateGray);
        btn.BorderBrush = new SolidColorBrush(Colors.DarkCyan);
    }

    private void DropImage_OnDragLeave(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;

        btn.Background = new SolidColorBrush(Colors.Transparent);
        btn.BorderBrush = new SolidColorBrush(Colors.DimGray);
    }
}