using System.Text.RegularExpressions;

namespace PasaporteFiller.services;

public class TextCleaner
{
    public static string CleanText(string input)
    {
        // Eliminar caracteres no alfabéticos
        string cleaned = Regex.Replace(input, @"[^a-zA-Z\s]", "");
        return cleaned.Trim();
    }
}