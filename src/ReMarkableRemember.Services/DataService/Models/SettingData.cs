using System;

namespace ReMarkableRemember.Services.DataService.Models;

public sealed class SettingData
{
    public SettingData(String prefix, String key, String value)
    {
        this.Key = key;
        this.Prefix = prefix;
        this.Value = value;
    }

    internal String DatabaseKey { get { return $"{this.Prefix} {this.Key}"; } }

    public String Key { get; }

    public String Prefix { get; }

    public String Value { get; set; }
}
