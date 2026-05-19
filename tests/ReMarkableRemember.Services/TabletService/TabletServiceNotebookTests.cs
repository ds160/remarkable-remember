using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ReMarkableRemember.Common.Notebook;
using ReMarkableRemember.Services.TabletService.Exceptions;
using ReMarkableRemember.Services.TabletService.Files;
using ReMarkableRemember.Services.TabletService.Tests.Fakes;

namespace ReMarkableRemember.Services.TabletService.Tests;

[TestFixture]
public sealed class TabletServiceNotebookTests
{
    private static Byte[] BuildEmptyVersion5()
    {
        using MemoryStream stream = new MemoryStream();
        using BinaryWriter writer = new BinaryWriter(stream, Encoding.Default);
        writer.Write(Encoding.Default.GetBytes("reMarkable .lines file, version=5          "));
        writer.Write(0);
        return stream.ToArray();
    }

    private static void StubInformation(TabletServiceFixture fixture)
    {
        fixture.Ssh.Setup(s => s.FileReadText("/proc/version")).ReturnsAsync("Linux blah-rm11x");
        fixture.Ssh.Setup(s => s.FileReadText("/usr/lib/os-release")).ReturnsAsync("IMG_VERSION=\"3.2.1.0\"");
    }

    [Test]
    public async Task GetNotebook_InvalidFileType_ThrowsTabletException()
    {
        TabletServiceFixture fixture = new TabletServiceFixture();
        fixture.Ssh.Setup(s => s.FileReadText(It.IsAny<String>())).ReturnsAsync("{}");
        fixture.FileSerializer.Setup(f => f.Deserialize<ContentFile>(It.IsAny<String>()))
            .Returns(new ContentFile { FileType = "bogus", FormatVersion = 1 });

        TabletService service = fixture.Build();
        Func<Task> act = () => service.GetNotebook("id-1");

        await act.Should().ThrowAsync<TabletException>();
    }

    [Test]
    public async Task GetNotebook_InvalidFormatVersion_ThrowsTabletException()
    {
        TabletServiceFixture fixture = new TabletServiceFixture();
        fixture.Ssh.Setup(s => s.FileReadText(It.IsAny<String>())).ReturnsAsync("{}");
        fixture.FileSerializer.Setup(f => f.Deserialize<ContentFile>(It.IsAny<String>()))
            .Returns(new ContentFile { FileType = "notebook", FormatVersion = 99 });

        TabletService service = fixture.Build();
        Func<Task> act = () => service.GetNotebook("id-1");

        await act.Should().ThrowAsync<TabletException>();
    }

    [Test]
    public async Task GetNotebook_LegacyPagesField_ReadsEachPage()
    {
        TabletServiceFixture fixture = new TabletServiceFixture();
        StubInformation(fixture);
        fixture.Ssh.Setup(s => s.FileReadText("/home/root/.local/share/remarkable/xochitl/id-1.content")).ReturnsAsync("{}");
        fixture.FileSerializer.Setup(f => f.Deserialize<ContentFile>(It.IsAny<String>()))
            .Returns(new ContentFile { FileType = "notebook", FormatVersion = 1, Pages = ["p1", "p2"] });
        fixture.Ssh.Setup(s => s.FileReadBytes(It.IsAny<String>())).ReturnsAsync(BuildEmptyVersion5());

        TabletService service = fixture.Build();
        Notebook notebook = await service.GetNotebook("id-1");

        notebook.Pages.Should().HaveCount(2);
        fixture.Ssh.Verify(s => s.FileReadBytes("/home/root/.local/share/remarkable/xochitl/id-1/p1.rm"), Times.Once);
        fixture.Ssh.Verify(s => s.FileReadBytes("/home/root/.local/share/remarkable/xochitl/id-1/p2.rm"), Times.Once);
    }

    [Test]
    public async Task GetNotebook_ModernCPages_SkipsDeletedPages()
    {
        TabletServiceFixture fixture = new TabletServiceFixture();
        StubInformation(fixture);
        fixture.Ssh.Setup(s => s.FileReadText("/home/root/.local/share/remarkable/xochitl/id-1.content")).ReturnsAsync("{}");

        ContentFile.PagesContainer cpages = new ContentFile.PagesContainer
        {
            Pages = new Collection<ContentFile.PagesContainer.Page>
            {
                new ContentFile.PagesContainer.Page { Id = "p1", Deleted = null },
                new ContentFile.PagesContainer.Page { Id = "p2", Deleted = "anything-nonnull" },
                new ContentFile.PagesContainer.Page { Id = "p3", Deleted = null },
            },
        };
        fixture.FileSerializer.Setup(f => f.Deserialize<ContentFile>(It.IsAny<String>()))
            .Returns(new ContentFile { FileType = "notebook", FormatVersion = 2, CPages = cpages });
        fixture.Ssh.Setup(s => s.FileReadBytes(It.IsAny<String>())).ReturnsAsync(BuildEmptyVersion5());

        TabletService service = fixture.Build();
        Notebook notebook = await service.GetNotebook("id-1");

        notebook.Pages.Should().HaveCount(2, "deleted page p2 should be skipped");
        fixture.Ssh.Verify(s => s.FileReadBytes("/home/root/.local/share/remarkable/xochitl/id-1/p2.rm"), Times.Never);
    }
}
