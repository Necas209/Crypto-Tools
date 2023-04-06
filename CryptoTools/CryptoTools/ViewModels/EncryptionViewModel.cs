using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media;
using CryptoLib.Models;
using CryptoLib.Services;

namespace CryptoTools.ViewModels;

public class EncryptionViewModel : ViewModelBase
{
    public delegate void DisplayMessageDelegate(string message, Color color);

    private readonly RSA _rsa = RSA.Create();

    public DisplayMessageDelegate? DisplayMessage;

    public EncryptionViewModel()
    {
        SelectedAlgorithm = Model.EncryptionAlgorithms.First();
        var rsaBin = Path.Combine(AppFolder, "enc.bin");
        if (File.Exists(rsaBin))
        {
            var xmlString = Encoding.UTF8.GetString(File.ReadAllBytes(rsaBin));
            _rsa.FromXmlString(xmlString);
        }
        else
        {
            var parameters = _rsa.ExportParameters(true);
            _rsa.ImportParameters(parameters);
        }
    }

    public EncryptionAlgorithm SelectedAlgorithm { get; set; }

    public void EncryptFile(string path)
    {
        EncryptionService.EncryptFile(path, SelectedAlgorithm.Name, _rsa.ExportParameters(false));
        DisplayMessage?.Invoke("File encrypted successfully.", Colors.Green);
    }

    public void DecryptFile(string path)
    {
        var result = EncryptionService.DecryptFile(path, SelectedAlgorithm.Name, _rsa.ExportParameters(true));
        if (result)
            DisplayMessage?.Invoke("File decrypted successfully.", Colors.Green);
        else
            DisplayMessage?.Invoke("File decryption failed.", Colors.Red);
    }
}