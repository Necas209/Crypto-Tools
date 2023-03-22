using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CryptoTools.Services;
using Image = System.Drawing.Image;

namespace CryptoTools.ViewModels;

public class EncryptPageViewModel : BaseViewModel
{
    public List<ComboBoxItem> EncryptAlgorithms { get; }

    public ComboBoxItem Algorithm { get; set; }

    private static WriteableBitmap EncryptedByteArrayToGrayscaleImage(byte[] encryptedImageBytes, int width, int height)
    {
        var writeableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray8, null);

        var stride = (width * writeableBitmap.Format.BitsPerPixel + 7) / 8;

        // Ensure the byte array has the exact length required for the specified width and height
        var requiredLength = stride * height;
        if (encryptedImageBytes.Length != requiredLength)
        {
            Array.Resize(ref encryptedImageBytes, requiredLength);
        }

        writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), encryptedImageBytes, stride, 0);

        return writeableBitmap;
    }

    public static WriteableBitmap EncryptImage(string imagePath)
    {
        // Read the original image bytes
        var imageBytes = File.ReadAllBytes(imagePath);
        
        // get the image width and height
        var image = Image.FromFile(imagePath);
        var width = image.Width;
        var height = image.Height;

        // Encrypt the image
        var encryptedImageBytes = EncryptService.EncryptImage(imageBytes, "DES"); /*TEMPORARY*/

        // Convert the encrypted byte array to a BitmapImage
        return EncryptedByteArrayToGrayscaleImage(encryptedImageBytes, width, height);
    }
}