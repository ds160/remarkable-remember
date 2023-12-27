using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;

namespace ReMarkableRemember.ViewModels;

public sealed partial class TemplateViewModel : DialogWindowModel
{
    private String category;
    private TemplateIconCodeViewModel iconCode;
    private Boolean landscape;
    private String name;
    private String sourceFilePath;

    public TemplateViewModel() : base("Template", "Upload", true)
    {
        this.IconCodes = TemplateIconCodeViewModel.GetIconCodes();

        this.category = String.Empty;
        this.iconCode = this.IconCodes.First();
        this.landscape = false;
        this.name = String.Empty;
        this.sourceFilePath = String.Empty;

        this.CommandSetSourceFilePath = ReactiveCommand.CreateFromTask(this.SetSourceFilePath);

        this.WhenAnyValue(vm => vm.Category).Subscribe(value => this.CheckProperty(value, nameof(this.Category)));
        this.WhenAnyValue(vm => vm.Name).Subscribe(value => this.CheckProperty(value, nameof(this.Name)));
        this.WhenAnyValue(vm => vm.SourceFilePath).Subscribe(this.CheckSourceFilePath);
    }

    public ReactiveCommand<Unit, Unit> CommandSetSourceFilePath { get; }

    public String Category { get { return this.category; } set { this.RaiseAndSetIfChanged(ref this.category, value); } }

    public TemplateIconCodeViewModel IconCode { get { return this.iconCode; } set { this.RaiseAndSetIfChanged(ref this.iconCode, value); } }

    public IEnumerable<TemplateIconCodeViewModel> IconCodes { get; }

    public Boolean Landscape { get { return this.landscape; } set { this.RaiseAndSetIfChanged(ref this.landscape, value); } }

    public String Name { get { return this.name; } set { this.RaiseAndSetIfChanged(ref this.name, value); } }

    public String SourceFilePath { get { return this.sourceFilePath; } private set { this.RaiseAndSetIfChanged(ref this.sourceFilePath, value); } }

    private void CheckProperty(String value, String propertyName)
    {
        this.ClearErrors(propertyName);

        if (String.IsNullOrEmpty(value))
        {
            this.AddError(propertyName, $"{propertyName} is required");
        }
    }

    private void CheckSourceFilePath(String filePath)
    {
        this.ClearErrors(nameof(this.SourceFilePath));

        if (String.IsNullOrEmpty(filePath))
        {
            this.AddError(nameof(this.SourceFilePath), "Source File is required");
        }
        else if (!File.Exists(filePath))
        {
            this.AddError(nameof(this.SourceFilePath), "Source File not found");
        }
    }

    private async Task SetSourceFilePath()
    {
        String? filePath = await this.OpenFilePicker.Handle("Template");
        if (filePath != null)
        {
            this.SourceFilePath = filePath;
        }
    }
}
