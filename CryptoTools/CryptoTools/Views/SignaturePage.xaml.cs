using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using CryptoTools.ViewModels;
using Microsoft.Win32;

namespace CryptoTools.Views;

public partial class SignaturePage
{
    private readonly string _dialogPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    private readonly DispatcherTimer _dispatcherTimer = new()
    {
        Interval = new TimeSpan(0, 0, 5)
    };

    private readonly SignaturePageViewModel _viewModel;

    public SignaturePage()
    {
        InitializeComponent();

        _viewModel = (SignaturePageViewModel)DataContext;
        _viewModel.DisplayMessage = ShowMessage;
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

    private void Sign_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            InitialDirectory = _dialogPath,
            Filter = "All files (*.*)|*.*"
        };
        if (dialog.ShowDialog() != true) return;
        _viewModel.SignFiles(dialog.FileNames);
    }

    private void Sign_OnDrop(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;
        btn.Background = new SolidColorBrush(Colors.Transparent);
        btn.BorderBrush = new SolidColorBrush(Colors.DimGray);

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            // Note that you can have more than one file.
            var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0) return;
            // Register the file
            _viewModel.SignFiles(files);
        }
        else
        {
            if (_dispatcherTimer.IsEnabled) _dispatcherTimer.Stop();
            ShowMessage("This is not a file", Colors.Red);
        }
    }

    private void Sign_OnDragEnter(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;

        btn.Background = new SolidColorBrush(Colors.LightSlateGray);
        btn.BorderBrush = new SolidColorBrush(Colors.DarkCyan);
    }

    private void Sign_OnDragLeave(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;

        btn.Background = new SolidColorBrush(Colors.Transparent);
        btn.BorderBrush = new SolidColorBrush(Colors.DimGray);
    }

    private void Verify_OnClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "All files (*.*)|*.*"
        };
        if (openFileDialog.ShowDialog() != true) return;
        _viewModel.VerifySignature(openFileDialog.FileName);
    }

    private void Verify_OnDrop(object sender, DragEventArgs e)
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
        if (files.Length > 1)
        {
            ShowMessage("You can only validate one file at a time.", Colors.Red);
            return;
        }

        // validate the file
        _viewModel.VerifySignature(files[0]);
    }

    private void Verify_OnDragEnter(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;

        btn.Background = new SolidColorBrush(Colors.LightSlateGray);
        btn.BorderBrush = new SolidColorBrush(Colors.DarkCyan);
    }

    private void Verifiy_OnDragLeave(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;

        btn.Background = new SolidColorBrush(Colors.Transparent);
        btn.BorderBrush = new SolidColorBrush(Colors.DimGray);
    }
}