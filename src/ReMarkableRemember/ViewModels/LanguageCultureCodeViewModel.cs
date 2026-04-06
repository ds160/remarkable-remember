using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using ReMarkableRemember.Common.Localization;

namespace ReMarkableRemember.ViewModels;

public sealed class LanguageCultureCodeViewModel
{
    private static readonly IEnumerable<LanguageCultureCodeViewModel> languages;

    static LanguageCultureCodeViewModel()
    {
        languages = Language.SupportedCultureCodes
            .Select(cultureCode => new LanguageCultureCodeViewModel(cultureCode, GetDisplayName(cultureCode)))
            .ToArray();
    }

    private LanguageCultureCodeViewModel(String cultureCode, String displayName)
    {
        this.CultureCode = cultureCode;
        this.DisplayName = displayName;
    }

    public String CultureCode { get; }

    public String DisplayName { get; }

    private static String GetDisplayName(String cultureCode)
    {
        String displayName = cultureCode;

        Thread thread = new Thread(() =>
        {
            displayName = CultureInfo.GetCultureInfo(cultureCode).DisplayName;
        });

        CultureInfo cultureInfo = CultureInfo.GetCultureInfo(cultureCode);
        thread.CurrentCulture = cultureInfo;
        thread.CurrentUICulture = cultureInfo;

        thread.Start();
        thread.Join();

        return displayName;
    }

    internal static IEnumerable<LanguageCultureCodeViewModel> GetLanguages(String defaultDisplayName)
    {
        List<LanguageCultureCodeViewModel> result = languages.ToList();
        result.Insert(0, new LanguageCultureCodeViewModel(String.Empty, defaultDisplayName));
        return result;
    }
}
