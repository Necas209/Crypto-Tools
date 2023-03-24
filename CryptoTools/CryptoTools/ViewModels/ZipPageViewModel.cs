using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using CryptoTools.Services;

namespace CryptoTools.ViewModels;

public class ZipPageViewModel : BaseViewModel
{
    public readonly record struct ArchiveEntry(string Name, string Path, bool IsDirectory);

    public ObservableCollection<ArchiveEntry> ArchiveEntries { get; set; } = new();

    public List<ArchiveEntry> SelectedEntries
    {
        get => _selectedEntries;
        set => SetField(ref _selectedEntries, value);
    }

    private List<ArchiveEntry> _selectedEntries = new();

    public void RemoveSelectedEntries()
    {
        foreach (var entry in SelectedEntries)
        {
            ArchiveEntries.Remove(entry);
        }

        SelectedEntries.Clear();
    }

    public void AddEntries(IEnumerable<string> entries, bool isDirectory = false)
    {
        foreach (var entry in entries)
        {
            ArchiveEntries.Add(new ArchiveEntry(Path.GetFileName(entry), entry, isDirectory));
        }
    }

    public void CompressEntries(string fileName)
    {
        using (var fileStream = File.Create(fileName))
        {
            using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
            {
                foreach (var entry in ArchiveEntries)
                {
                    if (entry.IsDirectory)
                        archive.CreateEntryFromDirectory(entry.Path, entry.Name);
                    else
                        archive.CreateEntryFromFile(entry.Path, entry.Name, CompressionLevel.Fastest);
                }
            }
        }

        ArchiveEntries.Clear();
    }

    public static void DecompressArchive(string dialogFileName)
    {
        using var fileStream = File.OpenRead(dialogFileName);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
        foreach (var entry in archive.Entries)
        {
            var entryPath = Path.Combine(Path.GetDirectoryName(dialogFileName) ?? string.Empty, entry.FullName);
            var directory = Path.GetDirectoryName(entryPath);

            if (directory != null && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            entry.ExtractToFile(entryPath, true);
        }
    }
}