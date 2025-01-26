using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ReMarkableRemember.Services.HandWritingRecognition;

namespace ReMarkableRemember.ViewModels;

public sealed class HandWritingRecognitionLanguageViewModel
{
    private HandWritingRecognitionLanguageViewModel(String code)
    {
        this.Code = code;
        this.DisplayName = GetDisplayName(code);
    }

    public String Code { get; }

    public String DisplayName { get; }

    internal static IEnumerable<HandWritingRecognitionLanguageViewModel> GetLanguages(IHandWritingRecognitionService service)
    {
        return service.SupportedLanguages
            .Select(code => new HandWritingRecognitionLanguageViewModel(code))
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
