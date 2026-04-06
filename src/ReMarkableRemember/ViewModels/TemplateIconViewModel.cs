using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Services.TabletService.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class TemplateIconViewModel
{
    private static readonly String? assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

    private TemplateIconViewModel(TabletTemplateIcon icon)
    {
        String name = icon.GetName();

        this.Code = icon.Code;
        this.Image = new Bitmap(AssetLoader.Open(new Uri($"avares://{assemblyName}/Assets/Templates/{icon.ImageName}.png")));
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
