using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using CryptoLib.Models;
using CryptoTools.Utils;

namespace CryptoTools.ViewModels;

public class ImageEncryptionViewModel : ViewModelBase
{
    public ImageEncryptionViewModel()
    {
        SelectedAlgorithm = Model.EncryptionAlgorithms.First();
    }

    public EncryptionAlgorithm SelectedAlgorithm { get; set; }

    public Bitmap EncryptImage(string imagePath)
    {
        using var algorithm = EncryptionUtils.GetAlgorithm(SelectedAlgorithm.Name);
        // Set the encryption key and generate an initialization vector
        algorithm.GenerateKey();
        algorithm.GenerateIV();
        // Convert the image to grayscale
        var grayscale = BitmapUtils.ToGrayscale(imagePath);
        // Extract the pixel data from the grayscale image
        var pixelData = new byte[grayscale.PixelWidth * grayscale.PixelHeight];
        grayscale.CopyPixels(pixelData, grayscale.PixelWidth, 0);
        // Encrypt the pixel data
        using var encryptor = algorithm.CreateEncryptor();
        var encryptedPixelData = encryptor.TransformFinalBlock(pixelData, 0, pixelData.Length);
        // Save the encrypted pixel data as a new image
        var encryptedBitmap = new Bitmap(grayscale.PixelWidth, grayscale.PixelHeight, PixelFormat.Format8bppIndexed);
        var rect = new Rectangle(0, 0, grayscale.PixelWidth, grayscale.PixelHeight);
        var encryptedBmpData = encryptedBitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
        Marshal.Copy(encryptedPixelData, 0, encryptedBmpData.Scan0, pixelData.Length);
        encryptedBitmap.UnlockBits(encryptedBmpData);
        return encryptedBitmap;
    }
}