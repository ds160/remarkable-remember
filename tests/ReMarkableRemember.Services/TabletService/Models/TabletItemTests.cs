using System;
using FluentAssertions;
using NUnit.Framework;
using ReMarkableRemember.Services.TabletService.Models;

namespace ReMarkableRemember.Services.TabletService.Tests.Models;

[TestFixture]
public sealed class TabletItemTests
{
    [Test]
    public void DocumentType_NameWithoutPdfExtension_GetsPdfAppended()
    {
        TabletItem item = new TabletItem("id-1", "1700000000000", "parent-id", "DocumentType", "MyFile");

        item.Name.Should().Be("MyFile.pdf");
    }

    [Test]
    public void DocumentType_NameAlreadyEndingInPdf_NoDoubleExtension()
    {
        TabletItem item = new TabletItem("id-1", "1700000000000", "parent-id", "DocumentType", "MyFile.pdf");

        item.Name.Should().Be("MyFile.pdf");
    }

    [Test]
    public void DocumentType_PdfExtensionCaseInsensitive()
    {
        TabletItem item = new TabletItem("id-1", "1700000000000", "parent-id", "DocumentType", "MyFile.PDF");

        item.Name.Should().Be("MyFile.PDF");
    }

    [Test]
    public void CollectionType_HasCollectionListInitializedEmpty()
    {
        TabletItem item = new TabletItem("id-1", "1700000000000", "parent-id", "CollectionType", "Folder");

        item.Collection.Should().NotBeNull();
        item.Collection.Should().BeEmpty();
        item.Name.Should().Be("Folder");
    }

    [Test]
    public void DocumentType_CollectionIsNull()
    {
        TabletItem item = new TabletItem("id-1", "1700000000000", "parent-id", "DocumentType", "x");

        item.Collection.Should().BeNull();
    }

    [Test]
    public void Parent_EqualsTrash_TrashedIsTrue()
    {
        TabletItem item = new TabletItem("id-1", "1700000000000", "trash", "DocumentType", "x");

        item.Trashed.Should().BeTrue();
    }

    [Test]
    public void Parent_NotTrash_TrashedIsFalse()
    {
        TabletItem item = new TabletItem("id-1", "1700000000000", "other", "DocumentType", "x");

        item.Trashed.Should().BeFalse();
    }

    [Test]
    public void Modified_ParsedFromUnixEpochMilliseconds()
    {
        // 1700000000000 ms = 2023-11-14 22:13:20 UTC
        TabletItem item = new TabletItem("id-1", "1700000000000", "p", "DocumentType", "x");

        item.Modified.Should().Be(new DateTime(2023, 11, 14, 22, 13, 20, DateTimeKind.Utc));
    }

    [Test]
    public void Name_StripsInvalidFileNameChars()
    {
        TabletItem item = new TabletItem("id-1", "1700000000000", "p", "DocumentType", "bad\0name");

        item.Name.Should().NotContain("\0");
    }
}
