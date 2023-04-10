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
        var pixelData = new byte[bmpData.Stride * input.Height];
        Marshal.Copy(bmpData.Scan0, pixelData, 0, pixelData.Length);
        input.UnlockBits(bmpData);
        bmpData = output.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
        var rowBytes = new byte[bmpData.Stride];
        for (var y = 0; y < bmpData.Height; y++)
        {
            var offset = y * bmpData.Stride;
            for (var x = 0; x < bmpData.Width; x++)
            {
                var pixelOffset = offset + x;
                var r = pixelData[pixelOffset * 3];
                var g = pixelData[pixelOffset * 3 + 1];
                var b = pixelData[pixelOffset * 3 + 2];
                rowBytes[x] = (byte)(r * 0.299 + g * 0.587 + b * 0.114);
            }

            Marshal.Copy(rowBytes, 0, bmpData.Scan0 + offset, rowBytes.Length);
        }

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

    public static BitmapImage ToBitmapImage(Bitmap bitmap)
    {
        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        stream.Position = 0;
        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.StreamSource = stream;
        bitmapImage.EndInit();
        bitmapImage.Freeze(); // just in case you want to load the image in another thread
        return bitmapImage;
    }
}