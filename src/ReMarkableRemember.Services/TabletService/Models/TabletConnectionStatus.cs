namespace ReMarkableRemember.Services.TabletService.Models;

public sealed class TabletConnectionStatus
{
    public static readonly TabletConnectionStatus Default = new TabletConnectionStatus(null, TabletError.Unknown);

    internal TabletConnectionStatus(TabletType? type, TabletError? error)
    {
        this.Error = error;
        this.Type = type;
    }

    public TabletError? Error { get; }

    public TabletType? Type { get; }
}
