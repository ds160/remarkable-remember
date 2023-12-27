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

    private TemplateIconViewModel(String code, String name, String image, Boolean landscape)
    {
        this.Code = code;
        this.Image = new Bitmap(AssetLoader.Open(new Uri($"avares://{assemblyName}/Assets/Templates/{image}.png")));
        this.Landscape = landscape;
        this.Name = name;
    }

    public String Code { get; }

    public Bitmap Image { get; }

    public Boolean Landscape { get; }

    public String Name { get; }

    internal static IEnumerable<TemplateIconViewModel> GetIcons()
    {
        List<TemplateIconViewModel> iconCodes = new List<TemplateIconViewModel>()
        {
            new TemplateIconViewModel("\uE9FE", "Blank", "E9FE", false),
            new TemplateIconViewModel("\uE98F", "Checklist", "E98F", false),
            new TemplateIconViewModel("\uEA00", "Isometric", "EA00", false),
            new TemplateIconViewModel("\uE9A8", "Lined small", "E9A8", false),
        };

        return iconCodes.OrderBy(iconCode => iconCode.Name);
    }
}
