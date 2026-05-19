using System;
using FluentAssertions;
using NUnit.Framework;
using ReMarkableRemember.Services.TabletService.Files;

namespace ReMarkableRemember.Services.TabletService.Tests.Files;

[TestFixture]
public sealed class TabletFileSerializerTests
{
    [Test]
    public void Roundtrip_MetaDataFile_PreservesValues()
    {
        TabletFileSerializer serializer = new TabletFileSerializer();
        MetaDataFile source = new MetaDataFile
        {
            Deleted = null,
            LastModified = "1700000000000",
            Parent = "parent-id",
            Type = "DocumentType",
            VisibleName = "My File",
        };

        String json = serializer.Serialize(source);
        MetaDataFile roundtripped = serializer.Deserialize<MetaDataFile>(json);

        roundtripped.LastModified.Should().Be(source.LastModified);
        roundtripped.Parent.Should().Be(source.Parent);
        roundtripped.Type.Should().Be(source.Type);
        roundtripped.VisibleName.Should().Be(source.VisibleName);
    }

    [Test]
    public void Deserialize_MetaDataFromCamelCase_WorksCorrectly()
    {
        TabletFileSerializer serializer = new TabletFileSerializer();
        // Tablet stores everything in camelCase
        String json = /*lang=json,strict*/ """
            {
              "deleted": null,
              "lastModified": "1700000000000",
              "parent": "trash",
              "type": "CollectionType",
              "visibleName": "Folder"
            }
            """;

        MetaDataFile data = serializer.Deserialize<MetaDataFile>(json);

        data.Parent.Should().Be("trash");
        data.Type.Should().Be("CollectionType");
        data.VisibleName.Should().Be("Folder");
        data.LastModified.Should().Be("1700000000000");
    }

    [Test]
    public void Serialize_OmitsNullProperties()
    {
        TabletFileSerializer serializer = new TabletFileSerializer();
        MetaDataFile data = new MetaDataFile { LastModified = "0", Parent = "p", Type = "DocumentType", VisibleName = "x" };

        String json = serializer.Serialize(data);

        json.Should().NotContain("deleted", "Null properties are omitted by JsonIgnoreCondition.WhenWritingNull");
    }

    [Test]
    public void Deserialize_ContentFileLegacyPages_PopulatesPages()
    {
        TabletFileSerializer serializer = new TabletFileSerializer();
        String json = /*lang=json,strict*/ """
            {
              "fileType": "notebook",
              "formatVersion": 1,
              "pages": ["page-1", "page-2"]
            }
            """;

        ContentFile data = serializer.Deserialize<ContentFile>(json);

        data.FileType.Should().Be("notebook");
        data.FormatVersion.Should().Be(1);
        data.Pages.Should().Equal("page-1", "page-2");
        data.CPages.Should().BeNull();
    }

    [Test]
    public void Deserialize_ContentFileModernCPages_PopulatesCPages()
    {
        TabletFileSerializer serializer = new TabletFileSerializer();
        String json = /*lang=json,strict*/ """
            {
              "fileType": "notebook",
              "formatVersion": 2,
              "cPages": {
                "pages": [
                  { "id": "p1", "deleted": null },
                  { "id": "p2", "deleted": { "value": 1 } }
                ]
              }
            }
            """;

        ContentFile data = serializer.Deserialize<ContentFile>(json);

        data.CPages.Should().NotBeNull();
        data.CPages!.Value.Pages.Should().HaveCount(2);
    }
}
