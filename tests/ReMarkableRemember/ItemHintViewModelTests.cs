using System;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ReMarkableRemember.Services.TabletService.Models;
using ReMarkableRemember.Tests.Fakes;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Tests;

[TestFixture]
public sealed class ItemHintBackupTests
{
    private ServicesFixture fixture = null!;
    private String tempBackupDir = String.Empty;

    [SetUp]
    public void SetUp()
    {
        this.fixture = new ServicesFixture();
        this.tempBackupDir = Path.Combine(Path.GetTempPath(), "rmr-hint-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(this.tempBackupDir);
        this.fixture.TabletConfiguration.SetupGet(c => c.Backup).Returns(this.tempBackupDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(this.tempBackupDir)) { Directory.Delete(this.tempBackupDir, true); }
    }

    [Test]
    public void Backup_WhenBackupConfigPathDoesNotExist_ReturnsNone()
    {
        this.fixture.TabletConfiguration.SetupGet(c => c.Backup).Returns("/no/such/path");
        TabletItem item = ItemViewModelBuilder.MakeDocument();
        ItemViewModel vm = ItemViewModelBuilder.Create(item, this.fixture.Services.Object,
            dataItem: ServicesFixture.CreateItemData(item.Id, null, null, null, null));

        vm.BackupHint.Hint.Should().Be(ItemViewModel.ItemHintViewModel.Hints.None);
    }

    [Test]
    public void Backup_WhenDataItemIsNull_ReturnsNone()
    {
        TabletItem item = ItemViewModelBuilder.MakeDocument();
        ItemViewModel vm = ItemViewModelBuilder.Create(item, this.fixture.Services.Object, dataItem: null);

        vm.BackupHint.Hint.Should().Be(ItemViewModel.ItemHintViewModel.Hints.None);
    }

    [Test]
    public void Backup_WhenBackupDateIsNull_ReturnsNew()
    {
        TabletItem item = ItemViewModelBuilder.MakeDocument();
        ItemViewModel vm = ItemViewModelBuilder.Create(item, this.fixture.Services.Object,
            dataItem: ServicesFixture.CreateItemData(item.Id, backupDate: null, null, null, null));

        vm.BackupHint.Hint.Should().Be(ItemViewModel.ItemHintViewModel.Hints.New);
    }

    [Test]
    public void Backup_WhenBackupDateBeforeModified_ReturnsModified()
    {
        DateTime modified = new DateTime(2026, 5, 18, 12, 0, 0, DateTimeKind.Utc);
        TabletItem item = ItemViewModelBuilder.MakeDocument(modified: modified);
        DateTime olderBackup = modified.AddDays(-1);
        ItemViewModel vm = ItemViewModelBuilder.Create(item, this.fixture.Services.Object,
            dataItem: ServicesFixture.CreateItemData(item.Id, olderBackup, null, null, null));

        vm.BackupHint.Hint.Should().Be(ItemViewModel.ItemHintViewModel.Hints.Modified);
    }

    [Test]
    public void Backup_WhenBackupDateAtOrAfterModified_ReturnsNone()
    {
        DateTime modified = new DateTime(2026, 5, 18, 12, 0, 0, DateTimeKind.Utc);
        TabletItem item = ItemViewModelBuilder.MakeDocument(modified: modified);
        ItemViewModel vm = ItemViewModelBuilder.Create(item, this.fixture.Services.Object,
            dataItem: ServicesFixture.CreateItemData(item.Id, modified, null, null, null));

        vm.BackupHint.Hint.Should().Be(ItemViewModel.ItemHintViewModel.Hints.None);
    }

    [Test]
    public void Image_NoneHintWithNoDate_ReturnsNull()
    {
        TabletItem item = ItemViewModelBuilder.MakeDocument();
        ItemViewModel vm = ItemViewModelBuilder.Create(item, this.fixture.Services.Object, dataItem: null);

        vm.BackupHint.Image.Should().BeNull();
    }

    [Test]
    public void Image_NewHint_ResolvesYellowDot()
    {
        TabletItem item = ItemViewModelBuilder.MakeDocument();
        ItemViewModel vm = ItemViewModelBuilder.Create(item, this.fixture.Services.Object,
            dataItem: ServicesFixture.CreateItemData(item.Id, null, null, null, null));

        _ = vm.BackupHint.Image;

        this.fixture.ImageLoader.Verify(l => l.Svg("Dots/Yellow.svg"), Times.AtLeastOnce);
    }

    [Test]
    public void Image_NoneWithDate_ResolvesGreenDot()
    {
        DateTime modified = new DateTime(2026, 5, 18, 12, 0, 0, DateTimeKind.Utc);
        TabletItem item = ItemViewModelBuilder.MakeDocument(modified: modified);
        ItemViewModel vm = ItemViewModelBuilder.Create(item, this.fixture.Services.Object,
            dataItem: ServicesFixture.CreateItemData(item.Id, modified, null, null, null));

        _ = vm.BackupHint.Image;

        this.fixture.ImageLoader.Verify(l => l.Svg("Dots/Green.svg"), Times.AtLeastOnce);
    }
}

[TestFixture]
public sealed class ItemHintSyncTests
{
    private ServicesFixture fixture = null!;
    private String tempDir = String.Empty;

    [SetUp]
    public void SetUp()
    {
        this.fixture = new ServicesFixture();
        this.tempDir = Path.Combine(Path.GetTempPath(), "rmr-hint-sync-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(this.tempDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(this.tempDir)) { Directory.Delete(this.tempDir, true); }
    }

    [Test]
    public void Sync_FolderItem_AlwaysNone()
    {
        TabletItem folder = ItemViewModelBuilder.MakeCollection();
        ItemViewModel vm = ItemViewModelBuilder.Create(folder, this.fixture.Services.Object);

        vm.SyncHint.Hint.Should().Be(ItemViewModel.ItemHintViewModel.Hints.None);
    }

    [Test]
    public void Sync_NoSyncPath_ReturnsNone()
    {
        TabletItem doc = ItemViewModelBuilder.MakeDocument();
        ItemViewModel vm = ItemViewModelBuilder.Create(doc, this.fixture.Services.Object, syncPath: null);

        vm.SyncHint.Hint.Should().Be(ItemViewModel.ItemHintViewModel.Hints.None);
    }

    [Test]
    public void Sync_NoDataItem_ReturnsNone()
    {
        String syncPath = Path.Combine(this.tempDir, "file.pdf");
        TabletItem doc = ItemViewModelBuilder.MakeDocument();
        ItemViewModel vm = ItemViewModelBuilder.Create(doc, this.fixture.Services.Object, dataItem: null, syncPath: syncPath);

        vm.SyncHint.Hint.Should().Be(ItemViewModel.ItemHintViewModel.Hints.None);
    }

    [Test]
    public void Sync_DataItemSyncPathNullAndFileExists_ExistsInTarget()
    {
        String syncPath = Path.Combine(this.tempDir, "file.pdf");
        File.WriteAllText(syncPath, "x");
        TabletItem doc = ItemViewModelBuilder.MakeDocument();
        ItemViewModel vm = ItemViewModelBuilder.Create(doc, this.fixture.Services.Object,
            dataItem: ServicesFixture.CreateItemData(doc.Id, null, null, syncPath: null, null),
            syncPath: syncPath);

        vm.SyncHint.Hint.Should().Be(ItemViewModel.ItemHintViewModel.Hints.ExistsInTarget);
    }

    [Test]
    public void Sync_DataItemSyncPathNullAndFileMissing_ReturnsNew()
    {
        String syncPath = Path.Combine(this.tempDir, "missing.pdf");
        TabletItem doc = ItemViewModelBuilder.MakeDocument();
        ItemViewModel vm = ItemViewModelBuilder.Create(doc, this.fixture.Services.Object,
            dataItem: ServicesFixture.CreateItemData(doc.Id, null, null, syncPath: null, null),
            syncPath: syncPath);

        vm.SyncHint.Hint.Should().Be(ItemViewModel.ItemHintViewModel.Hints.New);
    }

    [Test]
    public void Sync_DataItemSyncPathDifferent_ReturnsSyncPathChanged()
    {
        String currentPath = Path.Combine(this.tempDir, "new.pdf");
        String oldPath = Path.Combine(this.tempDir, "old.pdf");
        TabletItem doc = ItemViewModelBuilder.MakeDocument();
        ItemViewModel vm = ItemViewModelBuilder.Create(doc, this.fixture.Services.Object,
            dataItem: ServicesFixture.CreateItemData(doc.Id, null, DateTime.UtcNow, oldPath, null),
            syncPath: currentPath);

        vm.SyncHint.Hint.Should().Be(ItemViewModel.ItemHintViewModel.Hints.SyncPathChanged);
    }

    [Test]
    public void Sync_SyncDataOlderThanModified_ReturnsModified()
    {
        DateTime modified = new DateTime(2026, 5, 18, 12, 0, 0, DateTimeKind.Utc);
        String syncPath = Path.Combine(this.tempDir, "f.pdf");
        File.WriteAllText(syncPath, "x");
        TabletItem doc = ItemViewModelBuilder.MakeDocument(modified: modified);
        ItemViewModel vm = ItemViewModelBuilder.Create(doc, this.fixture.Services.Object,
            dataItem: ServicesFixture.CreateItemData(doc.Id, null, modified.AddDays(-1), syncPath, null),
            syncPath: syncPath);

        vm.SyncHint.Hint.Should().Be(ItemViewModel.ItemHintViewModel.Hints.Modified);
    }

    [Test]
    public void Sync_FileDeletedAfterPriorSync_ReturnsNotFoundInTarget()
    {
        DateTime modified = new DateTime(2026, 5, 18, 12, 0, 0, DateTimeKind.Utc);
        String syncPath = Path.Combine(this.tempDir, "gone.pdf");
        TabletItem doc = ItemViewModelBuilder.MakeDocument(modified: modified);
        ItemViewModel vm = ItemViewModelBuilder.Create(doc, this.fixture.Services.Object,
            dataItem: ServicesFixture.CreateItemData(doc.Id, null, modified, syncPath, null),
            syncPath: syncPath);

        vm.SyncHint.Hint.Should().Be(ItemViewModel.ItemHintViewModel.Hints.NotFoundInTarget);
    }

    [Test]
    public void Sync_UpToDate_FileExistsAndSyncDataAtModified_ReturnsNone()
    {
        DateTime modified = new DateTime(2026, 5, 18, 12, 0, 0, DateTimeKind.Utc);
        String syncPath = Path.Combine(this.tempDir, "ok.pdf");
        File.WriteAllText(syncPath, "x");
        TabletItem doc = ItemViewModelBuilder.MakeDocument(modified: modified);
        ItemViewModel vm = ItemViewModelBuilder.Create(doc, this.fixture.Services.Object,
            dataItem: ServicesFixture.CreateItemData(doc.Id, null, modified, syncPath, null),
            syncPath: syncPath);

        vm.SyncHint.Hint.Should().Be(ItemViewModel.ItemHintViewModel.Hints.None);
    }
}

[TestFixture]
public sealed class ItemHintModifiedTests
{
    private ServicesFixture fixture = null!;

    [SetUp]
    public void SetUp()
    {
        this.fixture = new ServicesFixture();
    }

    [Test]
    public void Modified_LeafItem_CombinesBackupAndSyncHints()
    {
        TabletItem doc = ItemViewModelBuilder.MakeDocument();
        // No DataItem -> both Backup.None and Sync.None -> combined None
        ItemViewModel vm = ItemViewModelBuilder.Create(doc, this.fixture.Services.Object);

        vm.ModifiedHint.Hint.Should().Be(ItemViewModel.ItemHintViewModel.Hints.None);
    }

    [Test]
    public void Modified_LeafItem_BackupNew_PropagatesToCombined()
    {
        String tempDir = Path.Combine(Path.GetTempPath(), "rmr-hint-mod-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            this.fixture.TabletConfiguration.SetupGet(c => c.Backup).Returns(tempDir);
            TabletItem doc = ItemViewModelBuilder.MakeDocument();
            ItemViewModel vm = ItemViewModelBuilder.Create(doc, this.fixture.Services.Object,
                dataItem: ServicesFixture.CreateItemData(doc.Id, null, null, null, null));

            vm.ModifiedHint.Hint.HasFlag(ItemViewModel.ItemHintViewModel.Hints.New).Should().BeTrue();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
