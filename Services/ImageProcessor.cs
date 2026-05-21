using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using SkiaSharp;

namespace PsWinSnip.Services;

public class ImageProcessor
{
    public static SKBitmap CropCapture(SKBitmap fullCapture, Rect selection)
    {
        // 1. Crop to selection
        SKRectI cropRect = new SKRectI(
            (int)Math.Floor(selection.X),
            (int)Math.Floor(selection.Y),
            (int)Math.Ceiling(selection.X + selection.Width),
            (int)Math.Ceiling(selection.Y + selection.Height)
        );

        // Ensure we don't exceed full capture bounds
        cropRect = SKRectI.Intersect(cropRect, new SKRectI(0, 0, fullCapture.Width, fullCapture.Height));

        if (cropRect.Width <= 0 || cropRect.Height <= 0)
        {
            return new SKBitmap(1, 1);
        }

        SKBitmap cropped = new SKBitmap(cropRect.Width, cropRect.Height);
        if (!fullCapture.ExtractSubset(cropped, cropRect))
        {
            // Fallback if extraction fails
            cropped.Dispose();
            return new SKBitmap(1, 1);
        }

        return cropped;
    }

    public static void CopyToClipboard(SKBitmap bitmap)
    {
        // Convert SKBitmap to BitmapSource for WPF Clipboard
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = new MemoryStream();
        data.SaveTo(stream);
        stream.Seek(0, SeekOrigin.Begin);

        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.StreamSource = stream;
        bitmapImage.EndInit();
        bitmapImage.Freeze();

        System.Windows.Clipboard.SetImage(bitmapImage);
    }
}