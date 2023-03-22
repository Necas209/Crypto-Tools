using System.Collections.Generic;
using System.Linq;
using CryptoTools.Models;
using CryptoTools.Services;

namespace CryptoTools.ViewModels;

public class HashPageViewModel : BaseViewModel
{
    public HashPageViewModel()
    {
        HashingAlgorithms = Context.HashingAlgorithms.ToList();

        Algorithm = HashingAlgorithms.First();
    }

    public List<HashingAlgorithm> HashingAlgorithms { get; }
    public string UnhashedText { get; set; } = string.Empty;
    private string _hashedText = string.Empty;

    public string HashedText
    {
        get => _hashedText;
        set => SetField(ref _hashedText, value);
    }

    private string _hashedFile = string.Empty;

    public string HashedFile
    {
        get => _hashedFile;
        set => SetField(ref _hashedFile, value);
    }

    public HashingAlgorithm Algorithm { get; set; }


    public void HashText()
    {
        if (string.IsNullOrEmpty(UnhashedText))
        {
            HashedText = string.Empty;
            return;
        }

        HashedText = HashingService.GetHash(UnhashedText, Algorithm.Name);
    }

    public void HashFile(string file)
    {
        if (string.IsNullOrEmpty(file))
        {
            HashedFile = string.Empty;
            return;
        }

        HashedFile = HashingService.GetFileHash(file, Algorithm.Name);
    }
}