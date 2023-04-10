using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CryptoTools.Utils;

public static class BitmapUtils
{
    public static FormatConvertedBitmap ToGrayscale(string fileName)
    {
        var bitmapImage = ToBitmapImage(fileName);
        return new FormatConvertedBitmap(bitmapImage, PixelFormats.Gray8, null, 0);
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
        bitmap.Save(stream, ImageFormat.Bmp);
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