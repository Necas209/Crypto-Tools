using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CryptoLib.Models;
using CryptoTools.Utils;
using Microsoft.UI.Xaml.Media.Imaging;

namespace CryptoTools.ViewModels;

public class ImageEncryptionViewModel : ViewModelBase
{
    private BitmapImage? _encryptedImage;

    public ImageEncryptionViewModel()
    {
        SelectedAlgorithm = Algorithms[0];
        SelectedCipherMode = CipherMode.CBC;
    }

    public List<EncryptionAlgorithm> Algorithms => Model.EncryptionAlgorithms;

    public EncryptionAlgorithm SelectedAlgorithm { get; set; }

    public Dictionary<string, CipherMode> CipherModes { get; } = new()
    {
        { "CBC", CipherMode.CBC },
        { "CFB", CipherMode.CFB },
        { "CTS", CipherMode.CTS },
        { "ECB", CipherMode.ECB },
        { "OFB", CipherMode.OFB }
    };

    public CipherMode SelectedCipherMode { get; set; }

    public BitmapImage? EncryptedImage
    {
        get => _encryptedImage;
        set => SetField(ref _encryptedImage, value);
    }

    public async Task EncryptImage(string imagePath)
    {
        using var algorithm = EncryptionUtils.GetAlgorithm(SelectedAlgorithm.Name);
        algorithm.GenerateKey();
        algorithm.GenerateIV();
        algorithm.Mode = SelectedCipherMode;
        using var bmp = new Bitmap(imagePath);
        // Extract the pixel data from the bitmap
        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        var bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
        // Calculate the number of bytes needed for the pixelData array
        var bytesPerPixel = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
        var dataSize = bmp.Width * bmp.Height * bytesPerPixel;
        // Copy the pixel data into the pixelData array
        var pixelData = new byte[dataSize];
        Marshal.Copy(bmpData.Scan0, pixelData, 0, dataSize);
        // Encrypt the pixel data
        using var encryptor = algorithm.CreateEncryptor();
        var encryptedPixelData = encryptor.TransformFinalBlock(pixelData, 0, dataSize);
        // Copy the encrypted pixel data back into the bitmap
        Marshal.Copy(encryptedPixelData, 0, bmpData.Scan0, dataSize);
        bmp.UnlockBits(bmpData);
        EncryptedImage = await BitmapUtils.ToBitmapImage(bmp);
    }
}