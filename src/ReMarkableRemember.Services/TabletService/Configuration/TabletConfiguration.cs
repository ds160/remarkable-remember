using System;
using ReMarkableRemember.Services.ConfigurationService.Configuration;

namespace ReMarkableRemember.Services.TabletService.Configuration;

public sealed class TabletConfiguration : ConfigurationBase, ITabletConfiguration
{
    public TabletConfiguration() : base("Tablet")
    {
        this.Backup = String.Empty;
        this.IP = String.Empty;
        this.Password = String.Empty;
    }

    public String Backup { get; set; }

    public String IP { get; set; }

    public String Password { get; set; }
}
