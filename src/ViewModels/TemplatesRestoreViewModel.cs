using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using ReMarkableRemember.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class TemplatesRestoreViewModel : DialogWindowModel
{
    public TemplatesRestoreViewModel(IEnumerable<TabletTemplate> templates) : base("Templates", "Restore", true)
    {
        this.Templates = templates.Select(template => new TemplateViewModel(template)).OrderBy(template => template.Name).ToArray();
    }

    public IEnumerable<TemplateViewModel> Templates { get; }
}
