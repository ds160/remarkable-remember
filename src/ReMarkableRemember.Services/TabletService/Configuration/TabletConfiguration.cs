using System;
using ReMarkableRemember.Services.ConfigurationService.Configuration;

namespace ReMarkableRemember.Services.TabletService.Configuration;

public sealed class TabletConfiguration : ConfigurationBase, ITabletConfiguration
{
    public TabletConfiguration() : base("Tablet") { }

    public String Backup { get; set; } = String.Empty;

    public String IP { get; set; } = String.Empty;

    public String Password { get; set; } = String.Empty;
}
