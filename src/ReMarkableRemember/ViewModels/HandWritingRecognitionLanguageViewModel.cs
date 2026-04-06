using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using ReMarkableRemember.Services.HandWritingRecognitionService;
using ReMarkableRemember.Services.LocalizationService;

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

    internal static IEnumerable<HandWritingRecognitionLanguageViewModel> GetLanguages(IHandWritingRecognitionService service, ILocalizationService localizationService)
    {
        List<HandWritingRecognitionLanguageViewModel> languages = new List<HandWritingRecognitionLanguageViewModel>();

        Thread thread = new Thread(() =>
        {
            languages.AddRange(service.SupportedLanguages.Select(code => new HandWritingRecognitionLanguageViewModel(code)));
        });

        if (!String.IsNullOrEmpty(localizationService.Configuration.CultureCode))
        {
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(localizationService.Configuration.CultureCode);
            thread.CurrentCulture = cultureInfo;
            thread.CurrentUICulture = cultureInfo;
        }

        thread.Start();
        thread.Join();

        return languages.OrderBy(language => language.DisplayName).ToArray();
    }

    private static String GetDisplayName(String code)
    {
        return code switch
        {
            "az_AZ" => CultureInfo.GetCultureInfo("az").DisplayName,
            "bs_BA" => CultureInfo.GetCultureInfo("bs").DisplayName,
            _ => CultureInfo.GetCultureInfo(code).DisplayName,
        };
    }
}
