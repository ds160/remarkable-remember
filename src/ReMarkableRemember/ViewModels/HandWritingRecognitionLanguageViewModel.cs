using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using ReMarkableRemember.DependencyInjection;

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

    internal static IEnumerable<HandWritingRecognitionLanguageViewModel> GetLanguages(IServices services)
    {
        List<HandWritingRecognitionLanguageViewModel> languages = new List<HandWritingRecognitionLanguageViewModel>();

        Thread thread = new Thread(() =>
        {
            languages.AddRange(services.HandWritingRecognition.SupportedLanguages.Select(code => new HandWritingRecognitionLanguageViewModel(code)));
        });

        if (!String.IsNullOrEmpty(services.Settings.Configuration.ApplicationLanguage))
        {
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(services.Settings.Configuration.ApplicationLanguage);
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
