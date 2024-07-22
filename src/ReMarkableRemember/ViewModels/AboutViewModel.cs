using System;
using System.Reflection;

namespace ReMarkableRemember.ViewModels;

public sealed class AboutViewModel : DialogWindowModel
{
    public AboutViewModel() : base("About", "GitHub", "Close")
    {
    }

    public static String? Version { get { return Assembly.GetExecutingAssembly().GetName().Version?.ToString(3); } }
}
