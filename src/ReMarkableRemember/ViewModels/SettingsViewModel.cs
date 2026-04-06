using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Services.HandWritingRecognitionService;
using ReMarkableRemember.Services.HandWritingRecognitionService.Configuration;
using ReMarkableRemember.Services.LocalizationService;
using ReMarkableRemember.Services.LocalizationService.Configuration;
using ReMarkableRemember.Services.TabletService;
using ReMarkableRemember.Services.TabletService.Configuration;

namespace ReMarkableRemember.ViewModels;

public sealed partial class SettingsViewModel : DialogWindowModel
{
    private readonly IHandWritingRecognitionConfiguration handWritingRecognitionConfiguration;
    private readonly ILocalizationConfiguration localizationConfiguration;
    private readonly HandWritingRecognitionConfigurationMyScript? myScriptConfiguration;
    private readonly ITabletConfiguration tabletConfiguration;

    internal SettingsViewModel(IHandWritingRecognitionService handWritingRecognitionService, ILocalizationService localizationService, ITabletService tabletService)
        : base(Language.Current.SettingsTitle, Language.Current.ButtonSave, Language.Current.ButtonCancel)
    {
        this.HandWritingRecognitionLanguages = HandWritingRecognitionLanguageViewModel.GetLanguages(handWritingRecognitionService, localizationService);
        this.LanguageCultureCodes = LanguageCultureCodeViewModel.GetLanguages(Language.Current.SettingsLanguageDefault);

        this.handWritingRecognitionConfiguration = handWritingRecognitionService.Configuration;
        this.localizationConfiguration = localizationService.Configuration;
        this.myScriptConfiguration = handWritingRecognitionService.Configuration as HandWritingRecognitionConfigurationMyScript;
        this.tabletConfiguration = tabletService.Configuration;

        this.Backup = this.tabletConfiguration.Backup;
        this.HandWritingRecognitionLanguage = this.HandWritingRecognitionLanguages.Single(language => String.Equals(language.Code, this.handWritingRecognitionConfiguration.Language, StringComparison.Ordinal));
        this.LanguageCultureCode = this.LanguageCultureCodes.Single(language => String.Equals(language.CultureCode, this.localizationConfiguration.CultureCode, StringComparison.Ordinal));
        this.MyScriptApplicationKey = this.myScriptConfiguration?.ApplicationKey ?? String.Empty;
        this.MyScriptHmacKey = this.myScriptConfiguration?.HmacKey ?? String.Empty;
        this.TabletIp = this.tabletConfiguration.IP;
        this.TabletPassword = this.tabletConfiguration.Password;

        this.CommandSetBackup = ReactiveCommand.CreateFromTask(this.SetBackup);

        this.WhenAnyValue(vm => vm.TabletIp).Subscribe(this.CheckTabletIp);
        this.WhenAnyValue(vm => vm.TabletPassword).Subscribe(this.CheckTabletPassword);
    }

    public ICommand CommandSetBackup { get; }

    public String Backup { get; private set { this.RaiseAndSetIfChanged(ref field, value); } }

    public HandWritingRecognitionLanguageViewModel HandWritingRecognitionLanguage { get; set { this.RaiseAndSetIfChanged(ref field, value); } }

    public IEnumerable<HandWritingRecognitionLanguageViewModel> HandWritingRecognitionLanguages { get; }

    public Boolean HasMyScript { get { return this.myScriptConfiguration != null; } }

    public LanguageCultureCodeViewModel LanguageCultureCode { get; set { this.RaiseAndSetIfChanged(ref field, value); } }

    public IEnumerable<LanguageCultureCodeViewModel> LanguageCultureCodes { get; }

    public String MyScriptApplicationKey { get; set { this.RaiseAndSetIfChanged(ref field, value); } }

    public String MyScriptHmacKey { get; set { this.RaiseAndSetIfChanged(ref field, value); } }

    public String TabletIp { get; set { this.RaiseAndSetIfChanged(ref field, value); } }

    public String TabletPassword { get; set { this.RaiseAndSetIfChanged(ref field, value); } }

    private void CheckTabletIp(String host)
    {
        this.ClearErrors(nameof(this.TabletIp));

        if (String.IsNullOrEmpty(host)) { return; }
        if (IpRegex().IsMatch(host)) { return; }

        this.AddError(nameof(this.TabletIp), Language.Current.SettingsTabletIpInvalid);
    }

    private void CheckTabletPassword(String password)
    {
        this.ClearErrors(nameof(this.TabletPassword));

        if (String.IsNullOrEmpty(password))
        {
            this.AddError(nameof(this.TabletPassword), Language.Current.SettingsTabletPasswordRequired);
        }
    }

    protected override async Task<Boolean> OnClose()
    {
        this.handWritingRecognitionConfiguration.Language = this.HandWritingRecognitionLanguage.Code;
        await this.handWritingRecognitionConfiguration.Save().ConfigureAwait(true);

        this.localizationConfiguration.CultureCode = this.LanguageCultureCode.CultureCode;
        await this.localizationConfiguration.Save().ConfigureAwait(true);

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
        String? backupFolder = await this.OpenFolderPicker.Handle(Language.Current.SettingsBackupFolder);
        this.Backup = backupFolder ?? String.Empty;
    }
}
