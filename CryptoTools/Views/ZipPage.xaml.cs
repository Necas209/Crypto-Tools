using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CryptoTools.ViewModels;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace CryptoTools.Views;

public partial class ZipPage
{
    private readonly string _desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    private readonly ZipViewModel _viewModel;

    public ZipPage()
    {
        InitializeComponent();
        _viewModel = (ZipViewModel)DataContext;
    }

    private void BtnZip_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            InitialDirectory = _desktopPath,
            Filter = "Zip files (*.zip)|*.zip",
            RestoreDirectory = true,
            Title = "Select a zip file to create",
            DefaultExt = "zip"
        };

        if (dialog.ShowDialog() == true)
            _viewModel.CompressArchive(dialog.FileName);
    }

    private void BtnUnzip_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            InitialDirectory = _desktopPath,
            Filter = "Zip files (*.zip)|*.zip",
            RestoreDirectory = true,
            Title = "Select a zip file to extract",
            DefaultExt = "zip"
        };

        if (dialog.ShowDialog() == true)
            _viewModel.DecompressArchive(dialog.FileName);
    }

    private void BtnRemove_OnClick(object sender, RoutedEventArgs e)
    {
        _viewModel.RemoveSelectedEntries();
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _viewModel.SelectedEntries = Files.SelectedItems
            .Cast<ZipViewModel.ArchiveEntry>()
            .ToList();
    }

    private void BtnAddDir_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new CommonOpenFileDialog
        {
            InitialDirectory = _desktopPath,
            IsFolderPicker = true
        };

        if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;
        if (!string.IsNullOrEmpty(dialog.FileName))
            _viewModel.AddDirectory(dialog.FileName);
    }

    private void BtnAddFile_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            InitialDirectory = _desktopPath,
            Multiselect = true,
            Filter = "All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() == true)
            _viewModel.AddFiles(dialog.FileNames);
    }
}