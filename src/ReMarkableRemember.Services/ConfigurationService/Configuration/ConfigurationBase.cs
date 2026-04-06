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
        if (this.service == null) { throw new InvalidOperationException(); }
        await this.service.Save(this).ConfigureAwait(false);
    }

    internal void Load(IConfigurationService service)
    {
        this.service = service;
        this.service.Load(this).Wait();
    }
}
