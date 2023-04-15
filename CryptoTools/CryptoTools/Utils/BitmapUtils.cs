using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media.Imaging;

namespace CryptoTools.Utils;

public static class BitmapUtils
{
    public static async Task<BitmapImage> ToBitmapImage(StorageFile file)
    {
        using var stream = await file.OpenReadAsync();
        var bitmapImage = new BitmapImage
        {
            DecodePixelHeight = 0, // set to 0 to automatically decode the image at its original size
            DecodePixelWidth = 0 // set to 0 to automatically decode the image at its original size
        };
        await bitmapImage.SetSourceAsync(stream);
        return bitmapImage;
    }

    public static async Task<BitmapImage> ToBitmapImage(Bitmap bitmap, ImageFormat imageFormat)
    {
        using var stream = new InMemoryRandomAccessStream();
        bitmap.Save(stream.AsStream(), imageFormat);
        var bitmapImage = new BitmapImage
        {
            DecodePixelHeight = 0, // set to 0 to automatically decode the image at its original size
            DecodePixelWidth = 0 // set to 0 to automatically decode the image at its original size
        };
        await bitmapImage.SetSourceAsync(stream);
        return bitmapImage;
    }

    public static ImageFormat GetImageFormat(string file)
    {
        using var image = Image.FromFile(file);
        return image.RawFormat;
    }
}