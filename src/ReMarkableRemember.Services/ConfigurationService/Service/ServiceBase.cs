using ReMarkableRemember.Services.ConfigurationService.Configuration;

namespace ReMarkableRemember.Services.ConfigurationService.Service;

public abstract class ServiceBase<T> where T : ConfigurationBase, new()
{
    protected ServiceBase(IConfigurationService configurationService)
    {
        this.Configuration = configurationService.Load<T>();
        this.Configuration.SetService(configurationService);
    }

    public T Configuration { get; }
}
