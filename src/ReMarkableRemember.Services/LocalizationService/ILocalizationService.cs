using System;
using ReMarkableRemember.Services.LocalizationService.Configuration;

namespace ReMarkableRemember.Services.LocalizationService;

public interface ILocalizationService
{
    ILocalizationConfiguration Configuration { get; }

    String GetDateTimeFormat()
    {
        return "yyyy-MM-dd HH:mm";
    }
}
