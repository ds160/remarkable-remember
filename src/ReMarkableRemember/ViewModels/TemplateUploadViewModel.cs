using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using ReactiveUI;
using ReMarkableRemember.Helper;
using ReMarkableRemember.Services.DataService;
using ReMarkableRemember.Services.DataService.Models;
using ReMarkableRemember.Services.TabletService;
using ReMarkableRemember.Services.TabletService.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class TemplateUploadViewModel : DialogWindowModel
{
    private readonly IDataService dataService;
    private readonly ITabletService tabletService;

    public TemplateUploadViewModel(IDataService dataService, ITabletService tabletService) : base("Template", "Upload", "Cancel")
    {
        this.dataService = dataService;
        this.tabletService = tabletService;

        this.Icons = TemplateIconViewModel.GetIcons();

        this.Category = String.Empty;
        this.Icon = this.Icons.First();
        this.Name = String.Empty;
        this.SourceFilePath = String.Empty;

        this.CommandSetSourceFilePath = ReactiveCommand.CreateFromTask(this.SetSourceFilePath);

        this.WhenAnyValue(vm => vm.Category).Subscribe(value => this.CheckProperty(value, nameof(this.Category)));
        this.WhenAnyValue(vm => vm.Name).Subscribe(value => this.CheckProperty(value, nameof(this.Name)));
        this.WhenAnyValue(vm => vm.SourceFilePath).Subscribe(value => this.CheckProperty(value, nameof(this.SourceFilePath), "Source File"));
    }

    public ICommand CommandSetSourceFilePath { get; }

    public String Category { get; set { this.RaiseAndSetIfChanged(ref field, value); } }

    public TemplateIconViewModel Icon { get; set { this.RaiseAndSetIfChanged(ref field, value); } }

    public IEnumerable<TemplateIconViewModel> Icons { get; }

    public String Name { get; set { this.RaiseAndSetIfChanged(ref field, value); } }

    public String SourceFilePath { get; private set { this.RaiseAndSetIfChanged(ref field, value); } }

    private void CheckProperty(String value, String propertyName, String? displayName = null)
    {
        this.ClearErrors(propertyName);

        if (String.IsNullOrEmpty(value))
        {
            this.AddError(propertyName, $"{displayName ?? propertyName} is required");
        }
    }

    protected override async Task<Boolean> OnClose()
    {
        TabletTemplate tabletTemplate = new TabletTemplate(this.Name, this.Category, this.Icon.Code, this.SourceFilePath);
        await this.tabletService.UploadTemplate(tabletTemplate).ConfigureAwait(true);

        TemplateData dataTemplate = new TemplateData(tabletTemplate.Category, tabletTemplate.Name, tabletTemplate.IconCode, tabletTemplate.BytesPng, tabletTemplate.BytesSvg);
        await this.dataService.SetTemplate(dataTemplate).ConfigureAwait(true);

        return await base.OnClose().ConfigureAwait(true);
    }

    private async Task SetSourceFilePath()
    {
        FilePickerOpenOptions options = new FilePickerOpenOptions() { AllowMultiple = false, Title = "Template", FileTypeFilter = new[] { FilePickerFileTypes.ImagePng, FilePickerFileTypesExtensions.ImageSvg } };
        IEnumerable<String>? files = await this.OpenFilePicker.Handle(options);
        String? file = files?.SingleOrDefault();
        if (file != null)
        {
            this.SourceFilePath = file;
        }
    }
}
