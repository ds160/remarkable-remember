using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media.Imaging;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Helper;
using ReMarkableRemember.Services.TabletService.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class TemplateIconViewModel
{
    private static readonly Dictionary<String, Bitmap> images = new Dictionary<String, Bitmap>();

    private TemplateIconViewModel(TabletTemplateIcon icon)
    {
        if (!images.TryGetValue(icon.Code, out Bitmap? image))
        {
            image = ImageLoader.Bitmap($"Templates/{icon.ImageName}.png");
            images.Add(icon.Code, image);
        }
        String name = icon.GetName();

        this.Code = icon.Code;
        this.Image = image;
        this.IsLandscape = icon.IsLandscape;
        this.Name = icon.IsLandscape ? $"{name} ({Language.Current.TemplateLandscape})" : $"{name} ({Language.Current.TemplatePortrait})";
        this.SortName = name;
    }

    public String Code { get; }

    public Bitmap Image { get; }

    private Boolean IsLandscape { get; }

    public String Name { get; }

    private String SortName { get; }

    internal static IEnumerable<TemplateIconViewModel> GetIcons()
    {
        return TabletTemplateIcon.Icons
            .Select(icon => new TemplateIconViewModel(icon))
            .OrderBy(icon => icon.IsLandscape ? 1 : 0)
            .ThenBy(icon => icon.SortName)
            .ToArray();
    }
}
