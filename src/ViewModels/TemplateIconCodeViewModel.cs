using System;
using System.Collections.Generic;
using System.Reflection;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace ReMarkableRemember.ViewModels;

public sealed class TemplateIconCodeViewModel
{
    private static readonly String? assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

    private TemplateIconCodeViewModel(String code, String name, String image)
    {
        this.Code = code;
        this.Image = new Bitmap(AssetLoader.Open(new Uri($"avares://{assemblyName}/Assets/Templates/{image}.png")));
        this.Name = name;
    }

    public String Code { get; }

    public Bitmap Image { get; }

    public String Name { get; }

    internal static IEnumerable<TemplateIconCodeViewModel> GetIconCodes()
    {
        return new List<TemplateIconCodeViewModel>()
        {
            new TemplateIconCodeViewModel("\uE9FE", "Blank", "E9FE"),
            new TemplateIconCodeViewModel("\uE98F", "Checklist", "E98F"),
            new TemplateIconCodeViewModel("\uEA00", "Isometric", "EA00"),
            new TemplateIconCodeViewModel("\uE9A8", "Lined small", "E9A8"),
        };
    }
}
