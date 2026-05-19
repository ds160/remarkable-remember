using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ReMarkableRemember.Services.TabletService.Files;
using ReMarkableRemember.Services.TabletService.Files.Interfaces;
using ReMarkableRemember.Services.TabletService.Models;
using ReMarkableRemember.Services.TabletService.Tests.Fakes;

namespace ReMarkableRemember.Services.TabletService.Tests;

[TestFixture]
public sealed class TabletServiceGetItemsTests
{
    private const String RemoteNotebooks = "/home/root/.local/share/remarkable/xochitl/";

    private TabletServiceFixture fixture = null!;

    [SetUp]
    public void SetUp()
    {
        this.fixture = new TabletServiceFixture();
    }

    private void SetupMetadata(String id, String parent, String type, String visibleName, Boolean? deleted = null, String lastModified = "1700000000000")
    {
        String fileName = $"{id}.metadata";
        String fullPath = RemoteNotebooks + fileName;
        this.fixture.Ssh.Setup(s => s.FileReadText(fullPath)).ReturnsAsync($"{{\"id\":\"{id}\"}}");
        this.fixture.FileSerializer.Setup(f => f.Deserialize<MetaDataFile>($"{{\"id\":\"{id}\"}}"))
            .Returns(new MetaDataFile
            {
                Deleted = deleted,
                LastModified = lastModified,
                Parent = parent,
                Type = type,
                VisibleName = visibleName,
            });
    }

    [Test]
    public async Task GetItems_OnlyMetadataFiles_AreProcessed()
    {
        this.SetupMetadata("doc-1", "", "DocumentType", "Doc1");
        IEnumerable<ITabletFileInfo> files = new[]
        {
            TabletFileInfoStub.File(RemoteNotebooks, "doc-1.metadata"),
            TabletFileInfoStub.File(RemoteNotebooks, "doc-1.content"), // not metadata - skipped
            TabletFileInfoStub.File(RemoteNotebooks, "doc-1.pdf"),     // not metadata - skipped
            TabletFileInfoStub.Directory(RemoteNotebooks, "doc-1"),    // not regular file - skipped
        };
        this.fixture.Ssh.Setup(s => s.FileList(RemoteNotebooks)).ReturnsAsync(files);

        TabletItems items = await this.fixture.Build().GetItems();

        items.Items.Should().HaveCount(1);
        items.Items.Single().Id.Should().Be("doc-1");
    }

    [Test]
    public async Task GetItems_DeletedMetadata_IsSkipped()
    {
        this.SetupMetadata("doc-1", "", "DocumentType", "Doc1");
        this.SetupMetadata("doc-2", "", "DocumentType", "Doc2", deleted: true);
        this.fixture.Ssh.Setup(s => s.FileList(RemoteNotebooks)).ReturnsAsync(new[]
        {
            TabletFileInfoStub.File(RemoteNotebooks, "doc-1.metadata"),
            TabletFileInfoStub.File(RemoteNotebooks, "doc-2.metadata"),
        });

        TabletItems items = await this.fixture.Build().GetItems();

        items.Items.Should().ContainSingle().Which.Id.Should().Be("doc-1");
    }

    [Test]
    public async Task GetItems_DeletedFalseOrNull_AreIncluded()
    {
        this.SetupMetadata("a", "", "DocumentType", "A", deleted: null);
        this.SetupMetadata("b", "", "DocumentType", "B", deleted: false);
        this.fixture.Ssh.Setup(s => s.FileList(RemoteNotebooks)).ReturnsAsync(new[]
        {
            TabletFileInfoStub.File(RemoteNotebooks, "a.metadata"),
            TabletFileInfoStub.File(RemoteNotebooks, "b.metadata"),
        });

        TabletItems items = await this.fixture.Build().GetItems();

        items.Items.Select(i => i.Id).Should().BeEquivalentTo(["a", "b"]);
    }

    [Test]
    public async Task GetItems_MetadataDeserializeFailure_RecordedAsNotReadable()
    {
        this.fixture.Ssh.Setup(s => s.FileReadText(It.IsAny<String>())).ReturnsAsync("broken-json");
        this.fixture.FileSerializer.Setup(f => f.Deserialize<MetaDataFile>("broken-json"))
            .Throws(new InvalidOperationException("bad json"));
        this.fixture.Ssh.Setup(s => s.FileList(RemoteNotebooks)).ReturnsAsync(new[]
        {
            TabletFileInfoStub.File(RemoteNotebooks, "broken.metadata"),
        });

        TabletItems items = await this.fixture.Build().GetItems();

        items.Items.Should().BeEmpty();
        items.NotReadable.Should().ContainSingle().Which.Should().Be(RemoteNotebooks + "broken.metadata");
    }

    [Test]
    public async Task GetItems_NestedCollections_BuildsTree()
    {
        this.SetupMetadata("root", "", "CollectionType", "RootFolder");
        this.SetupMetadata("child", "root", "DocumentType", "ChildDoc");
        this.SetupMetadata("grandchild", "child-folder", "DocumentType", "Grandchild");
        this.SetupMetadata("child-folder", "root", "CollectionType", "ChildFolder");
        this.fixture.Ssh.Setup(s => s.FileList(RemoteNotebooks)).ReturnsAsync(new[]
        {
            TabletFileInfoStub.File(RemoteNotebooks, "root.metadata"),
            TabletFileInfoStub.File(RemoteNotebooks, "child.metadata"),
            TabletFileInfoStub.File(RemoteNotebooks, "child-folder.metadata"),
            TabletFileInfoStub.File(RemoteNotebooks, "grandchild.metadata"),
        });

        TabletItems items = await this.fixture.Build().GetItems();

        // Only root-level items appear in Items
        items.Items.Should().ContainSingle().Which.Id.Should().Be("root");
        TabletItem root = items.Items.Single();
        root.Collection.Should().NotBeNull();
        root.Collection!.Select(c => c.Id).Should().BeEquivalentTo(["child", "child-folder"]);

        TabletItem childFolder = root.Collection!.Single(c => c.Id == "child-folder");
        childFolder.Collection.Should().NotBeNull();
        childFolder.Collection!.Single().Id.Should().Be("grandchild");
    }

    [Test]
    public async Task GetItems_TrashedItemsAppearAtRootWithTrashedFlag()
    {
        this.SetupMetadata("doc-1", "trash", "DocumentType", "Trashed");
        this.SetupMetadata("doc-2", "", "DocumentType", "Live");
        this.fixture.Ssh.Setup(s => s.FileList(RemoteNotebooks)).ReturnsAsync(new[]
        {
            TabletFileInfoStub.File(RemoteNotebooks, "doc-1.metadata"),
            TabletFileInfoStub.File(RemoteNotebooks, "doc-2.metadata"),
        });

        TabletItems items = await this.fixture.Build().GetItems();

        items.Items.Should().HaveCount(2, "trashed items are also surfaced at the root");
        items.Items.Single(i => i.Id == "doc-1").Trashed.Should().BeTrue();
        items.Items.Single(i => i.Id == "doc-2").Trashed.Should().BeFalse();
    }

    [Test]
    public async Task GetItems_TrashedFolder_PropagatesTrashedToChildren()
    {
        this.SetupMetadata("folder", "trash", "CollectionType", "Folder");
        this.SetupMetadata("nested", "folder", "DocumentType", "Nested");
        this.fixture.Ssh.Setup(s => s.FileList(RemoteNotebooks)).ReturnsAsync(new[]
        {
            TabletFileInfoStub.File(RemoteNotebooks, "folder.metadata"),
            TabletFileInfoStub.File(RemoteNotebooks, "nested.metadata"),
        });

        TabletItems items = await this.fixture.Build().GetItems();

        TabletItem folder = items.Items.Single(i => i.Id == "folder");
        folder.Trashed.Should().BeTrue();
        folder.Collection.Should().NotBeNull();
        folder.Collection!.Single().Trashed.Should().BeTrue();
    }

    [Test]
    public async Task GetItems_EmptyDirectory_ReturnsEmptyItems()
    {
        this.fixture.Ssh.Setup(s => s.FileList(RemoteNotebooks)).ReturnsAsync(Array.Empty<ITabletFileInfo>());

        TabletItems items = await this.fixture.Build().GetItems();

        items.Items.Should().BeEmpty();
        items.NotReadable.Should().BeEmpty();
    }

    [Test]
    public async Task GetItems_MultipleNotReadable_AllListed()
    {
        this.fixture.Ssh.Setup(s => s.FileReadText(It.IsAny<String>())).ReturnsAsync("broken");
        this.fixture.FileSerializer.Setup(f => f.Deserialize<MetaDataFile>("broken"))
            .Throws(new InvalidOperationException("bad"));
        this.fixture.Ssh.Setup(s => s.FileList(RemoteNotebooks)).ReturnsAsync(new[]
        {
            TabletFileInfoStub.File(RemoteNotebooks, "x.metadata"),
            TabletFileInfoStub.File(RemoteNotebooks, "y.metadata"),
        });

        TabletItems items = await this.fixture.Build().GetItems();

        items.NotReadable.Should().HaveCount(2);
    }
}
