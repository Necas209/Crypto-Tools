using System.Linq;
using CryptoLib.Models;
using CryptoLib.Services;

namespace CryptoTools.ViewModels;

public class HashViewModel : ViewModelBase
{
    private string _hashedFile = string.Empty;
    private string _hashedText = string.Empty;

    public HashViewModel()
    {
        SelectedAlgorithm = Model.HashingAlgorithms.First();
    }

    public string UnhashedText { get; set; } = string.Empty;

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

    public HashingAlgorithm SelectedAlgorithm { get; set; }


    public void HashText()
    {
        if (string.IsNullOrEmpty(UnhashedText))
        {
            HashedText = string.Empty;
            return;
        }

        HashedText = HashingService.GetHash(UnhashedText, SelectedAlgorithm.Name);
    }

    public void HashFile(string file)
    {
        if (string.IsNullOrEmpty(file))
        {
            HashedFile = string.Empty;
            return;
        }

        HashedFile = HashingService.GetFileHash(file, SelectedAlgorithm.Name);
    }
}