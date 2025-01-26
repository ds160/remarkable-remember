using System;
using ReMarkableRemember.Services.ConfigurationService.Configuration;

namespace ReMarkableRemember.Services.TabletService.Configuration;

public interface ITabletConfiguration : IConfiguration
{
    String Backup { get; set; }

    String IP { get; set; }

    String Password { get; set; }
}