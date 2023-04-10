using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace CryptoTools.Utils;

public static class BitmapUtils
{
    public static Bitmap Grayscale(Bitmap input)
    {
        var output = new Bitmap(input.Width, input.Height, PixelFormat.Format8bppIndexed);
        var grayscale = output.Palette;
        for (var i = 0; i < grayscale.Entries.Length; i++) grayscale.Entries[i] = Color.FromArgb(i, i, i);
        var rect = new Rectangle(0, 0, input.Width, input.Height);
        var bmpData = input.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        var numBytes = bmpData.Stride * input.Height;
        var pixelData = new byte[numBytes];
        Marshal.Copy(bmpData.Scan0, pixelData, 0, numBytes);
        input.UnlockBits(bmpData);
        bmpData = output.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
        for (var i = 0; i < pixelData.Length; i += 3)
        {
            var value = (byte)(pixelData[i] * 0.299 + pixelData[i + 1] * 0.587 + pixelData[i + 2] * 0.114);
            pixelData[i / 3] = value;
        }

        Marshal.Copy(pixelData, 0, bmpData.Scan0, numBytes / 3);
        output.UnlockBits(bmpData);
        return output;
    }

    public static BitmapImage ToBitmapImage(string fileName)
    {
        using var stream = new FileStream(fileName, FileMode.Open);
        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.StreamSource = stream;
        bitmapImage.EndInit();
        bitmapImage.Freeze(); // just in case you want to load the image in another thread
        return bitmapImage;
    }
}