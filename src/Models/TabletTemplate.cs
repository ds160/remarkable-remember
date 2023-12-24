using System;
using System.IO;

namespace ReMarkableRemember.Models;

internal sealed class TabletTemplate
{
    public TabletTemplate(String name, String category, String iconCode, Boolean landscape, String sourceFilePath)
    {
        String directory = Path.GetDirectoryName(sourceFilePath) ?? String.Empty;
        String fileName = Path.GetFileNameWithoutExtension(sourceFilePath);

        this.BytesPng = File.ReadAllBytes(Path.Combine(directory, $"{fileName}.png"));
        this.BytesSvg = File.ReadAllBytes(Path.Combine(directory, $"{fileName}.svg"));
        this.Category = category;
        this.FileName = fileName;
        this.IconCode = iconCode;
        this.Landscape = landscape;
        this.Name = name;
    }

    public Byte[] BytesPng { get; }
    public Byte[] BytesSvg { get; }
    public String Category { get; }
    public String FileName { get; }
    public String IconCode { get; }
    public Boolean Landscape { get; }
    public String Name { get; }
}
