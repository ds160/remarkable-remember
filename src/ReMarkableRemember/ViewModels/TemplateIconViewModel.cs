using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReMarkableRemember.Services.TabletService.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class TemplateIconViewModel
{
    private static readonly String? assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

    private readonly TabletTemplateIcon icon;

    private TemplateIconViewModel(TabletTemplateIcon icon)
    {
        this.icon = icon;

        String imageName = icon.IsLandscape ? $"LS {icon.Name}" : $"P {icon.Name}";

        this.Code = icon.Code;
        this.Image = new Bitmap(AssetLoader.Open(new Uri($"avares://{assemblyName}/Assets/Templates/{imageName}.png")));
    }

    public String Code { get; }

    public Bitmap Image { get; }

    public String Name
    {
        get
        {
#warning Translate
            return this.icon.IsLandscape ? $"{this.icon.Name} (Landscape)" : $"{this.icon.Name} (Portrait)";
        }
    }

    internal static IEnumerable<TemplateIconViewModel> GetIcons()
    {
        return TabletTemplateIcon.Icons.Select(icon => new TemplateIconViewModel(icon));
    }
}
