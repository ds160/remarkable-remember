using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Templates;

public class ViewLocator : IDataTemplate
{
    private static readonly Dictionary<Type, Func<Control>> mapping = new Dictionary<Type, Func<Control>>();

    public static void AddMapping<TViewModel, TControl>()
        where TViewModel : ViewModelBase
        where TControl : Control, new()
    {
        mapping.Add(typeof(TViewModel), () => new());
    }

    public Control? Build(Object? param)
    {
        if (param == null) { throw new ArgumentNullException(nameof(param)); }

        Func<Control> createControl = mapping[param.GetType()];
        Control control = createControl();
        control.DataContext = param;
        return control;
    }

    public Boolean Match(Object? data)
    {
        return data is ViewModelBase;
    }
}
