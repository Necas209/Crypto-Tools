using System;
using System.Linq;
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
        if (_timer.IsRunning)
            _timer.Stop();
        Message.Text = message;
        Message.Foreground = new SolidColorBrush(color);
        Message.Visibility = Visibility.Visible;
        _timer.Start();
    }

    private void TextToHash_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        ViewModel.PlainText = TbTextToHash.Text;
        ViewModel.HashText();
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
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
        if (file is null)
            return;

        ViewModel.HashFile(file.Path);
    }

    private void File_OnDragEnter(object sender, DragEventArgs e)
    {
        BtHash.Background = new SolidColorBrush(Colors.LightSlateGray);
        BtHash.BorderBrush = new SolidColorBrush(Colors.DarkCyan);
    }

    private void File_OnDragLeave(object sender, DragEventArgs e)
    {
        BtHash.Background = new SolidColorBrush(Colors.Transparent);
        BtHash.BorderBrush = new SolidColorBrush(Colors.DimGray);
    }

    private void File_OnDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private async void File_OnDrop(object sender, DragEventArgs e)
    {
        BtHash.Background = new SolidColorBrush(Colors.Transparent);
        BtHash.BorderBrush = new SolidColorBrush(Colors.DimGray);

        if (!e.DataView.Contains(StandardDataFormats.StorageItems)) return;

        var items = await e.DataView.GetStorageItemsAsync();
        if (items.FirstOrDefault() is not StorageFile file)
        {
            ShowMessage("Please drop only one file!", Colors.Red);
            return;
        }

        ViewModel.HashFile(file.Path);
    }
}