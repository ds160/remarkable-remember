using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.DependencyInjection;
using ReMarkableRemember.Services.TabletService.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class TemplateIconViewModel
{
    private TemplateIconViewModel(TabletTemplateIcon icon, IServices services)
    {
        String name = icon.GetName();

        this.Code = icon.Code;
        this.Image = services.ImageLoader.Bitmap($"Templates/{icon.ImageName}.png");
        this.IsLandscape = icon.IsLandscape;
        this.Name = icon.IsLandscape ? $"{name} ({Language.Current.TemplateLandscape})" : $"{name} ({Language.Current.TemplatePortrait})";
        this.SortName = name;
    }

    public String Code { get; }

    public IImage Image { get; }

    private Boolean IsLandscape { get; }

    public String Name { get; }

    private String SortName { get; }

    internal static IEnumerable<TemplateIconViewModel> GetIcons(IServices services)
    {
        return TabletTemplateIcon.Icons
            .Select(icon => new TemplateIconViewModel(icon, services))
            .OrderBy(icon => icon.IsLandscape ? 1 : 0)
            .ThenBy(icon => icon.SortName)
            .ToArray();
    }
}
