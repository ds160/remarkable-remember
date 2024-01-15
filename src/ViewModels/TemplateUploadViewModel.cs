using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using ReactiveUI;

namespace ReMarkableRemember.ViewModels;

public sealed class TemplateUploadViewModel : DialogWindowModel
{
    private String category;
    private TemplateIconViewModel icon;
    private String name;
    private String sourceFilePath;

    public TemplateUploadViewModel() : base("Template", "Upload", true)
    {
        this.Icons = TemplateIconViewModel.GetIcons();

        this.category = String.Empty;
        this.icon = this.Icons.First();
        this.name = String.Empty;
        this.sourceFilePath = String.Empty;

        this.CommandSetSourceFilePath = ReactiveCommand.CreateFromTask(this.SetSourceFilePath);

        this.WhenAnyValue(vm => vm.Category).Subscribe(value => this.CheckProperty(value, nameof(this.Category)));
        this.WhenAnyValue(vm => vm.Name).Subscribe(value => this.CheckProperty(value, nameof(this.Name)));
        this.WhenAnyValue(vm => vm.SourceFilePath).Subscribe(value => this.CheckProperty(value, nameof(this.SourceFilePath), "Source File"));
    }

    public ReactiveCommand<Unit, Unit> CommandSetSourceFilePath { get; }

    public String Category { get { return this.category; } set { this.RaiseAndSetIfChanged(ref this.category, value); } }

    public TemplateIconViewModel Icon { get { return this.icon; } set { this.RaiseAndSetIfChanged(ref this.icon, value); } }

    public IEnumerable<TemplateIconViewModel> Icons { get; }

    public String Name { get { return this.name; } set { this.RaiseAndSetIfChanged(ref this.name, value); } }

    public String SourceFilePath { get { return this.sourceFilePath; } private set { this.RaiseAndSetIfChanged(ref this.sourceFilePath, value); } }

    private void CheckProperty(String value, String propertyName, String? displayName = null)
    {
        this.ClearErrors(propertyName);

        if (String.IsNullOrEmpty(value))
        {
            this.AddError(propertyName, $"{displayName ?? propertyName} is required");
        }
    }

    private async Task SetSourceFilePath()
    {
        FilePickerOpenOptions options = new FilePickerOpenOptions() { AllowMultiple = false, Title = "Template", FileTypeFilter = new[] { FilePickerFileTypes.ImagePng } };
        IEnumerable<String>? files = await this.OpenFilePicker.Handle(options);
        String? file = files?.SingleOrDefault();
        if (file != null)
        {
            this.SourceFilePath = file;
        }
    }
}
