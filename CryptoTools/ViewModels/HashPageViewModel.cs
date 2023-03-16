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
}