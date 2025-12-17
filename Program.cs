using System.Runtime.InteropServices;

namespace PasaporteFiller
{
    public class Program {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        private static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
            RenderPasaporte renderPasaporte = new();
            renderPasaporte.Start().Wait();
            ShowWindow(handle, SW_HIDE);
        }
    }
}
