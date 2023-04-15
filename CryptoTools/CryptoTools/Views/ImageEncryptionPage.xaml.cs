using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using CryptoTools.Utils;
using CryptoTools.ViewModels;
using Microsoft.Win32;

namespace CryptoTools.Views;

public partial class ImageEncryptionPage
{
    private readonly DispatcherTimer _dispatcherTimer = new()
    {
        Interval = new TimeSpan(0, 0, 5)
    };

    private readonly ImageEncryptionViewModel _viewModel;

    public ImageEncryptionPage()
    {
        InitializeComponent();
        _viewModel = (ImageEncryptionViewModel)DataContext;
        _dispatcherTimer.Tick += (_, _) =>
        {
            Message.Visibility = Visibility.Collapsed;
            _dispatcherTimer.Stop();
        };
    }

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

    private void DropImage_OnClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp"
        };
        if (openFileDialog.ShowDialog() != true) return;
        var file = openFileDialog.FileName;
        // Get the image format
        OriginalImage.Source = BitmapUtils.ToBitmapImage(file);
        using var bitmap = _viewModel.EncryptImage(file);
        var format = BitmapUtils.GetImageFormat(file);
        EncryptedImage.Source = BitmapUtils.ToBitmapImage(bitmap, format);
    }

    private void DropImage_OnDrop(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;
        btn.Background = new SolidColorBrush(Colors.Transparent);
        btn.BorderBrush = new SolidColorBrush(Colors.DimGray);
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            ShowMessage("This is not an image!", Colors.Red);
            return;
        }

        // Note that you can have more than one file.
        var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
        if (files is null || files.Length == 0) return;
        if (files.Length > 1)
        {
            ShowMessage("You can only encrypt one image at a time.", Colors.Red);
            return;
        }

        var file = files[0];
        OriginalImage.Source = BitmapUtils.ToBitmapImage(file);
        using var bitmap = _viewModel.EncryptImage(file);
        var format = BitmapUtils.GetImageFormat(file);
        EncryptedImage.Source = BitmapUtils.ToBitmapImage(bitmap, format);
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