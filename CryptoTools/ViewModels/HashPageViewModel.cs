using System.Collections.Generic;
using System.Linq;
using CryptoTools.Models;

namespace CryptoTools.ViewModels;

public class HashPageViewModel : BaseViewModel
{
    public HashPageViewModel()
    {
        HashingAlgorithms = Context.HashingAlgorithms.ToList();
    }

    public List<HashingAlgorithm> HashingAlgorithms { get; }
    public int HashingAlgorithmId { get; set; } = 1;
}