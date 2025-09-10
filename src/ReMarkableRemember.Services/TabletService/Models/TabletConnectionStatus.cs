namespace ReMarkableRemember.Services.TabletService.Models;

public sealed class TabletConnectionStatus
{
    public static readonly TabletConnectionStatus Default = new TabletConnectionStatus(null, TabletError.Unknown);

    internal TabletConnectionStatus(TabletInformation? information, TabletError? error)
    {
        this.Error = error;
        this.Information = information;
    }

    public TabletError? Error { get; }

    public TabletInformation? Information { get; }
}
