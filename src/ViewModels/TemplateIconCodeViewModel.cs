using System;
using System.Collections.Generic;

namespace ReMarkableRemember.ViewModels;

public sealed class TemplateIconCodeViewModel
{
    private TemplateIconCodeViewModel(String code, String name)
    {
        this.Code = code;
        this.Name = name;
    }

    public String Code { get; }

    public String Name { get; }

    internal static IEnumerable<TemplateIconCodeViewModel> GetIconCodes()
    {
        return new List<TemplateIconCodeViewModel>()
        {
            new TemplateIconCodeViewModel("\uE9FE", "Blank"),
            new TemplateIconCodeViewModel("\uE98F", "Checklist"),
            new TemplateIconCodeViewModel("\uEA00", "Isometric"),
            new TemplateIconCodeViewModel("\uE9A8", "Lined small"),
        };
    }
}
