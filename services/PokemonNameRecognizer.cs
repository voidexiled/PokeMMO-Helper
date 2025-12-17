namespace PasaporteFiller.services;

public class PokemonNameRecognizer
{
    public static string? RecognizePokemonName(string input)
    {
        string cleanedInput = TextCleaner.CleanText(input);
        string closestMatch = null;
        int smallestDistance = int.MaxValue;

        foreach (var name in ScreenTextRecognizer.PokemonNames)
        {
            int distance = LevenshteinDistance.Compute(cleanedInput, name);
            if (distance < smallestDistance)
            {
                smallestDistance = distance;
                closestMatch = name;
            }
        }

        return closestMatch;
    }
}