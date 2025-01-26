using System;

namespace ReMarkableRemember.Services.DataService.Models;

public sealed class TemplateData
{
    public TemplateData(String category, String name, String iconCode, Byte[] bytesPng, Byte[] bytesSvg)
    {
        this.Category = category;
        this.Name = name;
        this.IconCode = iconCode;
        this.BytesPng = bytesPng;
        this.BytesSvg = bytesSvg;
    }

    public String Category { get; }

    public String Name { get; }

    public String IconCode { get; }

    public Byte[] BytesPng { get; }

    public Byte[] BytesSvg { get; }
}