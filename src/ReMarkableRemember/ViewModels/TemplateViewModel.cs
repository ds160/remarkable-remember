using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ReMarkableRemember.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class TemplateViewModel
{
    private static readonly Dictionary<String, TemplateIconViewModel> icons = TemplateIconViewModel.GetIcons().ToDictionary(icon => icon.Code);

    private readonly TabletTemplate template;
    private readonly ObservableCollection<TemplateViewModel> templates;

    internal TemplateViewModel(TabletTemplate template, ObservableCollection<TemplateViewModel> templates)
    {
        this.template = template;
        this.templates = templates;

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
        await this.template.Delete().ConfigureAwait(false);

        this.templates.Remove(this);
    }

    public async Task Restore()
    {
        await this.template.Restore().ConfigureAwait(false);
    }
}
