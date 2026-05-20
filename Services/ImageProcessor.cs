using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using SkiaSharp;

namespace PsWinSnip.Services;

public class ImageProcessor
{
    private const int InstagramWidth = 1080;

    public static SKBitmap ProcessCapture(SKBitmap fullCapture, Rect selection)
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

        using SKBitmap cropped = new SKBitmap(cropRect.Width, cropRect.Height);
        if (!fullCapture.ExtractSubset(cropped, cropRect))
        {
            // Fallback if extraction fails
            return new SKBitmap(1, 1);
        }

        // 2. Resize to Instagram standard (1080px width)
        double ratio = (double)cropped.Width / cropped.Height;
        
        // Snap to common ratios if very close (e.g. within 1%)
        if (Math.Abs(ratio - 1.0) < 0.01) ratio = 1.0;
        else if (Math.Abs(ratio - 0.8) < 0.01) ratio = 0.8; // 4:5
        else if (Math.Abs(ratio - 0.75) < 0.01) ratio = 0.75; // 3:4

        int targetWidth = InstagramWidth;
        int targetHeight = (int)Math.Round(InstagramWidth / ratio);

        SKBitmap resized = new SKBitmap(targetWidth, targetHeight);
        cropped.ScalePixels(resized, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));
        
        return resized;
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