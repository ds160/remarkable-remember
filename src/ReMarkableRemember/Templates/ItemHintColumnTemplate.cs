using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ReMarkableRemember.Services.LocalizationService;
using ReMarkableRemember.ViewModels;
using ReMarkableRemember.Views;

namespace ReMarkableRemember.Templates;

public sealed class ItemHintColumnTemplate : IDataTemplate
{
    private readonly Func<ItemViewModel, DateTime?> dateTime;
    private readonly Func<ItemViewModel, ItemViewModel.Hint> hint;
    private readonly ILocalizationService localizationService;

    public ItemHintColumnTemplate(ILocalizationService localizationService, Func<ItemViewModel, DateTime?> dateTime, Func<ItemViewModel, ItemViewModel.Hint> hint)
    {
        this.dateTime = dateTime;
        this.hint = hint;
        this.localizationService = localizationService;
    }

    public Control? Build(Object? param)
    {
        ItemViewModel item = param as ItemViewModel ?? throw new ArgumentNullException(nameof(param));
        return new ItemHintColumnView(this.localizationService, item, this.dateTime, this.hint);
    }

    public Boolean Match(Object? data)
    {
        return data is ItemViewModel;
    }
}
