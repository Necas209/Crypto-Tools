using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using CryptoTools.ViewModels;
using Microsoft.Win32;

namespace CryptoTools.Views;

public partial class FileIntegrityPage
{
    private readonly string _dialogPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    private readonly DispatcherTimer _dispatcherTimer = new()
    {
        Interval = new TimeSpan(0, 0, 5)
    };

    private readonly FileIntegrityPageViewModel _viewModel;

    public FileIntegrityPage()
    {
        InitializeComponent();
        _viewModel = (FileIntegrityPageViewModel)DataContext;
        _viewModel.DisplayMessage += ShowMessage;
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

    private void Register_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            InitialDirectory = _dialogPath,
            Filter = "All files (*.*)|*.*"
        };
        if (dialog.ShowDialog() != true) return;
        _viewModel.RegisterFiles(dialog.FileNames);
    }

    private void Register_OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            // Note that you can have more than one file.
            var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0) return;
            // Register the file
            _viewModel.RegisterFiles(files);
        }
        else
        {
            if (_dispatcherTimer.IsEnabled) _dispatcherTimer.Stop();
            ShowMessage("This is not a file", Colors.Red);
        }
    }

    private void Register_OnDragEnter(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;
     
        btn.Background = new SolidColorBrush(Colors.LightSlateGray);
        btn.BorderBrush = new SolidColorBrush(Colors.DarkCyan);
        
    }

    private void Register_OnDragLeave(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;
     
        btn.Background = new SolidColorBrush(Colors.Transparent);
        btn.BorderBrush = new SolidColorBrush(Colors.DimGray);
    }

    private void Validation_OnClick(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "All files (*.*)|*.*"
        };
        if (openFileDialog.ShowDialog() != true) return;
        _viewModel.ValidateFile(openFileDialog.FileName);
    }

    private void Validation_OnDrop(object sender, DragEventArgs e)
    {
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
        _viewModel.ValidateFile(files[0]);
    }

    private void Validation_OnDragEnter(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;
     
        btn.Background = new SolidColorBrush(Colors.LightSlateGray);
        btn.BorderBrush = new SolidColorBrush(Colors.DarkCyan);
    }

    private void Validation_OnDragLeave(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;
     
        btn.Background = new SolidColorBrush(Colors.Transparent);
        btn.BorderBrush = new SolidColorBrush(Colors.DimGray);
    }
}