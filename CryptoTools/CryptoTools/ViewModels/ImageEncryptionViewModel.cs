using System.Linq;
using CryptoLib.Models;
using CryptoTools.Services;

namespace CryptoTools.ViewModels;

public class ImageEncryptionViewModel : ViewModelBase
{
    public ImageEncryptionViewModel()
    {
        SelectedAlgorithm = Model.EncryptionAlgorithms.First();
    }

    public EncryptionAlgorithm SelectedAlgorithm { get; set; }

    public string EncryptImage(string imagePath)
    {
        return EncryptionService.EncryptImage(imagePath, SelectedAlgorithm.Name);
    }
}