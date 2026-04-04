using ReMarkableRemember.Services.ConfigurationService;
using ReMarkableRemember.Services.ConfigurationService.Service;
using ReMarkableRemember.Services.LocalizationService.Configuration;

namespace ReMarkableRemember.Services.LocalizationService;

public sealed class LocalizationService : ServiceBase<LocalizationConfiguration>, ILocalizationService
{
    public LocalizationService(IConfigurationService configurationService)
        : base(configurationService)
    {
    }

    ILocalizationConfiguration ILocalizationService.Configuration
    {
        get { return this.Configuration; }
    }
}
