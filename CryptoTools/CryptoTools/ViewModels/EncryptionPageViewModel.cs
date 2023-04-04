using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Media;
using CryptoLib.Models;
using CryptoLib.Services;

namespace CryptoTools.ViewModels;

public class EncryptionPageViewModel : BaseViewModel
{
    public delegate void DisplayMessageDelegate(string message, Color color);

    private readonly RSA _rsa = RSA.Create();

    public EncryptionPageViewModel()
    {
        EncryptionAlgorithms = Context.EncryptionAlgorithms.ToList();
        Algorithm = EncryptionAlgorithms.First();
    }

    public List<EncryptionAlgorithm> EncryptionAlgorithms { get; }

    public EncryptionAlgorithm Algorithm { get; set; }

    public event DisplayMessageDelegate? DisplayMessage;

    public void EncryptFile(string path)
    {
        EncryptionService.EncryptFile(path, Algorithm.Name, _rsa.ExportParameters(false));
        DisplayMessage?.Invoke("File encrypted successfully.", Colors.Green);
    }

    public void DecryptFile(string path)
    {
        EncryptionService.DecryptFile(path, Algorithm.Name, _rsa.ExportParameters(true));
        DisplayMessage?.Invoke("File decrypted successfully.", Colors.Green);
    }
}