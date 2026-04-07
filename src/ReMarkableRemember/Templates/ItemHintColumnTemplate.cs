using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ReMarkableRemember.Settings;
using ReMarkableRemember.ViewModels;
using ReMarkableRemember.Views;

namespace ReMarkableRemember.Templates;

public sealed class ItemHintColumnTemplate : IDataTemplate
{
    private readonly Func<ItemViewModel, DateTime?> dateTime;
    private readonly Func<ItemViewModel, ItemViewModel.Hint> hint;
    private readonly ISettingsService settingsService;

    public ItemHintColumnTemplate(ISettingsService settingsService, Func<ItemViewModel, DateTime?> dateTime, Func<ItemViewModel, ItemViewModel.Hint> hint)
    {
        this.dateTime = dateTime;
        this.hint = hint;
        this.settingsService = settingsService;
    }

    public Control? Build(Object? param)
    {
        ItemViewModel item = param as ItemViewModel ?? throw new ArgumentNullException(nameof(param));
        return new ItemHintColumnView(this.settingsService, item, this.dateTime, this.hint);
    }

    public Boolean Match(Object? data)
    {
        return data is ItemViewModel;
    }
}
