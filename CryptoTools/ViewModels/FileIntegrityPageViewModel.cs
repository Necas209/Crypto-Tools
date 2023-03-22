using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using CryptoTools.Models;
using CryptoTools.Services;
using Microsoft.EntityFrameworkCore;

namespace CryptoTools.ViewModels;

public class FileIntegrityPageViewModel : BaseViewModel
{
    public delegate void DisplayMessageDelegate(string message, Color color);

    public FileIntegrityPageViewModel()
    {
        HashingAlgorithms = Context.HashingAlgorithms.ToList();
        Algorithm = HashingAlgorithms.First();
    }

    public List<HashingAlgorithm> HashingAlgorithms { get; }

    public HashingAlgorithm Algorithm { get; set; }

    public event DisplayMessageDelegate? DisplayMessage;

    private bool RegisterFile(string file)
    {
        if (!File.Exists(file)) return false;
        var fileName = Path.GetFileName(file);
        var hashEntry = Context.HashEntries.FirstOrDefault(x => x.FileName == fileName);
        if (hashEntry is null)
        {
            var hash = HashingService.GetFileHash(file, Algorithm.Name);
            var fileIntegrity = new HashEntry
            {
                FileName = fileName,
                Hash = hash,
                HashingAlgorithmId = Algorithm.Id
            };
            Context.HashEntries.Add(fileIntegrity);
        }
        else
        {
            var hash = HashingService.GetFileHash(file, Algorithm.Name);
            hashEntry.Hash = hash;
            hashEntry.HashingAlgorithmId = Algorithm.Id;
            Context.HashEntries.Update(hashEntry);
        }

        return true;
    }

    public void RegisterFiles(IEnumerable<string> files)
    {
        var allFilesRegistered = files.Aggregate(true, (current, file) => current & RegisterFile(file));
        if (!allFilesRegistered)
            DisplayMessage?.Invoke("Some files could not be registered!", Colors.Coral);
        else
            DisplayMessage?.Invoke("File(s) registered successfully.", Colors.Green);
        Context.SaveChanges();
    }

    public void ValidateFile(string file)
    {
        var fileName = Path.GetFileName(file);
        var hashEntry = Context.HashEntries
            .Include(x => x.HashingAlgorithm)
            .FirstOrDefault(x => x.FileName == fileName);
        if (hashEntry is null)
        {
            DisplayMessage?.Invoke("File could not be found!", Colors.Coral);
            return;
        }

        var hash = HashingService.GetFileHash(file, hashEntry.HashingAlgorithm?.Name ?? string.Empty);
        if (hashEntry.Hash == hash)
            DisplayMessage?.Invoke("File is valid.", Colors.Green);
        else
            DisplayMessage?.Invoke("File is invalid!", Colors.Red);
    }
}