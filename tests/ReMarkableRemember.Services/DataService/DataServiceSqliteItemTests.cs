using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using ReMarkableRemember.Services.DataService.Models;
using ReMarkableRemember.Services.DataService.Tests.Fixtures;

namespace ReMarkableRemember.Services.DataService.Tests;

[TestFixture]
public sealed class DataServiceSqliteItemTests
{
    private InMemoryDataServiceFixture fixture = null!;
    private DataServiceSqlite service = null!;

    [SetUp]
    public void SetUp()
    {
        this.fixture = new InMemoryDataServiceFixture();
        this.service = this.fixture.Service;
    }

    [TearDown]
    public void TearDown()
    {
        this.fixture.Dispose();
    }

    [Test]
    public async Task GetItem_UnknownId_ReturnsItemDataWithNulls()
    {
        ItemData item = await this.service.GetItem("missing-id");

        item.Id.Should().Be("missing-id");
        item.BackupDate.Should().BeNull();
        item.SyncData.Should().BeNull();
        item.SyncPath.Should().BeNull();
        item.SyncTargetDirectory.Should().BeNull();
    }

    [Test]
    public async Task SetItemBackup_CreatesBackupAndReturnsUpdatedItemData()
    {
        DateTime modified = new DateTime(2026, 5, 18, 12, 0, 0, DateTimeKind.Utc);

        ItemData item = await this.service.SetItemBackup("id-1", modified);

        item.BackupDate.Should().Be(modified);
        item.Id.Should().Be("id-1");

        ItemData refetched = await this.service.GetItem("id-1");
        refetched.BackupDate.Should().Be(modified);
    }

    [Test]
    public async Task SetItemBackup_UpdatesExistingBackup()
    {
        DateTime first = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime second = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        await this.service.SetItemBackup("id-1", first);

        ItemData item = await this.service.SetItemBackup("id-1", second);

        item.BackupDate.Should().Be(second);
    }

    [Test]
    public async Task SetItemSync_CreatesSyncRecord()
    {
        DateTime modified = new DateTime(2026, 5, 18, 0, 0, 0, DateTimeKind.Utc);

        ItemData item = await this.service.SetItemSync("id-1", modified, "/path/to/file.pdf");

        item.SyncData.Should().Be(modified);
        item.SyncPath.Should().Be("/path/to/file.pdf");

        ItemData refetched = await this.service.GetItem("id-1");
        refetched.SyncData.Should().Be(modified);
        refetched.SyncPath.Should().Be("/path/to/file.pdf");
    }

    [Test]
    public async Task SetItemSync_UpdatesExistingSync()
    {
        DateTime first = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime second = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        await this.service.SetItemSync("id-1", first, "/old/path");

        ItemData item = await this.service.SetItemSync("id-1", second, "/new/path");

        item.SyncData.Should().Be(second);
        item.SyncPath.Should().Be("/new/path");
    }

    [Test]
    public async Task SetItemSyncTargetDirectory_NonNull_CreatesSyncConfiguration()
    {
        ItemData item = await this.service.SetItemSyncTargetDirectory("id-1", "/target/dir");

        item.SyncTargetDirectory.Should().Be("/target/dir");

        ItemData refetched = await this.service.GetItem("id-1");
        refetched.SyncTargetDirectory.Should().Be("/target/dir");
    }

    [Test]
    public async Task SetItemSyncTargetDirectory_UpdatesExisting()
    {
        await this.service.SetItemSyncTargetDirectory("id-1", "/initial");

        ItemData item = await this.service.SetItemSyncTargetDirectory("id-1", "/updated");

        item.SyncTargetDirectory.Should().Be("/updated");
    }

    [Test]
    public async Task SetItemSyncTargetDirectory_Null_RemovesExistingConfiguration()
    {
        await this.service.SetItemSyncTargetDirectory("id-1", "/initial");

        ItemData item = await this.service.SetItemSyncTargetDirectory("id-1", null);

        item.SyncTargetDirectory.Should().BeNull();

        ItemData refetched = await this.service.GetItem("id-1");
        refetched.SyncTargetDirectory.Should().BeNull();
    }

    [Test]
    public async Task GetItem_AfterAllThreeSetsApplied_AggregatesData()
    {
        DateTime backup = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime sync = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        await this.service.SetItemBackup("id-1", backup);
        await this.service.SetItemSync("id-1", sync, "/path/file.pdf");
        await this.service.SetItemSyncTargetDirectory("id-1", "/target");

        ItemData item = await this.service.GetItem("id-1");

        item.BackupDate.Should().Be(backup);
        item.SyncData.Should().Be(sync);
        item.SyncPath.Should().Be("/path/file.pdf");
        item.SyncTargetDirectory.Should().Be("/target");
    }
}
