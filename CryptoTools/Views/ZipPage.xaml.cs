using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
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
        ViewModel = new ZipViewModel();
        InitializeComponent();
    }

    public ZipViewModel ViewModel { get; }

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
        InitializeWithWindow.Initialize(picker, _app.Handle);

        var file = await picker.PickSaveFileAsync();
        if (file is null) return;
        await ViewModel.CompressArchive(file);
    }

    private async void BtnUnzip_OnClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.Desktop,
            FileTypeFilter = { ".zip" }
        };
        InitializeWithWindow.Initialize(picker, _app.Handle);

        var file = await picker.PickSingleFileAsync();
        StorageFolder folder;
        if (ViewModel.CreateNewFolder)
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop,
                ViewMode = PickerViewMode.List,
                FileTypeFilter = { "*" }
            };
            InitializeWithWindow.Initialize(folderPicker, _app.Handle);

            folder = await folderPicker.PickSingleFolderAsync();
            if (folder is null) return;
        }
        else
        {
            folder = await file.GetParentAsync();
        }

        await ZipViewModel.DecompressArchive(file, folder);
    }

    private void BtRemove_OnClick(object sender, RoutedEventArgs e) => ViewModel.RemoveSelectedEntries();

    private async void BtAddDir_OnClick(object sender, RoutedEventArgs e)
    {
        var picker = new FolderPicker
        {
            SuggestedStartLocation = PickerLocationId.Desktop,
            ViewMode = PickerViewMode.List,
            FileTypeFilter = { "*" }
        };
        InitializeWithWindow.Initialize(picker, _app.Handle);

        var folder = await picker.PickSingleFolderAsync();
        if (folder is null) return;
        ViewModel.AddDirectory(folder);
    }

    private async void BtAddFile_OnClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.Desktop,
            FileTypeFilter = { "*" }
        };
        InitializeWithWindow.Initialize(picker, _app.Handle);

        var files = await picker.PickMultipleFilesAsync();
        if (files.Count == 0) return;
        ViewModel.AddFiles(files);
    }

    private void ListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0 && e.RemovedItems.Count == 0) return;
        var added = e.AddedItems.Cast<IStorageItem>();
        var removed = e.RemovedItems.Cast<IStorageItem>();
        ViewModel.UpdateSelectedEntries(added, removed);
    }
}