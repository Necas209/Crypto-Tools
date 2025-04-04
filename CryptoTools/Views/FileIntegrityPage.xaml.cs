using System;
using System.Linq;
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

public partial class FileIntegrityPage
{
    private readonly App _app = (App)Application.Current;

    private readonly DispatcherTimer _dispatcherTimer = new() { Interval = new TimeSpan(0, 0, 5) };

    public FileIntegrityPage()
    {
        ViewModel = new FileIntegrityViewModel { DisplayMessage = ShowMessage };
        InitializeComponent();
        _dispatcherTimer.Tick += (_, _) =>
        {
            TbMessage.Visibility = Visibility.Collapsed;
            _dispatcherTimer.Stop();
        };
    }

    public FileIntegrityViewModel ViewModel { get; }

    private void ShowMessage(string message, Color color)
    {
        if (_dispatcherTimer.IsEnabled) _dispatcherTimer.Stop();
        TbMessage.Text = message;
        TbMessage.Foreground = new SolidColorBrush(color);
        TbMessage.Visibility = Visibility.Visible;
        _dispatcherTimer.Start();
    }

    private async void Register_OnClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.Desktop,
            FileTypeFilter = { "*" }
        };
        InitializeWithWindow.Initialize(picker, _app.Handle);

        var files = await picker.PickMultipleFilesAsync();
        if (files.Count == 0) return;
        await ViewModel.RegisterFiles(files);
    }

    private async void Register_OnDrop(object sender, DragEventArgs e)
    {
        BtRegister.Background = new SolidColorBrush(Colors.Transparent);
        BtRegister.BorderBrush = new SolidColorBrush(Colors.DimGray);

        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            if (_dispatcherTimer.IsEnabled) _dispatcherTimer.Stop();
            ShowMessage("This is not a file", Colors.Red);
            return;
        }

        var items = await e.DataView.GetStorageItemsAsync();
        if (items.Count == 0) return;

        var files = items.OfType<StorageFile>();
        await ViewModel.RegisterFiles(files);
    }

    private void Register_OnDragEnter(object sender, DragEventArgs e)
    {
        BtRegister.Background = new SolidColorBrush(Colors.LightSlateGray);
        BtRegister.BorderBrush = new SolidColorBrush(Colors.DarkCyan);
    }

    private void Register_OnDragLeave(object sender, DragEventArgs e)
    {
        BtRegister.Background = new SolidColorBrush(Colors.Transparent);
        BtRegister.BorderBrush = new SolidColorBrush(Colors.DimGray);
    }

    private async void Validation_OnClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.Desktop,
            FileTypeFilter = { "*" }
        };
        InitializeWithWindow.Initialize(picker, _app.Handle);

        var file = await picker.PickSingleFileAsync();
        if (file is null)
            return;

        await ViewModel.ValidateFile(file.Path);
    }

    private async void Validation_OnDrop(object sender, DragEventArgs e)
    {
        BtValidate.Background = new SolidColorBrush(Colors.Transparent);
        BtValidate.BorderBrush = new SolidColorBrush(Colors.DimGray);

        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            ShowMessage("This is not a file!", Colors.Red);
            return;
        }

        var files = await e.DataView.GetStorageItemsAsync();
        if (files is not [StorageFile file])
        {
            ShowMessage("You can only validate one file at a time.", Colors.Red);
            return;
        }

        await ViewModel.ValidateFile(file.Path);
    }

    private void Validation_OnDragEnter(object sender, DragEventArgs e)
    {
        BtValidate.Background = new SolidColorBrush(Colors.LightSlateGray);
        BtValidate.BorderBrush = new SolidColorBrush(Colors.DarkCyan);
    }

    private void Validation_OnDragLeave(object sender, DragEventArgs e)
    {
        BtValidate.Background = new SolidColorBrush(Colors.Transparent);
        BtValidate.BorderBrush = new SolidColorBrush(Colors.DimGray);
    }

    private void OnDragOver(object sender, DragEventArgs e) => e.AcceptedOperation = DataPackageOperation.Copy;
}