using System.Threading.Tasks;
using ReMarkableRemember.Services.ConfigurationService.Configuration;

namespace ReMarkableRemember.Services.ConfigurationService;
public interface IConfigurationService
{
    T Load<T>() where T : IConfiguration, new();

    Task Save<T>(T configuration) where T : IConfiguration;
}
