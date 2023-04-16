using System.Linq;
using CryptoLib.Models;
using CryptoTools.Utils;

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

        var hash = HashingUtils.Hash(UnhashedText, SelectedAlgorithm.Name);
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