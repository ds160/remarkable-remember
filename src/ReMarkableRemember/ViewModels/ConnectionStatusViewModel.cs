using System;
using ReMarkableRemember.Enumerations;
using ReMarkableRemember.Services.TabletService.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class ConnectionStatusViewModel
{
    private readonly TabletConnectionStatus connectionStatus;

    public ConnectionStatusViewModel() : this(TabletConnectionStatus.Default)
    {
    }

    public ConnectionStatusViewModel(TabletConnectionStatus connectionStatus)
    {
        this.connectionStatus = connectionStatus;

        this.Text = this.connectionStatus.Error switch
        {
            null => "Connected",
            TabletError.NotSupported => "Connected reMarkable not supported",
            TabletError.Unknown => "Not connected",
            TabletError.SshNotConfigured => "SSH protocol information are not configured or wrong",
            TabletError.SshNotConnected => "Not connected via WiFi or USB",
            TabletError.UsbNotActived => "USB connection is not activated",
            TabletError.UsbNotConnected => "Not connected via USB",
            _ => "Not connected",
        };
    }

    public Boolean IsConnected
    {
        get { return this.connectionStatus.Error is null; }
    }

    public String Text { get; }

    public Boolean CheckJob(Jobs job)
    {
        switch (job)
        {
            case Jobs.None:
            case Jobs.SetSyncTargetDirectory:
            case Jobs.Settings:
                return true;

            case Jobs.GetItems:
            case Jobs.Backup:
            case Jobs.HandwritingRecognition:
            case Jobs.UploadTemplate:
            case Jobs.ManageTemplates:
            case Jobs.InstallLamyEraser:
            case Jobs.InstallWebInterfaceOnBoot:
                return this.connectionStatus.Error is null or (not TabletError.NotSupported and not TabletError.Unknown and not TabletError.SshNotConfigured and not TabletError.SshNotConnected);

            case Jobs.Sync:
            case Jobs.Download:
            case Jobs.Upload:
                return this.connectionStatus.Error is null;

            default:
                throw new NotImplementedException();
        }
    }
}
