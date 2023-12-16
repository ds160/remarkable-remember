using System;
using System.ComponentModel.DataAnnotations;

namespace ReMarkableRemember.Entities;

public class Setting
{
    internal static class Keys
    {
        public const String MyScriptApplicationKey = "MyScript ApplicationKey";
        public const String MyScriptHmacKey = "MyScript HmacKey";
        public const String TabletIp = "Tablet IP";
        public const String TabletPassword = "Tablet Password";
    }

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
