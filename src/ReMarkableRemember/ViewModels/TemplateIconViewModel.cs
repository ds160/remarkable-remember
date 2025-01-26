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

    private TemplateIconViewModel(String iconCode)
    {
        Boolean landscape = TabletTemplate.IsLandscape(iconCode);
        String name = TabletTemplate.GetIconCodeName(iconCode);
        String prefix = landscape ? "LS" : "P";

        this.Code = iconCode;
        this.Image = new Bitmap(AssetLoader.Open(new Uri($"avares://{assemblyName}/Assets/Templates/{prefix} {name}.png")));
        this.Landscape = landscape;
        this.Name = name;
    }

    public String Code { get; }

    public Bitmap Image { get; }

    public Boolean Landscape { get; }

    public String Name { get; }

    internal static IEnumerable<TemplateIconViewModel> GetIcons()
    {
        return TabletTemplate.IconCodes
            .Select(iconCode => new TemplateIconViewModel(iconCode))
            .OrderBy(icon => icon.Landscape ? 1 : 0)
            .ThenBy(icon => icon.Name)
            .ToArray();
    }
}
