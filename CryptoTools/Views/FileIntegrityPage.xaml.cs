using System;
using System.Linq;
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

public partial class FileIntegrityPage
{
    private readonly App _app = (App)Application.Current;

    private readonly DispatcherTimer _dispatcherTimer = new()
    {
        Interval = new TimeSpan(0, 0, 5)
    };

    public FileIntegrityPage()
    {
        InitializeComponent();
        ViewModel.DisplayMessage = ShowMessage;
        _dispatcherTimer.Tick += (_, _) =>
        {
            Message.Visibility = Visibility.Collapsed;
            _dispatcherTimer.Stop();
        };
    }

    public FileIntegrityViewModel ViewModel { get; } = new();

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

    private async void Register_OnClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.Desktop,
            FileTypeFilter = { "*" }
        };
        InitializeWithWindow.Initialize(picker, _app.Hwnd);
        var files = await picker.PickMultipleFilesAsync();
        if (files.Count == 0) return;
        // Register the files
        ViewModel.RegisterFiles(files);
    }

    private async void Register_OnDrop(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;
        btn.Background = new SolidColorBrush(Colors.Transparent);
        btn.BorderBrush = new SolidColorBrush(Colors.DimGray);

        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            if (items.Count == 0) return;
            // Register the files
            var files = items
                .Where(item => item.IsOfType(StorageItemTypes.File))
                .Cast<StorageFile>();
            ViewModel.RegisterFiles(files);
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

    private async void Validation_OnClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.Desktop,
            FileTypeFilter = { "*" }
        };
        var file = await picker.PickSingleFileAsync();
        if (file == null) return;
        // validate the file
        ViewModel.ValidateFile(file.Path);
    }

    private async void Validation_OnDrop(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;
        btn.Background = new SolidColorBrush(Colors.Transparent);
        btn.BorderBrush = new SolidColorBrush(Colors.DimGray);

        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            ShowMessage("This is not a file!", Colors.Red);
            return;
        }

        var files = await e.DataView.GetStorageItemsAsync();
        switch (files.Count)
        {
            case 0 or > 1:
                ShowMessage("You can only validate one file at a time.", Colors.Red);
                return;
            default:
                // validate the file
                ViewModel.ValidateFile(files[0].Path);
                break;
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

    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }
}