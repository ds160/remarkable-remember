using System;
using System.IO;
using System.Linq;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Services.TabletService.Exceptions;

namespace ReMarkableRemember.Services.TabletService.Models;

public sealed class TabletTemplate
{
    public TabletTemplate(String name, String category, String iconCode, Byte[] bytesPng, Byte[] bytesSvg)
    {
        this.BytesPng = bytesPng;
        this.BytesSvg = bytesSvg;
        this.Category = category;
        this.IconCode = iconCode;
        this.Landscape = TabletTemplateIcon.Icons.Single(icon => icon.Code.Equals(iconCode, StringComparison.Ordinal)).IsLandscape;
        this.Name = name;
    }

    public TabletTemplate(String name, String category, String iconCode, String sourceFilePath)
    {
        String directory = Path.GetDirectoryName(sourceFilePath) ?? String.Empty;
        String fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
        String fileNamePng = Path.Combine(directory, $"{fileName}.png");
        String fileNameSvg = Path.Combine(directory, $"{fileName}.svg");

        this.BytesPng = File.Exists(fileNamePng) ? File.ReadAllBytes(fileNamePng) : Array.Empty<Byte>();
        this.BytesSvg = File.Exists(fileNameSvg) ? File.ReadAllBytes(fileNameSvg) : Array.Empty<Byte>();
        this.Category = category;
        this.IconCode = iconCode;
        this.Name = name;

        if (this.BytesPng.Length == 0 && this.BytesSvg.Length == 0) { throw new TabletException(Language.Current.TabletTemplateImageRequired); }
    }

    public Byte[] BytesPng { get; }

    public Byte[] BytesSvg { get; }

    public String Category { get; }

    public String FileName { get { return $"{this.Category} {this.Name}"; } }

    public String IconCode { get; }

    public Boolean Landscape { get; }

    public String Name { get; }
}
