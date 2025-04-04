using System.Collections.Generic;
using CryptoLib.Models;
using CryptoTools.Utils;

namespace CryptoTools.ViewModels;

public class HashViewModel : ViewModelBase
{
    private string _hashedFile = string.Empty;
    private string _hashedText = string.Empty;

    public HashViewModel() => SelectedAlgorithm = Algorithms[0];

    public string PlainText { get; set; } = string.Empty;

    public string HashedText
    {
        get => _hashedText;
        set => SetField(ref _hashedText, value);
    }

    public string HashedFile
    {
        get => _hashedFile;
        set => SetField(ref _hashedFile, value);
    }

    public List<HashingAlgorithm> Algorithms => Model.HashingAlgorithms;

    public HashingAlgorithm SelectedAlgorithm { get; set; }


    public void HashText()
    {
        if (string.IsNullOrEmpty(PlainText))
        {
            HashedText = string.Empty;
            return;
        }

        var hash = HashingUtils.Hash(PlainText, SelectedAlgorithm.Name);
        HashedText = HashingUtils.ToHexString(hash);
    }

    public void HashFile(string file)
    {
        if (string.IsNullOrEmpty(file))
        {
            HashedFile = string.Empty;
            return;
        }

        var hash = HashingUtils.HashFile(file, SelectedAlgorithm.Name);
        HashedFile = HashingUtils.ToHexString(hash);
    }
}