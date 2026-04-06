using ReMarkableRemember.Services.ConfigurationService.Configuration;

namespace ReMarkableRemember.Services.ConfigurationService.Service;

public abstract class ServiceBase<T> : ServiceBaseWithConfiguration<T> where T : ConfigurationBase, new()
{
    protected ServiceBase(IConfigurationService configurationService)
        : base(configurationService, new T())
    {
    }
}
