using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using Windows.Storage;
using CryptoTools.Extensions;
using CryptoTools.Models;

namespace CryptoTools.ViewModels;

public class ZipViewModel : ViewModelBase
{
    private readonly List<ArchiveEntry> _selectedEntries = new();
    public ObservableCollection<ArchiveEntry> ArchiveEntries { get; } = new();

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
        foreach (var entry in _selectedEntries) ArchiveEntries.Remove(entry);
        _selectedEntries.Clear();
    }

    public void AddFiles(IEnumerable<StorageFile> files)
    {
        foreach (var file in files)
            ArchiveEntries.Add(new ArchiveEntry
            {
                Name = file.Name,
                Path = file.Path
            });
    }

    public void AddDirectory(string directory)
    {
        ArchiveEntries.Add(new ArchiveEntry
        {
            Name = Path.GetFileName(directory),
            Path = directory,
            IsDirectory = true
        });
    }

    public void CompressArchive(string path)
    {
        using var fileStream = File.Create(path);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Create);
        foreach (var entry in ArchiveEntries)
            if (entry.IsDirectory)
                archive.CreateEntryFromDirectory(entry.Path, entry.Name, CompressionLevel);
            else
                archive.CreateEntryFromFile(entry.Path, entry.Name, CompressionLevel);
        ArchiveEntries.Clear();
    }

    public void DecompressArchive(string path)
    {
        using var fileStream = File.OpenRead(path);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
        var directory = Path.GetDirectoryName(path) ?? string.Empty;
        if (CreateNewFolder)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            directory = Path.Combine(directory, name);
        }

        archive.ExtractToDirectory(directory);
    }

    public void UpdateSelectedEntries(IEnumerable<ArchiveEntry> added, IEnumerable<ArchiveEntry> removed)
    {
        foreach (var entry in removed) _selectedEntries.Remove(entry);
        foreach (var entry in added) _selectedEntries.Add(entry);
    }
}