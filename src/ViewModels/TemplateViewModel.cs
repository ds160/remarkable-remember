using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Media.Imaging;
using ReMarkableRemember.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class TemplateViewModel
{
    private static readonly Dictionary<String, TemplateIconViewModel> icons = TemplateIconViewModel.GetIcons().ToDictionary(icon => icon.Code);

    private readonly TabletTemplate template;

    internal TemplateViewModel(TabletTemplate template)
    {
        this.template = template;

        this.Icon = icons[template.IconCode];
        this.Image = new Bitmap(new MemoryStream(template.BytesPng));
    }

    public String Category { get { return this.template.Category; } }

    public TemplateIconViewModel Icon { get; }

    public Bitmap Image { get; }

    public String Name { get { return this.template.Name; } }
}
