using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;

public class StringLocalizer : IStringLocalizer
{
    Dictionary<string, ResourceDictionary> resources;

    public StringLocalizer()
    {
        var enDict = LocalizedStringInitializer.GetEngLocalizedStrings();
        var ukDict = LocalizedStringInitializer.GetUkLocalizedStrings();

        resources = new Dictionary<string, ResourceDictionary>
        {
            {"en-GB", enDict },
            {"uk-UA", ukDict}
        };
    }

    public LocalizedString this[string name]
    {
        get
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            string val = "";
            if (resources.ContainsKey(currentCulture.Name))
            {
                if (resources[currentCulture.Name].Contains(name))
                {
                    val = (string)resources[currentCulture.Name][name];
                }
            }
            return new LocalizedString(name, val);
        }
    }

    public LocalizedString this[string name, params object[] arguments] => throw new NotImplementedException();

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        throw new NotImplementedException();
    }
}
