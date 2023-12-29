using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ReMarkableRemember.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class HandWritingRecognitionLanguageViewModel
{
    private HandWritingRecognitionLanguageViewModel(String code)
    {
        this.Code = code;
        this.DisplayName = CultureInfo.GetCultureInfo(code).DisplayName;
    }

    public String Code { get; }

    public String DisplayName { get; }

    internal static IEnumerable<HandWritingRecognitionLanguageViewModel> GetLanguages()
    {
        return MyScript.SupportedLanguages
            .Select(code => new HandWritingRecognitionLanguageViewModel(code))
            .OrderBy(language => language.DisplayName)
            .ToArray();
    }
}
