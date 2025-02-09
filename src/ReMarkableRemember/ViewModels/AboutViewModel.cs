using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace ReMarkableRemember.ViewModels;

public sealed class AboutViewModel : DialogWindowModel
{
    public AboutViewModel() : base("About", "GitHub", "Close")
    {
    }

    public static String? Version { get { return Assembly.GetExecutingAssembly().GetName().Version?.ToString(3); } }

    protected override Task<Boolean> OnClose()
    {
        Process.Start(new ProcessStartInfo("https://github.com/ds160/remarkable-remember") { UseShellExecute = true });

        return base.OnClose();
    }
}
