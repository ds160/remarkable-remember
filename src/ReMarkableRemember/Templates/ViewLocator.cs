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
    private static readonly Dictionary<Type, Type> mapping = new Dictionary<Type, Type>();

    static ViewLocator()
    {
        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            IEnumerable<Type> interfaceTypes = type.GetInterfaces().Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IViewFor<>));
            foreach (Type interfaceType in interfaceTypes)
            {
                mapping.Add(interfaceType.GetGenericArguments().Single(), type);
            }
        }
    }

    public Control? Build(Object? viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        Type viewType = mapping[viewModel.GetType()];
        Control view = Activator.CreateInstance(viewType) as Control ?? throw new NotSupportedException();
        view.DataContext = viewModel;
        return view;
    }

    public Boolean Match(Object? data)
    {
        return data is ViewModelBase;
    }
}
