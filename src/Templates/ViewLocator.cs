using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ReactiveUI;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Templates;

public sealed class ViewLocator : IDataTemplate
{
    private static readonly Dictionary<Type, Func<Control>> mapping = new Dictionary<Type, Func<Control>>();

    static ViewLocator()
    {
        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            IEnumerable<Type> interfaceTypes = type.GetInterfaces().Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IViewFor<>));
            foreach (Type interfaceType in interfaceTypes)
            {
                mapping.Add(interfaceType.GetGenericArguments().Single(), () => (Control)Activator.CreateInstance(type)!);
            }
        }
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
