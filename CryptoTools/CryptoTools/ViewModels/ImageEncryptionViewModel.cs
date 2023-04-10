using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CryptoLib.Models;
using CryptoTools.Utils;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

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
        // Load the image and extract the pixel data
        var bitmapImage = BitmapUtils.ToBitmapImage(imagePath);
        // Convert the image to grayscale
        var grayscale = new FormatConvertedBitmap(bitmapImage, PixelFormats.Gray8, null, 0);
        // Extract the pixel data from the grayscale image
        var stride = grayscale.PixelWidth;
        var numBytes = stride * grayscale.PixelHeight;
        var pixelData = new byte[numBytes];
        grayscale.CopyPixels(pixelData, stride, 0);
        // Encrypt the pixel data
        using var encryptor = algorithm.CreateEncryptor();
        var encryptedPixelData = encryptor.TransformFinalBlock(pixelData, 0, pixelData.Length);
        // Save the encrypted pixel data as a new image
        var encryptedBitmap = new Bitmap(grayscale.PixelWidth, grayscale.PixelHeight, PixelFormat.Format8bppIndexed);
        var rect = new Rectangle(0, 0, grayscale.PixelWidth, grayscale.PixelHeight);
        var encryptedBmpData = encryptedBitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
        Marshal.Copy(encryptedPixelData, 0, encryptedBmpData.Scan0, numBytes);
        encryptedBitmap.UnlockBits(encryptedBmpData);
        return encryptedBitmap;
    }
}