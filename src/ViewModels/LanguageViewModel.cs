using System;
using System.Collections.Generic;
using System.Globalization;

namespace ReMarkableRemember.ViewModels;

public sealed class LanguageViewModel
{
    private LanguageViewModel(String code)
    {
        this.Code = code;
        this.DisplayName = CultureInfo.GetCultureInfo(code).DisplayName;
    }

    public String Code { get; }

    public String DisplayName { get; }

    internal static IEnumerable<LanguageViewModel> GetLanguages()
    {
        return new List<LanguageViewModel>()
        {
            new LanguageViewModel("de_DE"),
            new LanguageViewModel("en_US")
        };
    }
}
