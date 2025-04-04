using System;
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

public partial class SignaturePage
{
    private readonly App _app = (App)Application.Current;
    private readonly DispatcherTimer _dispatcherTimer = new() { Interval = new TimeSpan(0, 0, 5) };

    public SignaturePage()
    {
        ViewModel = new SignatureViewModel { DisplayMessage = ShowMessage };
        InitializeComponent();
    }

    private SignatureViewModel ViewModel { get; }

    private void ShowMessage(string message, Color color)
    {
        if (_dispatcherTimer.IsEnabled) _dispatcherTimer.Stop();
        Message.Text = message;
        Message.Foreground = new SolidColorBrush(color);
        Message.Visibility = Visibility.Visible;
        _dispatcherTimer.Start();
    }

    private async void Sign_OnClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.Desktop,
            FileTypeFilter = { "*" }
        };
        InitializeWithWindow.Initialize(picker, _app.Handle);

        var file = await picker.PickSingleFileAsync();
        if (file is null) return;
        await ViewModel.SignFile(file.Path);
    }

    private async void Sign_OnDrop(object sender, DragEventArgs e)
    {
        BtSign.Background = new SolidColorBrush(Colors.Transparent);
        BtSign.BorderBrush = new SolidColorBrush(Colors.DimGray);

        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
            return;

        var items = await e.DataView.GetStorageItemsAsync();
        if (items is not [StorageFile file])
        {
            ShowMessage("You can only sign one file at a time.", Colors.Red);
            return;
        }

        await ViewModel.SignFile(file.Path);
    }

    private void Sign_OnDragEnter(object sender, DragEventArgs e)
    {
        BtSign.Background = new SolidColorBrush(Colors.LightSlateGray);
        BtSign.BorderBrush = new SolidColorBrush(Colors.DarkCyan);
    }

    private void Sign_OnDragLeave(object sender, DragEventArgs e)
    {
        BtSign.Background = new SolidColorBrush(Colors.Transparent);
        BtSign.BorderBrush = new SolidColorBrush(Colors.DimGray);
    }

    private async void Verify_OnClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.Desktop,
            FileTypeFilter = { "*" }
        };
        InitializeWithWindow.Initialize(picker, _app.Handle);

        var file = await picker.PickSingleFileAsync();
        if (file is null) return;
        await ViewModel.VerifySignature(file.Path);
    }

    private async void Verify_OnDrop(object sender, DragEventArgs e)
    {
        BtVerify.Background = new SolidColorBrush(Colors.Transparent);
        BtVerify.BorderBrush = new SolidColorBrush(Colors.DimGray);

        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
            return;

        var items = await e.DataView.GetStorageItemsAsync();
        if (items is not [StorageFile file])
        {
            ShowMessage("You can only validate one file at a time.", Colors.Red);
            return;
        }

        await ViewModel.VerifySignature(file.Path);
    }

    private void Verify_OnDragEnter(object sender, DragEventArgs e)
    {
        BtVerify.Background = new SolidColorBrush(Colors.LightSlateGray);
        BtVerify.BorderBrush = new SolidColorBrush(Colors.DarkCyan);
    }

    private void Verify_OnDragLeave(object sender, DragEventArgs e)
    {
        BtVerify.Background = new SolidColorBrush(Colors.Transparent);
        BtVerify.BorderBrush = new SolidColorBrush(Colors.DimGray);
    }

    private void OnDragOver(object sender, DragEventArgs e) => e.AcceptedOperation = DataPackageOperation.Copy;
}