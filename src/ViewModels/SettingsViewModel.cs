using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ReactiveUI;
using ReMarkableRemember.Models;

namespace ReMarkableRemember.ViewModels;

public sealed partial class SettingsViewModel : DialogWindowModel
{
    private String backup;
    private String myScriptApplicationKey;
    private String myScriptHmacKey;
    private String tabletIp;
    private String tabletPassword;

    private readonly Settings settings;

    internal SettingsViewModel(Settings settings) : base("Settings", "Save", true)
    {
        this.settings = settings;

        this.backup = this.settings.Backup;
        this.myScriptApplicationKey = this.settings.MyScriptApplicationKey;
        this.myScriptHmacKey = this.settings.MyScriptHmacKey;
        this.tabletIp = this.settings.TabletIp;
        this.tabletPassword = this.settings.TabletPassword;

        this.CommandSetBackup = ReactiveCommand.CreateFromTask(this.SetBackup);

        this.WhenAnyValue(vm => vm.TabletIp).Subscribe(this.CheckTabletIp);
        this.WhenAnyValue(vm => vm.TabletPassword).Subscribe(this.CheckTabletPassword);
    }

    public ReactiveCommand<Unit, Unit> CommandSetBackup { get; }

    public String Backup { get { return this.backup; } private set { this.RaiseAndSetIfChanged(ref this.backup, value); } }

    public String MyScriptApplicationKey { get { return this.myScriptApplicationKey; } set { this.RaiseAndSetIfChanged(ref this.myScriptApplicationKey, value); } }

    public String MyScriptHmacKey { get { return this.myScriptHmacKey; } set { this.RaiseAndSetIfChanged(ref this.myScriptHmacKey, value); } }

    public String TabletIp { get { return this.tabletIp; } set { this.RaiseAndSetIfChanged(ref this.tabletIp, value); } }

    public String TabletPassword { get { return this.tabletPassword; } set { this.RaiseAndSetIfChanged(ref this.tabletPassword, value); } }

    private void CheckTabletIp(String host)
    {
        this.ClearErrors(nameof(this.TabletIp));

        if (String.IsNullOrEmpty(host)) { return; }
        if (IpRegex().Match(host).Success) { return; }

        this.AddError(nameof(this.TabletIp), "Invalid IP address");
    }

    private void CheckTabletPassword(String password)
    {
        this.ClearErrors(nameof(this.TabletPassword));

        if (String.IsNullOrEmpty(password))
        {
            this.AddError(nameof(this.TabletPassword), "Password is required");
        }
    }

    public void SaveChanges()
    {
        this.settings.Backup = this.Backup;
        this.settings.MyScriptApplicationKey = this.MyScriptApplicationKey;
        this.settings.MyScriptHmacKey = this.MyScriptHmacKey;
        this.settings.TabletIp = this.TabletIp;
        this.settings.TabletPassword = this.TabletPassword;

        this.settings.SaveChanges();
    }

    [GeneratedRegex("^\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}$")]
    private static partial Regex IpRegex();

    private async Task SetBackup()
    {
        String? backupFolder = await this.OpenFolderPicker.Handle("Backup Folder");
        if (backupFolder != null)
        {
            this.Backup = backupFolder;
        }
    }
}
