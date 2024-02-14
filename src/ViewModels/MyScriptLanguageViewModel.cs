using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ReMarkableRemember.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class MyScriptLanguageViewModel
{
    private MyScriptLanguageViewModel(String code)
    {
        this.Code = code;
        this.DisplayName = GetDisplayName(code);
    }

    public String Code { get; }

    public String DisplayName { get; }

    internal static IEnumerable<MyScriptLanguageViewModel> GetLanguages()
    {
        return MyScript.SupportedLanguages
            .Select(code => new MyScriptLanguageViewModel(code))
            .OrderBy(language => language.DisplayName)
            .ToArray();
    }

    private static String GetDisplayName(String code)
    {
        switch (code)
        {
            case "az_AZ": return CultureInfo.GetCultureInfo("az").DisplayName;
            case "bs_BA": return CultureInfo.GetCultureInfo("bs").DisplayName;
            default: return CultureInfo.GetCultureInfo(code).DisplayName;
        }
    }
}
