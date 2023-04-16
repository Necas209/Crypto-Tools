using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media;
using CryptoLib.Models;
using CryptoTools.Utils;

namespace CryptoTools.ViewModels;

public class EncryptionViewModel : ViewModelBase
{
    public delegate void DisplayMessageDelegate(string message, Color color);

    private readonly RSA _rsa = RSA.Create();

    public DisplayMessageDelegate? DisplayMessage;

    public EncryptionViewModel()
    {
        SelectedAlgorithm = Model.EncryptionAlgorithms.First();
        var rsaBin = Path.Combine(Model.AppFolder, "rsa.bin");
        if (File.Exists(rsaBin))
        {
            var xmlString = Encoding.UTF8.GetString(File.ReadAllBytes(rsaBin));
            _rsa.FromXmlString(xmlString);
        }
        else
        {
            var xmlString = _rsa.ToXmlString(true);
            File.WriteAllBytes(rsaBin, Encoding.UTF8.GetBytes(xmlString));
        }
    }

    public EncryptionAlgorithm SelectedAlgorithm { get; set; }

    public void EncryptFile(string path)
    {
        EncryptionUtils.EncryptFile(path, SelectedAlgorithm.Name, _rsa.ExportParameters(false));
        DisplayMessage?.Invoke("File encrypted successfully.", Colors.Green);
    }

    public void DecryptFile(string path)
    {
        try
        {
            EncryptionUtils.DecryptFile(path, SelectedAlgorithm.Name, _rsa.ExportParameters(true));
            DisplayMessage?.Invoke("File decrypted successfully.", Colors.Green);
        }
        catch (CryptographicException)
        {
            DisplayMessage?.Invoke("File decryption failed.", Colors.Red);
        }
    }
}