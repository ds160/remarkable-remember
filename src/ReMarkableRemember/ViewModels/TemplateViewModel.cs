using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReMarkableRemember.Services.DataService;
using ReMarkableRemember.Services.TabletService;
using ReMarkableRemember.Services.TabletService.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class TemplateViewModel
{
    private static readonly Dictionary<String, TemplateIconViewModel> icons = TemplateIconViewModel.GetIcons().ToDictionary(icon => icon.Code);

    private readonly TabletTemplate template;
    private readonly ObservableCollection<TemplateViewModel> templates;

    private readonly IDataService dataService;
    private readonly ITabletService tabletService;

    internal TemplateViewModel(TabletTemplate template, ObservableCollection<TemplateViewModel> templates, ServiceProvider services)
    {
        this.template = template;
        this.templates = templates;

        this.dataService = services.GetRequiredService<IDataService>();
        this.tabletService = services.GetRequiredService<ITabletService>();

        this.Icon = icons[template.IconCode];
        this.Image = new Bitmap(new MemoryStream(template.BytesPng));

        this.CommandDelete = ReactiveCommand.CreateFromTask(this.Delete);
    }

    public ICommand CommandDelete { get; }

    public String Category { get { return this.template.Category; } }

    public TemplateIconViewModel Icon { get; }

    public Bitmap Image { get; }

    public String Name { get { return this.template.Name; } }

    private async Task Delete()
    {
        await this.tabletService.DeleteTemplate(this.template).ConfigureAwait(false);
        await this.dataService.DeleteTemplate(this.template.Category, this.template.Name).ConfigureAwait(false);

        this.templates.Remove(this);
    }

    public async Task Restore()
    {
        await this.tabletService.UploadTemplate(this.template).ConfigureAwait(false);
    }
}
