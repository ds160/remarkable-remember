using System;
using ReMarkableRemember.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class SettingsViewModel : DialogWindowModel
{
    private readonly Settings settings;

    internal SettingsViewModel(Settings settings) : base("Settings", "Save")
    {
        this.settings = settings;

        this.Backup = this.settings.Backup;
        this.MyScriptApplicationKey = this.settings.MyScriptApplicationKey;
        this.MyScriptHmacKey = this.settings.MyScriptHmacKey;
        this.TabletIp = this.settings.TabletIp;
        this.TabletPassword = this.settings.TabletPassword;
    }

    public String? Backup { get; set; }

    public String? MyScriptApplicationKey { get; set; }

    public String? MyScriptHmacKey { get; set; }

    public String? TabletIp { get; set; }

    public String? TabletPassword { get; set; }

    protected override Boolean Close()
    {
        this.settings.Backup = this.Backup;
        this.settings.MyScriptApplicationKey = this.MyScriptApplicationKey;
        this.settings.MyScriptHmacKey = this.MyScriptHmacKey;
        this.settings.TabletIp = this.TabletIp;
        this.settings.TabletPassword = this.TabletPassword;
        this.settings.SaveChanges();

        return base.Close();
    }
}
