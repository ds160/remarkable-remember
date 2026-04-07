using System;
using System.Collections.Generic;
using System.Linq;

namespace ReMarkableRemember.ViewModels;

public sealed class EnumViewModel
{
    private EnumViewModel(String value, String displayName)
    {
        this.DisplayName = displayName;
        this.Value = value;
    }

    public String DisplayName { get; }

    public String Value { get; }

    internal static IEnumerable<EnumViewModel> GetValues<T>(Func<T, String> displayName) where T : struct, Enum
    {
        return Enum.GetValues<T>()
            .Select(value => new EnumViewModel(Enum.GetName(value) ?? throw new NotImplementedException(), displayName(value)))
            .OrderBy(value => value.DisplayName)
            .ToArray();
    }
}
