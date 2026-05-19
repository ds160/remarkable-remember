using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ReMarkableRemember.Services.TabletService.Files.Interfaces;
using ReMarkableRemember.Services.TabletService.Tests.Fakes;

namespace ReMarkableRemember.Services.TabletService.Tests;

[TestFixture]
public sealed class TabletServiceBackupTests
{
    private const String RemoteNotebooks = "/home/root/.local/share/remarkable/xochitl/";

    private TabletServiceFixture fixture = null!;
    private String localBackupDir = String.Empty;

    [SetUp]
    public void SetUp()
    {
        this.fixture = new TabletServiceFixture();
        this.localBackupDir = Path.Combine(Path.GetTempPath(), "rmr-backup-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(this.localBackupDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(this.localBackupDir)) { Directory.Delete(this.localBackupDir, true); }
    }

    private TabletService BuildAndConfigure()
    {
        TabletService service = this.fixture.Build();
        service.Configuration.Backup = this.localBackupDir;
        return service;
    }

    [Test]
    public async Task Backup_WhenLocalTargetDirectoryDoesNotExist_NoSshActivity()
    {
        TabletService service = this.fixture.Build();
        service.Configuration.Backup = "/no/such/dir";
        this.fixture.Ssh.Setup(s => s.FileList(It.IsAny<String>())).ReturnsAsync(Array.Empty<ITabletFileInfo>());

        await service.Backup("doc-1");

        this.fixture.Ssh.Verify(s => s.FileList(It.IsAny<String>()), Times.Never);
        this.fixture.Ssh.Verify(s => s.FileDownload(It.IsAny<String>(), It.IsAny<String>()), Times.Never);
    }

    [Test]
    public async Task Backup_RemovesExistingDirectoriesMatchingIdPrefix()
    {
        String preExisting = Path.Combine(this.localBackupDir, "doc-1.thumbnails");
        Directory.CreateDirectory(preExisting);

        TabletService service = this.BuildAndConfigure();
        this.fixture.Ssh.Setup(s => s.FileList(RemoteNotebooks)).ReturnsAsync(Array.Empty<ITabletFileInfo>());

        await service.Backup("doc-1");

        Directory.Exists(preExisting).Should().BeFalse();
    }

    [Test]
    public async Task Backup_RemovesExistingFilesStartingWithIdPrefix()
    {
        String preExistingFile = Path.Combine(this.localBackupDir, "doc-1.content");
        await File.WriteAllTextAsync(preExistingFile, "old");

        TabletService service = this.BuildAndConfigure();
        this.fixture.Ssh.Setup(s => s.FileList(RemoteNotebooks)).ReturnsAsync(Array.Empty<ITabletFileInfo>());

        await service.Backup("doc-1");

        File.Exists(preExistingFile).Should().BeFalse();
    }

    [Test]
    public async Task Backup_DoesNotRemoveFilesForOtherIds()
    {
        String otherFile = Path.Combine(this.localBackupDir, "doc-2.content");
        await File.WriteAllTextAsync(otherFile, "keep");

        TabletService service = this.BuildAndConfigure();
        this.fixture.Ssh.Setup(s => s.FileList(RemoteNotebooks)).ReturnsAsync(Array.Empty<ITabletFileInfo>());

        await service.Backup("doc-1");

        File.Exists(otherFile).Should().BeTrue("other documents' backups must not be touched");
    }

    [Test]
    public async Task Backup_DownloadsRegularFilesMatchingIdPrefix()
    {
        TabletService service = this.BuildAndConfigure();
        IEnumerable<ITabletFileInfo> remoteFiles = new[]
        {
            TabletFileInfoStub.File(RemoteNotebooks, "doc-1.content"),
            TabletFileInfoStub.File(RemoteNotebooks, "doc-1.metadata"),
            TabletFileInfoStub.File(RemoteNotebooks, "doc-2.content"), // different id - skipped
        };
        this.fixture.Ssh.Setup(s => s.FileList(RemoteNotebooks)).ReturnsAsync(remoteFiles);

        await service.Backup("doc-1");

        this.fixture.Ssh.Verify(s => s.FileDownload(RemoteNotebooks + "doc-1.content", Path.Combine(this.localBackupDir, "doc-1.content")), Times.Once);
        this.fixture.Ssh.Verify(s => s.FileDownload(RemoteNotebooks + "doc-1.metadata", Path.Combine(this.localBackupDir, "doc-1.metadata")), Times.Once);
        this.fixture.Ssh.Verify(s => s.FileDownload(RemoteNotebooks + "doc-2.content", It.IsAny<String>()), Times.Never);
    }

    [Test]
    public async Task Backup_RecursesIntoMatchingDirectoriesAndSkipsDotEntries()
    {
        TabletService service = this.BuildAndConfigure();
        String docDir = RemoteNotebooks + "doc-1";

        IEnumerable<ITabletFileInfo> topLevel = new[]
        {
            TabletFileInfoStub.Directory(RemoteNotebooks, "doc-1"),
        };
        IEnumerable<ITabletFileInfo> insideDocDir = new[]
        {
            TabletFileInfoStub.File(docDir, "page-1.rm"),
            TabletFileInfoStub.Directory(docDir, "."),
            TabletFileInfoStub.Directory(docDir, ".."),
        };

        this.fixture.Ssh.Setup(s => s.FileList(RemoteNotebooks)).ReturnsAsync(topLevel);
        this.fixture.Ssh.Setup(s => s.FileList(docDir)).ReturnsAsync(insideDocDir);

        await service.Backup("doc-1");

        // Recursion happens into doc-1 dir (FileList called)
        this.fixture.Ssh.Verify(s => s.FileList(docDir), Times.Once);
        // The page file inside is downloaded
        this.fixture.Ssh.Verify(s => s.FileDownload(docDir + "/page-1.rm", Path.Combine(this.localBackupDir, "doc-1", "page-1.rm")), Times.Once);
        // The . and .. entries are not recursed
        this.fixture.Ssh.Verify(s => s.FileList(docDir + "/."), Times.Never);
        this.fixture.Ssh.Verify(s => s.FileList(docDir + "/.."), Times.Never);
    }

    [Test]
    public async Task Backup_RecursesMultipleLevelsDeep()
    {
        TabletService service = this.BuildAndConfigure();
        String docDir = RemoteNotebooks + "doc-1";
        String nestedDir = docDir + "/nested";

        this.fixture.Ssh.Setup(s => s.FileList(RemoteNotebooks)).ReturnsAsync(new[]
        {
            TabletFileInfoStub.Directory(RemoteNotebooks, "doc-1"),
        });
        this.fixture.Ssh.Setup(s => s.FileList(docDir)).ReturnsAsync(new[]
        {
            TabletFileInfoStub.Directory(docDir, "nested"),
        });
        this.fixture.Ssh.Setup(s => s.FileList(nestedDir)).ReturnsAsync(new[]
        {
            TabletFileInfoStub.File(nestedDir, "deep.rm"),
        });

        await service.Backup("doc-1");

        this.fixture.Ssh.Verify(s => s.FileDownload(nestedDir + "/deep.rm", Path.Combine(this.localBackupDir, "doc-1", "nested", "deep.rm")), Times.Once);
    }
}
