using System;
using System.Collections.ObjectModel;
using System.Globalization;

namespace ReMarkableRemember.ViewModels;

public sealed class LanguageViewModel : ViewModelBase
{
    public LanguageViewModel(String code)
    {
        this.Code = code;
        this.DisplayName = CultureInfo.GetCultureInfo(code).DisplayName;
    }

    public String Code { get; }

    public String DisplayName { get; }

    internal static Collection<LanguageViewModel> GetLanguages()
    {
        return new Collection<LanguageViewModel>()
        {
            new LanguageViewModel("de_DE"),
            new LanguageViewModel("en_US")
        };
    }
}
