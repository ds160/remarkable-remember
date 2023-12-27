using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace ReMarkableRemember.ViewModels;

public sealed class TemplateIconViewModel
{
    private static readonly String? assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

    private TemplateIconViewModel(String code, String key)
    {
        this.Code = code;
        this.Image = new Bitmap(AssetLoader.Open(new Uri($"avares://{assemblyName}/Assets/Templates/{key}.png")));

        if (key.StartsWith("LS ", StringComparison.OrdinalIgnoreCase))
        {
            this.Landscape = true;
            this.Name = key[3..];
        }
        else if (key.StartsWith("P ", StringComparison.OrdinalIgnoreCase))
        {
            this.Landscape = false;
            this.Name = key[2..];
        }
        else
        {
            throw new ArgumentException("Invalid icon key defined.", nameof(key));
        }
    }

    public String Code { get; }

    public Bitmap Image { get; }

    public Boolean Landscape { get; }

    public String Name { get; }

    internal static IEnumerable<TemplateIconViewModel> GetIcons()
    {
        List<TemplateIconViewModel> icons = new List<TemplateIconViewModel>()
        {
            new TemplateIconViewModel("\uE970", "LS Piano sheet large"),
            new TemplateIconViewModel("\uE975", "LS Piano sheet medium"),
            new TemplateIconViewModel("\uE976", "LS Piano sheet small"),
            new TemplateIconViewModel("\uE977", "P Piano sheet large"),
            new TemplateIconViewModel("\uE978", "P Piano sheet medium"),
            new TemplateIconViewModel("\uE979", "P Piano sheet small"),
        };

        return icons.OrderBy(icon => icon.Landscape ? 1 : 0).ThenBy(icon => icon.Name);
    }
}
