using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReMarkableRemember.Services.DataService;
using ReMarkableRemember.Services.TabletService;
using ReMarkableRemember.Services.TabletService.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class TemplatesViewModel : DialogWindowModel
{
    private readonly ObservableCollection<TemplateViewModel> templates;

    public TemplatesViewModel(IEnumerable<TabletTemplate> templates, IDataService dataService, ITabletService tabletService)
        : base("Templates", "Restore", "Close")
    {
        this.templates = new ObservableCollection<TemplateViewModel>();

        foreach (TabletTemplate template in templates.OrderBy(template => template.Name))
        {
            this.templates.Add(new TemplateViewModel(template, this.templates, dataService, tabletService));
        }

        this.templates.CollectionChanged += (s, e) => this.CheckTemplates(e.Action is NotifyCollectionChangedAction.Remove);
    }

    public Boolean RestartRequired { get; private set; }

    public IEnumerable<TemplateViewModel> Templates { get { return this.templates; } }

    private async void CheckTemplates(Boolean removed)
    {
        this.RestartRequired |= removed;

        if (this.templates.Count == 0)
        {
            await this.CommandCancel.Execute();
        }
    }

    protected override async Task<Boolean> OnClose()
    {
        await Task.WhenAll(this.Templates.Select(template => template.Restore())).ConfigureAwait(true);
        this.RestartRequired |= this.Templates.Any();

        return await base.OnClose().ConfigureAwait(true);
    }
}
