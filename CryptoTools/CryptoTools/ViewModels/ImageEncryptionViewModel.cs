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
        // Load the image and extract the pixel data
        using var bitmap = new Bitmap(imagePath);
        // Convert the image to grayscale
        // This helps to reduce the size of the encrypted image, which is useful for large images
        using var grayscaleBitmap = BitmapUtils.Grayscale(bitmap);
        // Extract the pixel data from the grayscale image
        var rect = new Rectangle(0, 0, grayscaleBitmap.Width, grayscaleBitmap.Height);
        var bmpData = grayscaleBitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
        var numBytes = bmpData.Stride * grayscaleBitmap.Height;
        var pixelData = new byte[numBytes];
        Marshal.Copy(bmpData.Scan0, pixelData, 0, numBytes);
        grayscaleBitmap.UnlockBits(bmpData);
        // Encrypt the pixel data
        using var encryptor = algorithm.CreateEncryptor();
        var encryptedPixelData = encryptor.TransformFinalBlock(pixelData, 0, numBytes);
        // Save the encrypted pixel data as a new image
        var encryptedBitmap = new Bitmap(grayscaleBitmap.Width, grayscaleBitmap.Height, PixelFormat.Format8bppIndexed);
        var encryptedBmpData = encryptedBitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
        Marshal.Copy(encryptedPixelData, 0, encryptedBmpData.Scan0, numBytes);
        encryptedBitmap.UnlockBits(encryptedBmpData);
        return encryptedBitmap;
    }
}