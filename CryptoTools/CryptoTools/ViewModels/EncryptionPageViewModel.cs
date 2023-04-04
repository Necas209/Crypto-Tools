using System;
using System.IO;
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
    private readonly string _rsaBlobFile = Path.Combine(AppFolder, "rsaBlob.bin");

    public DisplayMessageDelegate? DisplayMessage;

    public EncryptionPageViewModel()
    {
        SelectedAlgorithm = EncryptionAlgorithms.First();
        if (File.Exists(_rsaBlobFile))
            ImportParameters();
        else
            ExportParameters();
    }

    public EncryptionAlgorithm SelectedAlgorithm { get; set; }

    private void ImportParameters()
    {
        // Import the RSA parameters from a file
        using var fs = new FileStream(_rsaBlobFile, FileMode.Open);
        using var br = new BinaryReader(fs);
        var rsaBlob = new RSAParameters
        {
            Modulus = br.ReadBytes(br.ReadInt32()),
            Exponent = br.ReadBytes(br.ReadInt32()),
            D = br.ReadBytes(br.ReadInt32()),
            P = br.ReadBytes(br.ReadInt32()),
            Q = br.ReadBytes(br.ReadInt32()),
            DP = br.ReadBytes(br.ReadInt32()),
            DQ = br.ReadBytes(br.ReadInt32()),
            InverseQ = br.ReadBytes(br.ReadInt32())
        };
        _rsa.ImportParameters(rsaBlob);
    }

    private void ExportParameters()
    {
        // Export the RSA parameters to a file
        var rsaBlob = _rsa.ExportParameters(true);
        using var fs = new FileStream(_rsaBlobFile, FileMode.Create);
        using var bw = new BinaryWriter(fs);
        bw.Write(rsaBlob.Modulus?.Length ?? 0);
        bw.Write(rsaBlob.Modulus ?? Array.Empty<byte>());
        bw.Write(rsaBlob.Exponent?.Length ?? 0);
        bw.Write(rsaBlob.Exponent ?? Array.Empty<byte>());
        bw.Write(rsaBlob.D?.Length ?? 0);
        bw.Write(rsaBlob.D ?? Array.Empty<byte>());
        bw.Write(rsaBlob.P?.Length ?? 0);
        bw.Write(rsaBlob.P ?? Array.Empty<byte>());
        bw.Write(rsaBlob.Q?.Length ?? 0);
        bw.Write(rsaBlob.Q ?? Array.Empty<byte>());
        bw.Write(rsaBlob.DP?.Length ?? 0);
        bw.Write(rsaBlob.DP ?? Array.Empty<byte>());
        bw.Write(rsaBlob.DQ?.Length ?? 0);
        bw.Write(rsaBlob.DQ ?? Array.Empty<byte>());
        bw.Write(rsaBlob.InverseQ?.Length ?? 0);
        bw.Write(rsaBlob.InverseQ ?? Array.Empty<byte>());
    }

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