using System;
using System.ComponentModel.DataAnnotations;

namespace ReMarkableRemember.Entities;

internal sealed class Setting
{
    public Setting(String key, String value)
    {
        this.Key = key;
        this.Value = value;
    }

    [Key]
    public String Key { get; set; }

    [Required]
    public String Value { get; set; }
}
