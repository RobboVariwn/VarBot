using System;
using System.Linq;
using System.Reflection;
using Varwin;

public static class I18nEx
{
    public static string LocalizedString(this I18n self)
    {
        string lang = Settings.Instance().Language;
        Type localeType = typeof(I18n);
        string value = localeType.GetProperty(lang)?.GetValue(self)?.ToString();
        if (string.IsNullOrEmpty(value))
        {
            if (!string.IsNullOrEmpty(self.en))
            {
                value = self.en;
            }
            else
            {
                var properties = localeType.GetProperties();
                value = properties.FirstOrDefault(x => x.CanRead && string.IsNullOrEmpty(x.GetValue(self)?.ToString()))?.ToString();
            }
        }
        return value ?? string.Empty;
    }

    public static void SetLocale(this I18n self, string lang, string value)
    {
        typeof(I18n).GetProperty(lang)?.SetValue(self, value);
    }
    
    public static void SetLocale(this ILocalizable self, string lang, string value)
    {
        if (self.i18n == null)
        {
            self.i18n = new I18n();
        }
        self.i18n.SetLocale(lang, value);
    }
    
    public static string GetCurrentLocale(this I18n self)
    {
        string result = String.Empty;

        PropertyInfo property = typeof(I18n).GetProperty(Settings.Instance().Language);

        if (property != null)
        {
            result = property.GetValue(self).ToString();
        }

        return result;
    }
}