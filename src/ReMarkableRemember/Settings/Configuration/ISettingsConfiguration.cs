using System;
using ReMarkableRemember.Services.ConfigurationService.Configuration;

namespace ReMarkableRemember.Settings.Configuration;

public interface ISettingsConfiguration : IConfiguration
{
    String ApplicationLanguage { get; set; }

    String ApplicationTheme { get; set; }
}
