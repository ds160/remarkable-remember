using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Enumerations;
using ReMarkableRemember.Services.HandWritingRecognitionService;
using ReMarkableRemember.Services.HandWritingRecognitionService.Configuration;
using ReMarkableRemember.Services.TabletService;
using ReMarkableRemember.Services.TabletService.Configuration;
using ReMarkableRemember.Settings;
using ReMarkableRemember.Settings.Configuration;

namespace ReMarkableRemember.ViewModels;

public sealed partial class SettingsViewModel : DialogWindowModel
{
    private readonly IHandWritingRecognitionConfiguration handWritingRecognitionConfiguration;
    private readonly HandWritingRecognitionConfigurationMyScript? myScriptConfiguration;
    private readonly ISettingsConfiguration settingsService;
    private readonly ITabletConfiguration tabletConfiguration;

    internal SettingsViewModel(IHandWritingRecognitionService handWritingRecognitionService, ISettingsService settingsService, ITabletService tabletService)
        : base(Language.Current.SettingsTitle, Language.Current.ButtonSave, Language.Current.ButtonCancel)
    {
        this.ApplicationLanguages = ApplicationLanguageViewModel.GetLanguages(Language.Current.SettingsLanguageDefault);
        this.ApplicationThemes = EnumValues<ApplicationThemes>(EnumValuesApplicationThemes, settingsService.Configuration.ApplicationTheme, out EnumViewModel applicationTheme);
        this.HandWritingRecognitionLanguages = HandWritingRecognitionLanguageViewModel.GetLanguages(handWritingRecognitionService, settingsService);

        this.handWritingRecognitionConfiguration = handWritingRecognitionService.Configuration;
        this.myScriptConfiguration = handWritingRecognitionService.Configuration as HandWritingRecognitionConfigurationMyScript;
        this.settingsService = settingsService.Configuration;
        this.tabletConfiguration = tabletService.Configuration;

        this.ApplicationLanguage = this.ApplicationLanguages.Single(language => String.Equals(language.Code, this.settingsService.ApplicationLanguage, StringComparison.Ordinal));
        this.ApplicationTheme = applicationTheme;
        this.Backup = this.tabletConfiguration.Backup;
        this.HandWritingRecognitionLanguage = this.HandWritingRecognitionLanguages.Single(language => String.Equals(language.Code, this.handWritingRecognitionConfiguration.Language, StringComparison.Ordinal));
        this.MyScriptApplicationKey = this.myScriptConfiguration?.ApplicationKey ?? String.Empty;
        this.MyScriptHmacKey = this.myScriptConfiguration?.HmacKey ?? String.Empty;
        this.TabletIp = this.tabletConfiguration.IP;
        this.TabletPassword = this.tabletConfiguration.Password;

        this.CommandSetBackup = ReactiveCommand.CreateFromTask(this.SetBackup);

        this.WhenAnyValue(vm => vm.TabletIp).Subscribe(this.CheckTabletIp);
        this.WhenAnyValue(vm => vm.TabletPassword).Subscribe(this.CheckTabletPassword);
    }

    public ICommand CommandSetBackup { get; }

    public ApplicationLanguageViewModel ApplicationLanguage { get; set { this.RaiseAndSetIfChanged(ref field, value); } }

    public IEnumerable<ApplicationLanguageViewModel> ApplicationLanguages { get; }

    public EnumViewModel ApplicationTheme { get; set { this.RaiseAndSetIfChanged(ref field, value); } }

    public IEnumerable<EnumViewModel> ApplicationThemes { get; }

    public String Backup { get; private set { this.RaiseAndSetIfChanged(ref field, value); } }

    public HandWritingRecognitionLanguageViewModel HandWritingRecognitionLanguage { get; set { this.RaiseAndSetIfChanged(ref field, value); } }

    public IEnumerable<HandWritingRecognitionLanguageViewModel> HandWritingRecognitionLanguages { get; }

    public Boolean HasMyScript { get { return this.myScriptConfiguration != null; } }

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

    private static IEnumerable<EnumViewModel> EnumValues<T>(Func<T, String> displayName, String valueToFind, out EnumViewModel selectViewModel)
        where T : struct, Enum
    {
        IEnumerable<EnumViewModel> values = EnumViewModel.GetValues(displayName);

        selectViewModel = values.Single(vm => String.Equals(valueToFind, vm.Value, StringComparison.Ordinal));

        return values;
    }

    private static String EnumValuesApplicationThemes(ApplicationThemes theme)
    {
        return theme switch
        {
            Enumerations.ApplicationThemes.Default => Language.Current.SettingsApplicationThemeDefault,
            Enumerations.ApplicationThemes.Light => Language.Current.SettingsApplicationThemeLight,
            Enumerations.ApplicationThemes.Dark => Language.Current.SettingsApplicationThemeDark,
            _ => throw new NotImplementedException(),
        };
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

        this.settingsService.ApplicationLanguage = this.ApplicationLanguage.Code;
        this.settingsService.ApplicationTheme = this.ApplicationTheme.Value;
        await this.settingsService.Save().ConfigureAwait(true);

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
