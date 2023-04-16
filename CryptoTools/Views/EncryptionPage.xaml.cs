using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using CryptoTools.ViewModels;
using Microsoft.Win32;

namespace CryptoTools.Views;

public partial class EncryptionPage
{
    private readonly string _dialogPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    private readonly DispatcherTimer _dispatcherTimer = new()
    {
        Interval = new TimeSpan(0, 0, 5)
    };

    private readonly EncryptionViewModel _viewModel;

    public EncryptionPage()
    {
        InitializeComponent();
        _viewModel = (EncryptionViewModel)DataContext;
        _viewModel.DisplayMessage = ShowMessage;
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

    private void BtEncrypt_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            InitialDirectory = _dialogPath,
            Filter = "All files (*.*)|*.*"
        };
        if (dialog.ShowDialog() != true) return;
        _viewModel.EncryptFile(dialog.FileName);
    }

    private void BtEncrypt_OnDrop(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;
        btn.Background = new SolidColorBrush(Colors.Transparent);
        btn.BorderBrush = new SolidColorBrush(Colors.DimGray);

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            // Note that you can have more than one file.
            var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
            if (files is not { Length: 1 }) return;
            // Register the file
            _viewModel.EncryptFile(files[0]);
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

    private void BtDecrypt_OnClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "All files (*.*)|*.*"
        };
        if (openFileDialog.ShowDialog() != true) return;
        _viewModel.DecryptFile(openFileDialog.FileName);
    }

    private void BtDecrypt_OnDrop(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;
        btn.Background = new SolidColorBrush(Colors.Transparent);
        btn.BorderBrush = new SolidColorBrush(Colors.DimGray);

        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            ShowMessage("This is not a file!", Colors.Red);
            return;
        }

        var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
        if (files is not { Length: 1 }) return;
        if (files.Length > 1)
        {
            ShowMessage("You can only decrypt one file at a time.", Colors.Red);
            return;
        }

        // Decrypt the file
        _viewModel.DecryptFile(files[0]);
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