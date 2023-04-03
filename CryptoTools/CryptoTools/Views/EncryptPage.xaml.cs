using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CryptoTools.ViewModels;
using Microsoft.Win32;

namespace CryptoTools.Views;

public partial class EncryptPage
{
    private readonly DispatcherTimer _dispatcherTimer = new()
    {
        Interval = new TimeSpan(0, 0, 5)
    };

    private readonly EncryptPageViewModel _viewModel;

    public EncryptPage()
    {
        InitializeComponent();

        _viewModel = (EncryptPageViewModel)DataContext;

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

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        //_viewModel?.HashText();
    }

    private void DropImage_OnClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "All files (*.*)|*.*"
        };
        if (openFileDialog.ShowDialog() != true) return;

        img1.Source = new BitmapImage(new Uri(openFileDialog.FileName));

        var encryptImage = _viewModel.EncryptImage(openFileDialog.FileName);

        img2.Source = encryptImage;
    }

    private void DropImage_OnDrop(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;
        btn.Background = new SolidColorBrush(Colors.Transparent);
        btn.BorderBrush = new SolidColorBrush(Colors.DimGray);

        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            ShowMessage("This is not a file!", Colors.Red);
            return;
        }

        // Note that you can have more than one file.
        var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
        if (files == null || files.Length == 0) return;
        if (files.Length > 1) ShowMessage("You can only hash one file at a time.", Colors.Red);
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