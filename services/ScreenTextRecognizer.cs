using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Tesseract;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using PixelInternalFormat = Veldrid.OpenGLBinding.PixelInternalFormat;
using TextureMagFilter = Veldrid.OpenGLBinding.TextureMagFilter;
using TextureMinFilter = Veldrid.OpenGLBinding.TextureMinFilter;
using TextureParameterName = Veldrid.OpenGLBinding.TextureParameterName;
using TextureTarget = Veldrid.OpenGLBinding.TextureTarget;

namespace PasaporteFiller.services;

public class ScreenTextRecognizer
{

    public static IntPtr PreviewScreenshootImage { get; private set; } = IntPtr.Zero;
    public static string LastRecognizedPokemonName { get; private set; } = "";
    public static string LastRecognizedText { get; private set; } = "";
    public static string LastRecognizedPokemonNameWithLevenshtein { get; private set; } = "";

    public static List<string> PokemonNames { get; set; } = new();
    
    private static Rectangle _area;
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();
    
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private static IntPtr ConvertBitmapToIntPtr(Bitmap bitmap)
    {
// Lock the bitmap's bits

        return bitmap.GetHbitmap();
    }

    public static void setArea(int x1, int y1, int x2, int y2)
    {
        _area = new Rectangle(x1, y1, x2 - x1, y2 - y1);
    }
    public void setPokemonNames(List<string> pokemonNames)
    {
        PokemonNames = pokemonNames;
    }
    private static Bitmap CaptureScreen(Rectangle area)
    {
        Bitmap bmp = new Bitmap(area.Width, area.Height, PixelFormat.Format32bppRgb);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.CopyFromScreen(area.Left, area.Top, 0, 0, area.Size, CopyPixelOperation.SourceCopy);
        }
    
        return bmp;
    }
    
    private static string RecognizeTextFromScreen(Rectangle area)
    {
        // Set the TESSDATA_PREFIX environment variable
        string tessDataPrefix = AppDomain.CurrentDomain.BaseDirectory;
        Environment.SetEnvironmentVariable("TESSDATA_PREFIX", tessDataPrefix);

        Bitmap screenshot = CaptureScreen(area);
        string recognizedText = string.Empty;

        try
        {
            using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
            {
                using (var img = PixConverter.ToPix(screenshot))
                {
                    using (var page = engine.Process(img))
                    {
                        recognizedText = page.GetText();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during OCR processing: {ex.Message}");
        }
        finally
        {
            screenshot.Dispose();
        }

        return recognizedText;
    }
    
    public static void CheckForPokemonName(object? sender, ElapsedEventArgs e)
    {
        string recognizedText = RecognizeTextFromScreen(_area);
        LastRecognizedText = recognizedText;

        // Check for Pokémon names
        string? foundPokemon = null;

        if (!RenderPasaporte._useLevenshteinMethod)
        { 
            foundPokemon = PokemonNames.FirstOrDefault(name => recognizedText.Contains(name, StringComparison.OrdinalIgnoreCase));
            if (foundPokemon != null)
                LastRecognizedPokemonName = foundPokemon;
        }
        else
        {
            foundPokemon = PokemonNameRecognizer.RecognizePokemonName(recognizedText);
            if (foundPokemon != null)
                LastRecognizedPokemonNameWithLevenshtein = foundPokemon;    
        }
        
        if (!string.IsNullOrEmpty(foundPokemon))
        {
            Console.WriteLine("Found Pokémon: " + foundPokemon);
            LastRecognizedPokemonName = foundPokemon;
            // Store the found Pokémon name in a variable
            // You can add additional logic here to handle the found Pokémon
        }
        else
        {
            Console.WriteLine("No Pokémon found.");
        }

    }

    
}