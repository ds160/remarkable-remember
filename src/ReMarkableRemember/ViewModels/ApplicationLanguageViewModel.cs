using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using ReMarkableRemember.Common.Localization;

namespace ReMarkableRemember.ViewModels;

public sealed class ApplicationLanguageViewModel
{
    private static readonly IEnumerable<ApplicationLanguageViewModel> languages;

    static ApplicationLanguageViewModel()
    {
        languages = Language.SupportedCodes
            .Select(code => new ApplicationLanguageViewModel(code, GetDisplayName(code)))
            .OrderBy(language => language.DisplayName)
            .ToArray();
    }

    private ApplicationLanguageViewModel(String code, String displayName)
    {
        this.Code = code;
        this.DisplayName = displayName;
    }

    public String Code { get; }

    public String DisplayName { get; }

    private static String GetDisplayName(String code)
    {
        String displayName = code;

        Thread thread = new Thread(() =>
        {
            displayName = CultureInfo.GetCultureInfo(code).DisplayName;
        });

        CultureInfo cultureInfo = CultureInfo.GetCultureInfo(code);
        thread.CurrentCulture = cultureInfo;
        thread.CurrentUICulture = cultureInfo;

        thread.Start();
        thread.Join();

        return displayName;
    }

    internal static IEnumerable<ApplicationLanguageViewModel> GetLanguages(String defaultDisplayName)
    {
        List<ApplicationLanguageViewModel> result = languages.ToList();
        result.Insert(0, new ApplicationLanguageViewModel(String.Empty, defaultDisplayName));
        return result;
    }
}
