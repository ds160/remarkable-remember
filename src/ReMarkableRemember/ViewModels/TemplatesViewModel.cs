using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.Extensions.DependencyInjection;
using ReMarkableRemember.Services.TabletService.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class TemplatesViewModel : DialogWindowModel
{
    private readonly ObservableCollection<TemplateViewModel> templates;

    public TemplatesViewModel(IEnumerable<TabletTemplate> templates, ServiceProvider services)
        : base("Templates", "Restore", "Cancel")
    {
        this.templates = new ObservableCollection<TemplateViewModel>();

        foreach (TabletTemplate template in templates.OrderBy(template => template.Name))
        {
            this.templates.Add(new TemplateViewModel(template, this.templates, services));
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
}
