using System.Threading.Tasks;
using ReMarkableRemember.Services.ConfigurationService.Configuration;

namespace ReMarkableRemember.Services.ConfigurationService;

public interface IConfigurationService
{
    Task Load<T>(T configuration) where T : IConfiguration;

    Task Save<T>(T configuration) where T : IConfiguration;
}
