using System;
using ReMarkableRemember.Services.ConfigurationService.Configuration;

namespace ReMarkableRemember.Services.LocalizationService.Configuration;

public interface ILocalizationConfiguration : IConfiguration
{
    String CultureCode { get; set; }
}
