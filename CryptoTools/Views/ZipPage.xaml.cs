using System;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CryptoTools.ViewModels;
using Microsoft.Win32;

namespace CryptoTools.Views;

public partial class ZipPage
{
    private readonly ZipPageViewModel _viewModel;
    private readonly string _dialogPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    
    public ZipPage()
    {
        InitializeComponent();

        _viewModel = (ZipPageViewModel) DataContext;
    }

    private void BtnOpenFile_OnClick(object sender, RoutedEventArgs e)
    {
        
        var dialog = new CommonOpenFileDialog()
        {
            Multiselect = true,
            InitialDirectory = $"{_dialogPath}\\Desktop",
            EnsureFileExists = true,
            IsFolderPicker = true
        };
        
        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            _viewModel.AddEntries(dialog.FileNames, true);
        
        dialog.IsFolderPicker = false;
        dialog.InitialDirectory =  $"{_dialogPath}\\Desktop";
        
        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            _viewModel.AddEntries(dialog.FileNames);

    }

    private void BtnZip_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog()
        {
            InitialDirectory = $"{_dialogPath}\\Desktop",
            Filter = "Zip files (*.zip)|*.zip",
            FilterIndex = 1,
            RestoreDirectory = true
        };
        
        if (dialog.ShowDialog() == true)
            _viewModel.CompressEntries(dialog.FileName);
    }
    
    private void BtnUnzip_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new CommonOpenFileDialog()
        {
            InitialDirectory = $"{_dialogPath}\\Desktop",
            RestoreDirectory = true,
            IsFolderPicker = false,
            Multiselect = false,
            EnsureFileExists = true,
            Title = "Select a zip file to extract",
            DefaultExtension = "zip",
            Filters =
            {
                new CommonFileDialogFilter("Zip files", "*.zip"),
                new CommonFileDialogFilter("All files", "*.*")
            }
        };
        
        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            ZipPageViewModel.DecompressArchive(dialog.FileName);
    }

    private void BtnRem_OnClick(object sender, RoutedEventArgs e)
    {
        _viewModel.RemoveSelectedEntries();
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _viewModel.SelectedEntries = Files.SelectedItems
            .Cast<ZipPageViewModel.ArchiveEntry>().ToList();
    }
}