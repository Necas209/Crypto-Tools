using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using CryptoTools.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinRT.Interop;

namespace CryptoTools.Views;

public partial class HashPage
{
    private readonly App _app = (App)Application.Current;

    private readonly DispatcherQueueTimer _timer;

    public HashPage()
    {
        InitializeComponent();
        _timer = DispatcherQueue.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(5);
        _timer.Tick += (_, _) =>
        {
            Message.Visibility = Visibility.Collapsed;
            _timer.Stop();
        };
    }

    public HashViewModel ViewModel { get; } = new();

    private void ShowMessage(string message, Color color)
    {
        if (_timer.IsRunning) _timer.Stop();
        Message.Text = message;
        // Change the color of the text
        Message.Foreground = new SolidColorBrush(color);
        // Timer to change the visibility of the text
        Message.Visibility = Visibility.Visible;
        _timer.Start();
    }

    private void TextToHash_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.PlainText = ((TextBox)sender).Text;
        ViewModel.HashText();
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel?.HashText();
    }

    private async void File_OnClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.Desktop,
            FileTypeFilter = { "*" }
        };
        InitializeWithWindow.Initialize(picker, _app.Hwnd);
        var file = await picker.PickSingleFileAsync();
        if (file == null) return;
        ViewModel.HashFile(file.Path);
    }

    private void File_OnDragEnter(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;

        btn.Background = new SolidColorBrush(Colors.LightSlateGray);
        btn.BorderBrush = new SolidColorBrush(Colors.DarkCyan);
    }

    private void File_OnDragLeave(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;

        btn.Background = new SolidColorBrush(Colors.Transparent);
        btn.BorderBrush = new SolidColorBrush(Colors.DimGray);
    }

    private void File_OnDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private async void File_OnDrop(object sender, DragEventArgs e)
    {
        if (sender is not Button btn) return;

        btn.Background = new SolidColorBrush(Colors.Transparent);
        btn.BorderBrush = new SolidColorBrush(Colors.DimGray);

        var deferral = e.GetDeferral();
        var dataPackageView = e.DataView;
        if (!dataPackageView.Contains(StandardDataFormats.StorageItems)) return;

        var items = await dataPackageView.GetStorageItemsAsync();
        if (items.Count == 1)
        {
            if (items[0] is StorageFile file) ViewModel.HashFile(file.Path);
        }
        else
        {
            ShowMessage("Please drop only one file!", Colors.Red);
        }

        deferral.Complete();
    }
}