using ReMarkableRemember.Services.ConfigurationService.Configuration;

namespace ReMarkableRemember.Services.ConfigurationService.Service;

public abstract class ServiceBaseWithConfiguration<T> where T : ConfigurationBase
{
    protected ServiceBaseWithConfiguration(IConfigurationService configurationService, T configuration)
    {
        this.Configuration = configuration;
        this.Configuration.Load(configurationService);
    }

    public T Configuration { get; }
}
