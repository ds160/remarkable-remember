using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ReMarkableRemember.Services.TabletService.Tests.Fakes;

namespace ReMarkableRemember.Services.TabletService.Tests;

[TestFixture]
public sealed class TabletServiceUploadFileTests
{
    private TabletServiceFixture fixture = null!;
    private String tempFile = String.Empty;

    [SetUp]
    public void SetUp()
    {
        this.fixture = new TabletServiceFixture();
        this.tempFile = Path.Combine(Path.GetTempPath(), "rmr-upload-" + Guid.NewGuid().ToString("N") + ".pdf");
        File.WriteAllBytes(this.tempFile, [0x25, 0x50, 0x44, 0x46]); // "%PDF" magic
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(this.tempFile)) { File.Delete(this.tempFile); }
    }

    [Test]
    public async Task UploadFile_DelegatesToUsbCommunicationWithFileInfo()
    {
        TabletService service = this.fixture.Build();

        await service.UploadFile(this.tempFile, parentId: "parent-123");

        this.fixture.Usb.Verify(u => u.Upload(It.Is<FileInfo>(fi => fi.FullName == this.tempFile), "parent-123"), Times.Once);
    }

    [Test]
    public async Task UploadFile_NullParentId_IsForwarded()
    {
        TabletService service = this.fixture.Build();

        await service.UploadFile(this.tempFile, parentId: null);

        this.fixture.Usb.Verify(u => u.Upload(It.IsAny<FileInfo>(), null), Times.Once);
    }

    [Test]
    public async Task UploadFile_PropagatesUsbExceptions()
    {
        this.fixture.Usb
            .Setup(u => u.Upload(It.IsAny<FileInfo>(), It.IsAny<String>()))
            .ThrowsAsync(new InvalidOperationException("usb error"));
        TabletService service = this.fixture.Build();

        Func<Task> act = () => service.UploadFile(this.tempFile, "p");

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("usb error");
    }
}
