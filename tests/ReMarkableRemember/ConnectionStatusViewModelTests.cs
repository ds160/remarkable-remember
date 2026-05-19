using System;
using FluentAssertions;
using NUnit.Framework;
using ReMarkableRemember.Services.TabletService.Models;
using ReMarkableRemember.Services.TabletService.Models.Enumerations;
using ReMarkableRemember.ViewModels;
using ReMarkableRemember.ViewModels.Enumerations;

namespace ReMarkableRemember.Tests;

[TestFixture]
public sealed class ConnectionStatusViewModelTests
{
    private static TabletConnectionStatus CreateStatus(TabletInformation? info, TabletError? error)
    {
        return new TabletConnectionStatus(info, error);
    }

    [Test]
    public void Defaults_IsDisconnectedWithUnknownError()
    {
        ConnectionStatusViewModel vm = new ConnectionStatusViewModel();

        vm.IsConnected.Should().BeFalse();
        vm.Tablet.Should().BeNull();
    }

    [Test]
    public void Connected_HasTabletDisplayText()
    {
        TabletInformation info = new TabletInformation(TabletType.rM2, new Version(3, 2, 1));
        TabletConnectionStatus status = CreateStatus(info, null);

        ConnectionStatusViewModel vm = new ConnectionStatusViewModel(status);

        vm.IsConnected.Should().BeTrue();
        vm.Tablet.Should().Be("reMarkable 2 (3.2.1)");
    }

    [Test]
    public void CheckJob_AlwaysAllowedJobs_ReturnTrueRegardlessOfState()
    {
        ConnectionStatusViewModel disconnected = new ConnectionStatusViewModel();

        disconnected.CheckJob(Jobs.None).Should().BeTrue();
        disconnected.CheckJob(Jobs.Settings).Should().BeTrue();
        disconnected.CheckJob(Jobs.SetSyncTargetDirectory).Should().BeTrue();
    }

    [Test]
    public void CheckJob_FullConnectionRequired_ReturnFalseWhenAnyError()
    {
        TabletInformation info = new TabletInformation(TabletType.rM2, new Version(3, 0));
        TabletConnectionStatus partialStatus = CreateStatus(info, TabletError.UsbNotConnected);
        ConnectionStatusViewModel vm = new ConnectionStatusViewModel(partialStatus);

        vm.IsConnected.Should().BeFalse();
        vm.CheckJob(Jobs.Sync).Should().BeFalse();
        vm.CheckJob(Jobs.Download).Should().BeFalse();
        vm.CheckJob(Jobs.Upload).Should().BeFalse();
    }

    [Test]
    public void CheckJob_BasicConnectionRequired_AllowedWhenOnlyUsbDown()
    {
        TabletInformation info = new TabletInformation(TabletType.rM2, new Version(3, 0));
        TabletConnectionStatus status = CreateStatus(info, TabletError.UsbNotConnected);
        ConnectionStatusViewModel vm = new ConnectionStatusViewModel(status);

        vm.CheckJob(Jobs.GetItems).Should().BeTrue();
        vm.CheckJob(Jobs.Backup).Should().BeTrue();
        vm.CheckJob(Jobs.HandwritingRecognition).Should().BeTrue();
        vm.CheckJob(Jobs.UploadTemplate).Should().BeTrue();
        vm.CheckJob(Jobs.ManageTemplates).Should().BeTrue();
    }

    [Test]
    public void CheckJob_BasicConnectionRequired_DeniedWhenSshDown()
    {
        TabletConnectionStatus status = CreateStatus(null, TabletError.SshNotConnected);
        ConnectionStatusViewModel vm = new ConnectionStatusViewModel(status);

        vm.CheckJob(Jobs.GetItems).Should().BeFalse();
        vm.CheckJob(Jobs.Backup).Should().BeFalse();
    }

    [Test]
    public void CheckJob_InstallLamyEraser_RequiresHardwareSupport()
    {
        TabletInformation rmPaperPro = new TabletInformation(TabletType.rMPaperPro, new Version(3, 0));
        TabletConnectionStatus statusUnsupported = CreateStatus(rmPaperPro, null);
        ConnectionStatusViewModel vmUnsupported = new ConnectionStatusViewModel(statusUnsupported);

        TabletInformation rm2 = new TabletInformation(TabletType.rM2, new Version(3, 0));
        TabletConnectionStatus statusSupported = CreateStatus(rm2, null);
        ConnectionStatusViewModel vmSupported = new ConnectionStatusViewModel(statusSupported);

        vmUnsupported.CheckJob(Jobs.InstallLamyEraser).Should().BeFalse();
        vmSupported.CheckJob(Jobs.InstallLamyEraser).Should().BeTrue();
    }

    [Test]
    public void CheckJob_UndefinedJob_Throws()
    {
        ConnectionStatusViewModel vm = new ConnectionStatusViewModel();

        Action act = () => vm.CheckJob((Jobs)0x10000); // not a defined job

        act.Should().Throw<NotImplementedException>();
    }
}
