using System.IO;
using System.IO.Compression;
using System.Linq;

namespace CryptoTools.Extensions;

public static class ZipArchiveExtension
{
    private static void CreateEntryFromAny(this ZipArchive archive, string sourceName, string entryName = "",
        CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        var fileName = Path.GetFileName(sourceName);
        if (File.GetAttributes(sourceName).HasFlag(FileAttributes.Directory))
            archive.CreateEntryFromDirectory(sourceName, Path.Combine(entryName, fileName));
        else
            archive.CreateEntryFromFile(sourceName, Path.Combine(entryName, fileName), compressionLevel);
    }

    public static void CreateEntryFromDirectory(this ZipArchive archive, string sourceDirName, string entryName = "",
        CompressionLevel compressionLevel = CompressionLevel.Optimal)
    {
        var files = Directory.GetFiles(sourceDirName);
        var directories = Directory.GetDirectories(sourceDirName);
        var allFiles = files.Concat(directories);
        foreach (var file in allFiles) archive.CreateEntryFromAny(file, entryName, compressionLevel);
    }
}