using System.Threading.Tasks;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public static class LocaleSetting
{
#pragma warning disable CS4014
    public static void ChangeSelectedLocale(string locale) => _ChangeSelectedLocale(locale);
#pragma warning restore CS4014
    
    private static async Task _ChangeSelectedLocale(string locale)
    {
        LocalizationSettings.SelectedLocale = Locale.CreateLocale(locale);
        await LocalizationSettings.InitializationOperation.Task;
    }
}