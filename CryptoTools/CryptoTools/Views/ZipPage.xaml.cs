using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage.Pickers;
using CryptoTools.Models;
using CryptoTools.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinRT.Interop;

namespace CryptoTools.Views;

public partial class ZipPage
{
    private readonly App _app = (App)Application.Current;

    public ZipPage()
    {
        InitializeComponent();
    }

    public ZipViewModel ViewModel { get; } = new();

    private async void BtnZip_OnClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.Desktop,
            SuggestedFileName = "Archive",
            DefaultFileExtension = ".zip",
            FileTypeChoices =
            {
                { "Zip files", new List<string> { ".zip" } }
            }
        };
        InitializeWithWindow.Initialize(picker, _app.Hwnd);
        var result = await picker.PickSaveFileAsync();
        if (result == null) return;
        ViewModel.CompressArchive(result.Path);
    }

    private async void BtnUnzip_OnClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.Desktop,
            FileTypeFilter = { ".zip" }
        };
        InitializeWithWindow.Initialize(picker, _app.Hwnd);
        var result = await picker.PickSingleFileAsync();
        if (result == null) return;
        ViewModel.DecompressArchive(result.Path);
    }

    private void BtnRemove_OnClick(object sender, RoutedEventArgs e)
    {
        ViewModel.RemoveSelectedEntries();
    }

    private async void BtnAddDir_OnClick(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker
        {
            SuggestedStartLocation = PickerLocationId.Desktop,
            ViewMode = PickerViewMode.List,
            FileTypeFilter = { "*" }
        };
        InitializeWithWindow.Initialize(picker, _app.Hwnd);
        var folder = await picker.PickSingleFolderAsync();
        if (folder == null) return;
        ViewModel.AddDirectory(folder.Path);
    }

    private async void BtnAddFile_OnClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.Desktop,
            FileTypeFilter = { "*" }
        };
        InitializeWithWindow.Initialize(picker, _app.Hwnd);
        var files = await picker.PickMultipleFilesAsync();
        if (files.Count == 0) return;
        ViewModel.AddFiles(files);
    }

    private void ListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0 && e.RemovedItems.Count == 0) return;
        var added = e.AddedItems.Cast<ArchiveEntry>();
        var removed = e.RemovedItems.Cast<ArchiveEntry>();
        ViewModel.UpdateSelectedEntries(added, removed);
    }
}