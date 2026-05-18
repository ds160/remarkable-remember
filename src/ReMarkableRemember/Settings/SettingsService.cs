using ReMarkableRemember.Services.ConfigurationService;
using ReMarkableRemember.Services.ConfigurationService.Service;
using ReMarkableRemember.Settings.Configuration;

namespace ReMarkableRemember.Settings;

internal sealed class SettingsService : ServiceBase<SettingsConfiguration>, ISettingsService
{
    public SettingsService(IConfigurationService configurationService)
        : base(configurationService)
    {
    }

    ISettingsConfiguration ISettingsService.Configuration
    {
        get { return this.Configuration; }
    }
}
