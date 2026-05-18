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
using ReMarkableRemember.DependencyInjection;
using ReMarkableRemember.Services.TabletService.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class TemplateViewModel : ViewModelBase
{
    private static Dictionary<String, IImage>? icons;

    private readonly TabletTemplate template;
    private readonly ObservableCollection<TemplateViewModel> templates;

    private readonly IServices services;

    internal TemplateViewModel(TabletTemplate template, ObservableCollection<TemplateViewModel> templates, IServices services)
    {
        icons ??= TemplateIconViewModel.GetIcons(services).ToDictionary(icon => icon.Code, icon => icon.Image);

        this.template = template;
        this.templates = templates;

        this.services = services;

        this.Icon = icons[template.IconCode];
        this.Image = (IImage?)LoadPng(template.BytesPng) ?? LoadSvg(template.BytesSvg);

        this.CommandDelete = ReactiveCommand.CreateFromTask(this.Delete);
    }

    public ICommand CommandDelete { get; }

    public String Category { get { return this.template.Category; } }

    public IImage Icon { get; }

    public IImage? Image { get; }

    public String Name { get { return this.template.Name; } }

    private async Task Delete()
    {
        await this.services.Tablet.DeleteTemplate(this.template).ConfigureAwait(false);
        await this.services.Data.DeleteTemplate(this.template.Category, this.template.Name).ConfigureAwait(false);

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
        await this.services.Tablet.UploadTemplate(this.template).ConfigureAwait(false);
    }
}
