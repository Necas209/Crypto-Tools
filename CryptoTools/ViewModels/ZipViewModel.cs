using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using CryptoTools.Extensions;
using FileAttributes = Windows.Storage.FileAttributes;

namespace CryptoTools.ViewModels;

public class ZipViewModel : ViewModelBase
{
    public ObservableCollection<IStorageItem> SelectedItems { get; } = [];

    public ObservableCollection<IStorageItem> Items { get; } = [];

    public Dictionary<string, CompressionLevel> CompressionLevels { get; } = new()
    {
        { "No compression", CompressionLevel.NoCompression },
        { "Fastest", CompressionLevel.Fastest },
        { "Optimal", CompressionLevel.Optimal },
        { "Smallest size", CompressionLevel.SmallestSize }
    };

    public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Optimal;

    public bool CreateNewFolder { get; set; } = true;

    public void RemoveSelectedEntries()
    {
        var entries = SelectedItems.ToList();
        foreach (var entry in entries)
            Items.Remove(entry);
    }

    public void AddFiles(IEnumerable<StorageFile> files)
    {
        foreach (var file in files)
            Items.Add(file);
    }

    public void AddDirectory(StorageFolder folder)
    {
        Items.Add(folder);
    }

    public async Task CompressArchive(StorageFile file)
    {
        await using var fs = await file.OpenStreamForWriteAsync();
        using var archive = new ZipArchive(fs, ZipArchiveMode.Create);
        foreach (var item in Items)
            if (item.Attributes.HasFlag(FileAttributes.Directory))
                archive.CreateEntryFromDirectory(item.Path, item.Name, CompressionLevel);
            else
                archive.CreateEntryFromFile(item.Path, item.Name, CompressionLevel);
        Items.Clear();
    }

    public static async Task DecompressArchive(StorageFile file, StorageFolder folder)
    {
        await using var fs = await file.OpenStreamForReadAsync();
        using var archive = new ZipArchive(fs, ZipArchiveMode.Read);
        archive.ExtractToDirectory(folder.Path);
    }

    public void UpdateSelectedEntries(IEnumerable<IStorageItem> added, IEnumerable<IStorageItem> removed)
    {
        foreach (var entry in added)
            SelectedItems.Add(entry);
        foreach (var entry in removed)
            SelectedItems.Remove(entry);
    }
}