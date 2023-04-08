using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        using var algorithm = EncryptionService.GetAlgorithm(SelectedAlgorithm.Name);
        // Set the encryption key and generate an Initialization Vector
        algorithm.GenerateKey();
        algorithm.GenerateIV();
        ImageFormat imageFormat;
        using (var image = Image.FromFile(imagePath))
        {
            imageFormat = image.RawFormat;
        }

        using var inputFileStream = new FileStream(imagePath, FileMode.Open);
        // Create the output file path
        var imageExtension = Path.GetExtension(imagePath);
        var outputFilePath = Path.Combine(Path.GetDirectoryName(imagePath) ?? string.Empty,
            Path.GetFileNameWithoutExtension(imagePath) + ".encrypted" + imageExtension);
        using var outputFileStream = new FileStream(outputFilePath, FileMode.Create);
        using var encryptor = algorithm.CreateEncryptor();
        // Load the image and extract the pixel data
        var bitmap = new Bitmap(inputFileStream);
        var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        var numBytes = bmpData.Stride * bitmap.Height;
        var pixelData = new byte[numBytes];
        Marshal.Copy(bmpData.Scan0, pixelData, 0, numBytes);
        bitmap.UnlockBits(bmpData);
        // Encrypt the pixel data
        var encryptedPixelData = encryptor.TransformFinalBlock(pixelData, 0, numBytes);
        // Save the encrypted pixel data as a new image
        var encryptedBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
        var encryptedBmpData = encryptedBitmap.LockBits(rect, ImageLockMode.WriteOnly,
            PixelFormat.Format32bppArgb);
        Marshal.Copy(encryptedPixelData[..numBytes], 0, encryptedBmpData.Scan0, numBytes);
        encryptedBitmap.UnlockBits(encryptedBmpData);
        encryptedBitmap.Save(outputFileStream, imageFormat);
        return outputFilePath;
    }
}