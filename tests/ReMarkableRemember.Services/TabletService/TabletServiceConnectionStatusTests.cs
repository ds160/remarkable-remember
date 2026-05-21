using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ReMarkableRemember.Services.TabletService.Exceptions;
using ReMarkableRemember.Services.TabletService.Models;
using ReMarkableRemember.Services.TabletService.Models.Enumerations;
using ReMarkableRemember.Services.TabletService.Tests.Fakes;

namespace ReMarkableRemember.Services.TabletService.Tests;

[TestFixture]
public sealed class TabletServiceConnectionStatusTests
{
    private static void StubInformation(TabletServiceFixture fixture, String versionFileContent = "Linux blah-rm11x", String osReleaseContent = "IMG_VERSION=\"3.2.1.0\"")
    {
        fixture.Ssh.Setup(s => s.FileReadText("/proc/version")).ReturnsAsync(versionFileContent);
        fixture.Ssh.Setup(s => s.FileReadText("/usr/lib/os-release")).ReturnsAsync(osReleaseContent);
    }

    [Test]
    public async Task GetConnectionStatus_BothSshAndUsbSucceed_ReturnsConnectedStatus()
    {
        TabletServiceFixture fixture = new TabletServiceFixture();
        StubInformation(fixture);

        TabletService service = fixture.Build();
        TabletConnectionStatus status = await service.GetConnectionStatus();

        status.Error.Should().BeNull();
        status.Information.Should().NotBeNull();
        status.Information!.Type.Should().Be(TabletType.rM2);
        status.Information.SoftwareVersion.Should().Be(new Version("3.2.1.0"));
    }

    [Test]
    public async Task GetConnectionStatus_SshFails_ReturnsStatusWithSshError()
    {
        TabletServiceFixture fixture = new TabletServiceFixture();
        fixture.Communication.Setup(c => c.Ssh())
            .ThrowsAsync(new TabletException(TabletError.SshNotConnected, "ssh boom"));

        TabletService service = fixture.Build();
        TabletConnectionStatus status = await service.GetConnectionStatus();

        status.Error.Should().Be(TabletError.SshNotConnected);
        status.Information.Should().BeNull();
    }

    [Test]
    public async Task GetConnectionStatus_SshOkButUsbFails_ReturnsStatusWithUsbError()
    {
        TabletServiceFixture fixture = new TabletServiceFixture();
        StubInformation(fixture);
        fixture.Communication.Setup(c => c.Usb())
            .ThrowsAsync(new TabletException(TabletError.UsbNotConnected, "usb boom"));

        TabletService service = fixture.Build();
        TabletConnectionStatus status = await service.GetConnectionStatus();

        status.Error.Should().Be(TabletError.UsbNotConnected);
        status.Information.Should().NotBeNull("information was obtained over SSH before USB was checked");
    }

    [Test]
    public async Task GetConnectionStatus_UnsupportedTablet_ReturnsNotSupportedError()
    {
        TabletServiceFixture fixture = new TabletServiceFixture();
        StubInformation(fixture, versionFileContent: "Linux blah-unknown-hardware");

        TabletService service = fixture.Build();
        TabletConnectionStatus status = await service.GetConnectionStatus();

        status.Error.Should().Be(TabletError.NotSupported);
    }
}
