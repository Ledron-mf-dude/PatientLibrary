using PatientLibrary.Resources.LocalizedStrings;
using System.Windows;

public static class LocalizedStringInitializer
{
    public static ResourceDictionary GetEngLocalizedStrings()
    {
        ResourceDictionary result;
        result = EngLocalizedStrings.GetResource();
        return result;
    }

    public static ResourceDictionary GetUkLocalizedStrings()
    {
        ResourceDictionary result;
        result = UkLocalizedStrings.GetResource();
        return result;
    }
}
