using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using ReMarkableRemember.Services.HandWritingRecognition;
using ReMarkableRemember.Services.HandWritingRecognition.Configuration;
using ReMarkableRemember.Services.TabletService;
using ReMarkableRemember.Services.TabletService.Configuration;

namespace ReMarkableRemember.ViewModels;

public sealed partial class SettingsViewModel : DialogWindowModel
{
    private String backup;
    private HandWritingRecognitionLanguageViewModel handWritingRecognitionLanguage;
    private String myScriptApplicationKey;
    private String myScriptHmacKey;
    private String tabletIp;
    private String tabletPassword;

    private readonly IHandWritingRecognitionConfiguration handWritingRecognitionConfiguration;
    private readonly HandWritingRecognitionConfigurationMyScript? myScriptConfiguration;
    private readonly ITabletConfiguration tabletConfiguration;

    internal SettingsViewModel(IHandWritingRecognitionService handWritingRecognitionService, ITabletService tabletService) : base("Settings", "Save", "Cancel")
    {
        this.HandWritingRecognitionLanguages = HandWritingRecognitionLanguageViewModel.GetLanguages(handWritingRecognitionService);

        this.handWritingRecognitionConfiguration = handWritingRecognitionService.Configuration;
        this.myScriptConfiguration = handWritingRecognitionService.Configuration as HandWritingRecognitionConfigurationMyScript;
        this.tabletConfiguration = tabletService.Configuration;

        this.backup = this.tabletConfiguration.Backup;
        this.handWritingRecognitionLanguage = this.HandWritingRecognitionLanguages.Single(language => String.CompareOrdinal(language.Code, this.handWritingRecognitionConfiguration.Language) == 0);
        this.myScriptApplicationKey = this.myScriptConfiguration?.ApplicationKey ?? String.Empty;
        this.myScriptHmacKey = this.myScriptConfiguration?.HmacKey ?? String.Empty;
        this.tabletIp = this.tabletConfiguration.IP;
        this.tabletPassword = this.tabletConfiguration.Password;

        this.CommandSetBackup = ReactiveCommand.CreateFromTask(this.SetBackup);

        this.WhenAnyValue(vm => vm.TabletIp).Subscribe(this.CheckTabletIp);
        this.WhenAnyValue(vm => vm.TabletPassword).Subscribe(this.CheckTabletPassword);
    }

    public ICommand CommandSetBackup { get; }

    public String Backup { get { return this.backup; } private set { this.RaiseAndSetIfChanged(ref this.backup, value); } }

    public HandWritingRecognitionLanguageViewModel HandWritingRecognitionLanguage { get { return this.handWritingRecognitionLanguage; } set { this.RaiseAndSetIfChanged(ref this.handWritingRecognitionLanguage, value); } }

    public IEnumerable<HandWritingRecognitionLanguageViewModel> HandWritingRecognitionLanguages { get; }

    public Boolean HasMyScript { get { return this.myScriptConfiguration != null; } }

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

    protected override async Task<Boolean> OnClose()
    {
        this.handWritingRecognitionConfiguration.Language = this.HandWritingRecognitionLanguage.Code;
        await this.handWritingRecognitionConfiguration.Save().ConfigureAwait(true);

        if (this.myScriptConfiguration != null)
        {
            this.myScriptConfiguration.ApplicationKey = this.MyScriptApplicationKey;
            this.myScriptConfiguration.HmacKey = this.MyScriptHmacKey;
            await this.myScriptConfiguration.Save().ConfigureAwait(true);
        }

        this.tabletConfiguration.Backup = this.Backup;
        this.tabletConfiguration.IP = this.TabletIp;
        this.tabletConfiguration.Password = this.TabletPassword;
        await this.tabletConfiguration.Save().ConfigureAwait(true);

        return await base.OnClose().ConfigureAwait(true);
    }

    [GeneratedRegex("^\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}$")]
    private static partial Regex IpRegex();

    private async Task SetBackup()
    {
        String? backupFolder = await this.OpenFolderPicker.Handle("Backup Folder");
        this.Backup = backupFolder ?? String.Empty;
    }
}
