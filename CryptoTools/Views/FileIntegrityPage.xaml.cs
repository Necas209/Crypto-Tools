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
    private readonly DispatcherTimer _dispatcherTimer = new()
    {
        Interval = new TimeSpan(0, 0, 5)
    };

    private readonly string _dialogPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    private readonly FileIntegrityPageViewModel _viewModel;

    public FileIntegrityPage()
    {
        InitializeComponent();

        _viewModel = (FileIntegrityPageViewModel)DataContext;

        _dispatcherTimer.Tick += (_, _) =>
        {
            Message.Visibility = Visibility.Collapsed;
            _dispatcherTimer.Stop();
        };

        _viewModel.DisplayMessage += ShowMessage;
    }

    private void ShowMessage(string message, Color color)
    {
        Message.Text = message;
        // Change the color of the text
        Message.Foreground = new SolidColorBrush(color);
        // Timer to change the visibility of the text
        Message.Visibility = Visibility.Visible;

        _dispatcherTimer.Start();
    }

    private void Register_OnClick(object sender, RoutedEventArgs e)
    {
    }

    private void Register_OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            // Note that you can have more than one file.
            var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);

            if (files is null) return;

            // Register the file
            _viewModel.RegisterFiles(files);

            _dispatcherTimer.Start();
        }
        else
        {
            if (_dispatcherTimer.IsEnabled)
            {
                _dispatcherTimer.Stop();
            }

            ShowMessage("This is not a file", Colors.Red);

            _dispatcherTimer.Start();
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
        OpenFileDialog openFileDialog = new();
        openFileDialog.Multiselect = true;
        openFileDialog.Filter = "All files (*.*)|*.*";
        if (openFileDialog.ShowDialog() == true)
        {
        }
    }

    private void Validation_OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            // Note that you can have more than one file.
            var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);

            if (files is null) return;

            // validate the file
            _viewModel.ValidateFile(files[0]);

            _dispatcherTimer.Start();
        }
        else
        {
            if (_dispatcherTimer.IsEnabled)
            {
                _dispatcherTimer.Stop();
            }

            Message.Text = "Não é um arquivo";
            // Change the color of the text
            Message.Foreground = new SolidColorBrush(Colors.Red);
            // Timer to change the visibility of the text
            Message.Visibility = Visibility.Visible;

            _dispatcherTimer.Start();
        }
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