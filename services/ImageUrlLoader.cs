using System.Net.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PasaporteFiller.services;

/// <summary>
/// Helper class for loading images from URLs and converting to textures
/// </summary>
public static class ImageUrlLoader
{
    private static readonly HttpClient HttpClient = new();
    private static readonly Dictionary<string, byte[]> ImageCache = new();

    /// <summary>
    /// Download image from URL and return image bytes
    /// </summary>
    /// <param name="url">Image URL</param>
    /// <param name="cachePath">Optional path to cache the image locally</param>
    /// <returns>Image bytes ready for AddOrGetImagePointer()</returns>
    public static async Task<byte[]?> LoadImageFromUrl(string url, string? cachePath = null)
    {
        try
        {
            // Check memory cache first
            if (ImageCache.ContainsKey(url))
            {
                Console.WriteLine($"[ImageUrlLoader] Cache hit: {url}");
                return ImageCache[url];
            }

            // Check file cache if path provided
            if (!string.IsNullOrEmpty(cachePath) && File.Exists(cachePath))
            {
                Console.WriteLine($"[ImageUrlLoader] File cache hit: {cachePath}");
                var bytes = await File.ReadAllBytesAsync(cachePath);
                ImageCache[url] = bytes;
                return bytes;
            }

            // Download from URL
            Console.WriteLine($"[ImageUrlLoader] Downloading: {url}");
            var imageBytes = await HttpClient.GetByteArrayAsync(url);

            // Save to file cache if path provided
            if (!string.IsNullOrEmpty(cachePath))
            {
                var directory = Path.GetDirectoryName(cachePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                await File.WriteAllBytesAsync(cachePath, imageBytes);
                Console.WriteLine($"[ImageUrlLoader] Cached to: {cachePath}");
            }

            // Add to memory cache
            ImageCache[url] = imageBytes;

            return imageBytes;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ImageUrlLoader] ERROR loading {url}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Save downloaded image bytes as PNG file
    /// </summary>
    public static async Task<string?> DownloadAndSaveImage(string url, string savePath)
    {
        try
        {
            var imageBytes = await LoadImageFromUrl(url, savePath);
            return imageBytes != null ? savePath : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ImageUrlLoader] ERROR saving {url} to {savePath}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Clear the memory cache
    /// </summary>
    public static void ClearCache()
    {
        ImageCache.Clear();
        Console.WriteLine("[ImageUrlLoader] Memory cache cleared");
    }
}
