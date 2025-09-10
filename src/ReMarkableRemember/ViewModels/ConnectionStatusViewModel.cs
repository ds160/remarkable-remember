using System;
using ReMarkableRemember.Enumerations;
using ReMarkableRemember.Services.TabletService.Models;

namespace ReMarkableRemember.ViewModels;

public sealed class ConnectionStatusViewModel
{
    private readonly Boolean hasBasicConnection;
    private readonly TabletInformation? information;

    public ConnectionStatusViewModel() : this(TabletConnectionStatus.Default)
    {
    }

    public ConnectionStatusViewModel(TabletConnectionStatus connectionStatus)
    {
        this.hasBasicConnection = connectionStatus.Error is null or (not TabletError.NotSupported and not TabletError.Unknown and not TabletError.SshNotConfigured and not TabletError.SshNotConnected);
        this.information = connectionStatus.Information;

        this.IsConnected = connectionStatus.Error is null;
        this.Tablet = (this.information != null) ? $"{this.information.Type.GetDisplayText()} ({this.information.SoftwareVersion})" : null;
        this.Text = connectionStatus.Error switch
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

    public Boolean IsConnected { get; }

    public String? Tablet { get; }

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
                return this.hasBasicConnection;

            case Jobs.InstallLamyEraser:
                return this.hasBasicConnection && this.information?.LamyEraserSupport == true;

            case Jobs.InstallWebInterfaceOnBoot:
                return this.hasBasicConnection && this.information?.WebInterfaceOnBootSupport == true;

            case Jobs.Sync:
            case Jobs.Download:
            case Jobs.Upload:
                return this.IsConnected;

            default:
                throw new NotImplementedException();
        }
    }
}
