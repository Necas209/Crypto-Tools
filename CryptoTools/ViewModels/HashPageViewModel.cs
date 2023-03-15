using System.Collections.Generic;
using System.Linq;
using CryptoTools.Models;

namespace CryptoTools.ViewModels;

public class HashPageViewModel : BaseViewModel
{
    public List<HashingAlgorithm> HashingAlgorithms { get; set; }
    
    public HashPageViewModel()
    {
        HashingAlgorithms = Context.HashingAlgorithms.ToList();
    }
}