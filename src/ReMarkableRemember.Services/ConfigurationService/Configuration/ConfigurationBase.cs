using System;
using System.Threading.Tasks;

namespace ReMarkableRemember.Services.ConfigurationService.Configuration;

public abstract class ConfigurationBase : IConfiguration
{
    private readonly String prefix;
    private IConfigurationService? service;

    protected ConfigurationBase(String prefix)
    {
        this.prefix = prefix;
    }

    String IConfiguration.GetPrefix()
    {
        return this.prefix;
    }

    public async Task Save()
    {
        await this.service!.Save(this).ConfigureAwait(false);
    }

    internal void SetService(IConfigurationService service)
    {
        this.service = service;
    }
}