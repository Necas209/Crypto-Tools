using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using CryptoLib.Extensions;

namespace CryptoTools.ViewModels;

public class ZipViewModel : ViewModelBase
{
    private List<ArchiveEntry> _selectedEntries = new();

    public ObservableCollection<ArchiveEntry> ArchiveEntries { get; } = new();

    public List<ArchiveEntry> SelectedEntries
    {
        get => _selectedEntries;
        set => SetField(ref _selectedEntries, value);
    }

    public void RemoveSelectedEntries()
    {
        foreach (var entry in SelectedEntries) ArchiveEntries.Remove(entry);

        SelectedEntries.Clear();
    }

    public void AddFiles(IEnumerable<string> files)
    {
        foreach (var file in files)
            ArchiveEntries.Add(new ArchiveEntry
            {
                Name = Path.GetFileName(file),
                Path = file
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
                archive.CreateEntryFromDirectory(entry.Path, entry.Name);
            else
                archive.CreateEntryFromFile(entry.Path, entry.Name, CompressionLevel.Optimal);
        ArchiveEntries.Clear();
    }

    public static void DecompressArchive(string path)
    {
        using var fileStream = File.OpenRead(path);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
        var directory = Path.GetDirectoryName(path) ?? string.Empty;
        var directoryName = Path.GetFileNameWithoutExtension(path);
        var archiveDirectory = Path.Combine(directory, directoryName);
        archive.ExtractToDirectory(archiveDirectory);
    }

    public readonly record struct ArchiveEntry(string Name, string Path, bool IsDirectory);
}