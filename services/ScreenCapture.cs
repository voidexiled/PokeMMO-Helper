namespace PasaporteFiller.services;

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public class ScreenCapture
{
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out Rectangle rect);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
        IntPtr hdcSrc, int nXSrc, int nYSrc, CopyPixelOperation dwRop);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hdc);

    public static Bitmap CaptureScreen(Rectangle area)
    {
        IntPtr hDesk = GetDesktopWindow();
        IntPtr hSrce = CreateCompatibleDC(IntPtr.Zero);
        IntPtr hDest = CreateCompatibleDC(hSrce);
        IntPtr hBmp = CreateCompatibleBitmap(hSrce, area.Width, area.Height);
        IntPtr hOldBmp = SelectObject(hDest, hBmp);

        BitBlt(hDest, 0, 0, area.Width, area.Height, hSrce, area.Left, area.Top, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);

        Bitmap bmp = Image.FromHbitmap(hBmp);
        
        SelectObject(hDest, hOldBmp);
        DeleteObject(hBmp);
        DeleteDC(hDest);
        DeleteDC(hSrce);

        return bmp;
    }
}
