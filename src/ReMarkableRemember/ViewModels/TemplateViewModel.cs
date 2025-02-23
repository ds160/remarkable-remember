using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Svg;
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

    internal TemplateViewModel(TabletTemplate template, ObservableCollection<TemplateViewModel> templates, IDataService dataService, ITabletService tabletService)
    {
        this.template = template;
        this.templates = templates;

        this.dataService = dataService;
        this.tabletService = tabletService;

        this.Icon = icons[template.IconCode];
        this.Image = (IImage?)LoadPng(template.BytesPng) ?? LoadSvg(template.BytesSvg);

        this.CommandDelete = ReactiveCommand.CreateFromTask(this.Delete);
    }

    public ICommand CommandDelete { get; }

    public String Category { get { return this.template.Category; } }

    public TemplateIconViewModel Icon { get; }

    public IImage? Image { get; }

    public String Name { get { return this.template.Name; } }

    private async Task Delete()
    {
        await this.tabletService.DeleteTemplate(this.template).ConfigureAwait(false);
        await this.dataService.DeleteTemplate(this.template.Category, this.template.Name).ConfigureAwait(false);

        this.templates.Remove(this);
    }

    private static Bitmap? LoadPng(Byte[] bytesPng)
    {
        return (bytesPng.Length > 0) ? new Bitmap(new MemoryStream(bytesPng)) : null;
    }

    private static SvgImage? LoadSvg(Byte[] bytesSvg)
    {
        return (bytesSvg.Length > 0) ? new SvgImage() { Source = SvgSource.Load(new MemoryStream(bytesSvg)) } : null;
    }

    public async Task Restore()
    {
        await this.tabletService.UploadTemplate(this.template).ConfigureAwait(false);
    }
}
