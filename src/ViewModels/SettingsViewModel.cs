using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReMarkableRemember.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class SettingsViewModel : DialogWindowModel
{
    private readonly Settings settings;

    internal SettingsViewModel(Settings settings) : base("Settings", "Save", true)
    {
        this.settings = settings;

        this.Backup = this.settings.Backup;
        this.MyScriptApplicationKey = this.settings.MyScriptApplicationKey;
        this.MyScriptHmacKey = this.settings.MyScriptHmacKey;
        this.TabletIp = this.settings.TabletIp;
        this.TabletPassword = this.settings.TabletPassword;

        this.CommandSetBackup = ReactiveCommand.CreateFromTask(this.SetBackup);
    }

    public ReactiveCommand<Unit, Unit> CommandSetBackup { get; }

    public String? Backup { get; private set; }

    public String? MyScriptApplicationKey { get; set; }

    public String? MyScriptHmacKey { get; set; }

    public String? TabletIp { get; set; }

    public String? TabletPassword { get; set; }

    protected override void Close()
    {
        this.settings.Backup = this.Backup;
        this.settings.MyScriptApplicationKey = this.MyScriptApplicationKey;
        this.settings.MyScriptHmacKey = this.MyScriptHmacKey;
        this.settings.TabletIp = this.TabletIp;
        this.settings.TabletPassword = this.TabletPassword;
        this.settings.SaveChanges();
    }

    private async Task SetBackup()
    {
        String? backupFolder = await this.OpenFolderPicker.Handle("Backup Folder");
        if (backupFolder != null)
        {
            this.Backup = backupFolder;
            this.RaisePropertyChanged(nameof(this.Backup));
        }
    }
}
