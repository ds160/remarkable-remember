using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ReMarkableRemember.Services.TabletService.Exceptions;
using ReMarkableRemember.Services.TabletService.Models.Enumerations;
using ReMarkableRemember.Services.TabletService.Tests.Fakes;

namespace ReMarkableRemember.Services.TabletService.Tests;

[TestFixture]
public sealed class TabletServiceMiscTests
{
    [Test]
    public void Constructor_PassesConfigurationToCommunication()
    {
        TabletServiceFixture fixture = new TabletServiceFixture();

        TabletService service = fixture.Build();

        fixture.Communication.Verify(c => c.Configuration(service.Configuration), Times.Once);
    }

    [Test]
    public async Task Restart_CallsSystemctlRestartXochitl()
    {
        TabletServiceFixture fixture = new TabletServiceFixture();

        TabletService service = fixture.Build();
        await service.Restart();

        fixture.Ssh.Verify(s => s.Execute("systemctl restart xochitl", true), Times.Once);
    }

    [Test]
    public async Task InstallLamyEraser_UnsupportedHardware_ThrowsNotSupported()
    {
        TabletServiceFixture fixture = new TabletServiceFixture();
        // rMPaperPro does not support Lamy Eraser
        fixture.Ssh.Setup(s => s.FileReadText("/proc/version")).ReturnsAsync("Linux imx8mm-ferrari");
        fixture.Ssh.Setup(s => s.FileReadText("/usr/lib/os-release")).ReturnsAsync("IMG_VERSION=\"3.2.1.0\"");

        TabletService service = fixture.Build();
        Func<Task> act = () => service.InstallLamyEraser(false, false, false);

        TabletException exception = (await act.Should().ThrowAsync<TabletException>()).Which;
        exception.Error.Should().Be(TabletError.NotSupported);
    }
}
