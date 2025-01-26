using System;
using System.Threading.Tasks;

namespace ReMarkableRemember.Services.ConfigurationService.Configuration;

public interface IConfiguration
{
    String GetPrefix();

    Task Save();
}
