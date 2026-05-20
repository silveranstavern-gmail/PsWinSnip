using System.IO;
using PsWinSnip.Helpers;
using SkiaSharp;

namespace PsWinSnip.Services;

public class ScreenCapture
{
    public static (int X, int Y, int Width, int Height) GetVirtualScreenBounds()
    {
        int x = Win32Interop.GetSystemMetrics(Win32Interop.SM_XVIRTUALSCREEN);
        int y = Win32Interop.GetSystemMetrics(Win32Interop.SM_YVIRTUALSCREEN);
        int width = Win32Interop.GetSystemMetrics(Win32Interop.SM_CXVIRTUALSCREEN);
        int height = Win32Interop.GetSystemMetrics(Win32Interop.SM_CYVIRTUALSCREEN);
        return (x, y, width, height);
    }

    public static SKBitmap CaptureRegion(int x, int y, int width, int height)
    {
        IntPtr screenDc = Win32Interop.GetDC(IntPtr.Zero);
        IntPtr memDc = Win32Interop.CreateCompatibleDC(screenDc);
        IntPtr hBitmap = Win32Interop.CreateCompatibleBitmap(screenDc, width, height);
        IntPtr oldBitmap = Win32Interop.SelectObject(memDc, hBitmap);

        try
        {
            // Use CAPTUREBLT to include layered windows
            Win32Interop.BitBlt(memDc, 0, 0, width, height, screenDc, x, y, Win32Interop.SRCCOPY | Win32Interop.CAPTUREBLT);

            var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            
            var bmi = new Win32Interop.BITMAPINFO
            {
                bmiHeader = Win32Interop.BITMAPINFOHEADER.Create(width, height, 32)
            };

            Win32Interop.GetDIBits(memDc, hBitmap, 0, (uint)height, bitmap.GetPixels(), ref bmi, Win32Interop.DIB_RGB_COLORS);

            return bitmap;
        }
        finally
        {
            Win32Interop.SelectObject(memDc, oldBitmap);
            Win32Interop.DeleteObject(hBitmap);
            Win32Interop.DeleteDC(memDc);
            Win32Interop.ReleaseDC(IntPtr.Zero, screenDc);
        }
    }

    public static SKBitmap CaptureFullScreen()
    {
        var (x, y, width, height) = GetVirtualScreenBounds();
        return CaptureRegion(x, y, width, height);
    }

    public static void SaveBitmap(SKBitmap bitmap, string path, int quality = 95)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
        if (data != null)
        {
            using var stream = File.Create(path);
            data.SaveTo(stream);
        }
    }

    public static void SaveBitmap(SKBitmap bitmap, Stream stream, int quality = 95)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
        data?.SaveTo(stream);
    }
}