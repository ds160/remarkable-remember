using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ReMarkableRemember.Entities;

[PrimaryKey(nameof(Category), nameof(Name))]
internal sealed class Template
{
    public Template(String category, String name, String iconCode, Byte[] bytesPng, Byte[] bytesSvg)
    {
        this.Category = category;
        this.Name = name;
        this.IconCode = iconCode;
        this.BytesPng = bytesPng;
        this.BytesSvg = bytesSvg;
    }

    [Required]
    public String Category { get; set; }

    [Required]
    public String Name { get; set; }

    [Required]
    public String IconCode { get; set; }

    [Required]
    public Byte[] BytesPng { get; set; }

    [Required]
    public Byte[] BytesSvg { get; set; }
}
