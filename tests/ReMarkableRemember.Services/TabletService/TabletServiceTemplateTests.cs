using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ReMarkableRemember.Services.TabletService.Files;
using ReMarkableRemember.Services.TabletService.Models;
using ReMarkableRemember.Services.TabletService.Tests.Fakes;

namespace ReMarkableRemember.Services.TabletService.Tests;

[TestFixture]
public sealed class TabletServiceTemplateTests
{
    private const String TemplatesDir = "/usr/share/remarkable/templates/";
    private const String TemplatesFilePath = TemplatesDir + "templates.json";

    private TabletServiceFixture fixture = null!;

    [SetUp]
    public void SetUp()
    {
        this.fixture = new TabletServiceFixture();
    }

    /// <summary>
    /// Build a <see cref="TabletTemplate"/> via reflection so tests don't have to materialise
    /// an icon code that maps to a real <see cref="TabletTemplateIcon"/>.
    /// </summary>
    private static TabletTemplate MakeTemplate(String name, String category, Byte[]? png = null, Byte[]? svg = null)
    {
        // The (name, category, iconCode, bytesPng, bytesSvg) constructor looks up the icon code
        // in TabletTemplateIcon.Icons to derive Landscape. Pick a known icon.
        TabletTemplateIcon firstIcon = TabletTemplateIcon.Icons.First();
        return new TabletTemplate(name, category, firstIcon.Code, png ?? new Byte[] { 1 }, svg ?? new Byte[] { 2 });
    }

    private void SetupTemplatesFile(TemplatesFile templatesFile)
    {
        this.fixture.Ssh.Setup(s => s.FileReadText(TemplatesFilePath)).ReturnsAsync("{}");
        this.fixture.FileSerializer.Setup(f => f.Deserialize<TemplatesFile>("{}"))
            .Returns(templatesFile);
        this.fixture.FileSerializer.Setup(f => f.Serialize(It.IsAny<TemplatesFile>()))
            .Returns("{serialized}");
    }

    [Test]
    public async Task UploadTemplate_NewTemplate_AppendsToTemplatesFile()
    {
        TemplatesFile templates = new TemplatesFile { Templates = new List<TemplatesFile.Template>() };
        this.SetupTemplatesFile(templates);
        TabletTemplate template = MakeTemplate("MyTemplate", "MyCat", new Byte[] { 10, 20 }, new Byte[] { 30, 40 });

        TabletService service = this.fixture.Build();
        await service.UploadTemplate(template);

        templates.Templates.Should().HaveCount(1);
        templates.Templates[0].Filename.Should().Be(template.FileName);
        templates.Templates[0].Name.Should().Be("MyTemplate");
    }

    [Test]
    public async Task UploadTemplate_ExistingFilename_ReplacesEntryAtSameIndex()
    {
        TemplatesFile templates = new TemplatesFile
        {
            Templates = new List<TemplatesFile.Template>
            {
                new TemplatesFile.Template { Name = "Other", Filename = "Other Other", IconCode = "x" },
                new TemplatesFile.Template { Name = "MyTemplate", Filename = "MyCat MyTemplate", IconCode = "old", Categories = ["MyCat"] },
                new TemplatesFile.Template { Name = "Tail", Filename = "Tail Tail", IconCode = "y" },
            },
        };
        this.SetupTemplatesFile(templates);
        TabletTemplate replacement = MakeTemplate("MyTemplate", "MyCat");

        TabletService service = this.fixture.Build();
        await service.UploadTemplate(replacement);

        templates.Templates.Should().HaveCount(3, "replacement should keep the list size");
        templates.Templates[1].Filename.Should().Be(replacement.FileName);
        templates.Templates[1].IconCode.Should().Be(replacement.IconCode);
        templates.Templates[0].Name.Should().Be("Other");
        templates.Templates[2].Name.Should().Be("Tail");
    }

    [Test]
    public async Task UploadTemplate_WritesPngSvgAndJson()
    {
        TemplatesFile templates = new TemplatesFile { Templates = new List<TemplatesFile.Template>() };
        this.SetupTemplatesFile(templates);
        TabletTemplate template = MakeTemplate("Name", "Cat", new Byte[] { 1 }, new Byte[] { 2 });

        TabletService service = this.fixture.Build();
        await service.UploadTemplate(template);

        this.fixture.Ssh.Verify(s => s.FileWrite($"{TemplatesDir}{template.FileName}.png", template.BytesPng, false), Times.Once);
        this.fixture.Ssh.Verify(s => s.FileWrite($"{TemplatesDir}{template.FileName}.svg", template.BytesSvg, false), Times.Once);
        this.fixture.Ssh.Verify(s => s.FileWrite(TemplatesFilePath, "{serialized}", true), Times.Once);
    }

    [Test]
    public async Task DeleteTemplate_ExistingTemplate_RemovesFromList()
    {
        TabletTemplate template = MakeTemplate("Name", "Cat");
        TemplatesFile templates = new TemplatesFile
        {
            Templates = new List<TemplatesFile.Template>
            {
                new TemplatesFile.Template { Name = "Other", Filename = "Other Other" },
                new TemplatesFile.Template { Name = template.Name, Filename = template.FileName },
            },
        };
        this.SetupTemplatesFile(templates);

        TabletService service = this.fixture.Build();
        await service.DeleteTemplate(template);

        templates.Templates.Should().HaveCount(1);
        templates.Templates[0].Filename.Should().Be("Other Other");
    }

    [Test]
    public async Task DeleteTemplate_MissingEntry_LeavesListUntouchedButStillCleansFiles()
    {
        TemplatesFile templates = new TemplatesFile { Templates = new List<TemplatesFile.Template>() };
        this.SetupTemplatesFile(templates);
        TabletTemplate template = MakeTemplate("Name", "Cat");

        TabletService service = this.fixture.Build();
        await service.DeleteTemplate(template);

        templates.Templates.Should().BeEmpty();
        // Even when the JSON entry was missing, the PNG/SVG/json writes still happen.
        this.fixture.Ssh.Verify(s => s.FileDelete($"{TemplatesDir}{template.FileName}.png"), Times.Once);
        this.fixture.Ssh.Verify(s => s.FileDelete($"{TemplatesDir}{template.FileName}.svg"), Times.Once);
        this.fixture.Ssh.Verify(s => s.FileWrite(TemplatesFilePath, "{serialized}", true), Times.Once);
    }

    [Test]
    public async Task DeleteTemplate_DeletesPngSvgAndWritesUpdatedJson()
    {
        TabletTemplate template = MakeTemplate("Name", "Cat");
        TemplatesFile templates = new TemplatesFile
        {
            Templates = new List<TemplatesFile.Template>
            {
                new TemplatesFile.Template { Name = template.Name, Filename = template.FileName },
            },
        };
        this.SetupTemplatesFile(templates);

        TabletService service = this.fixture.Build();
        await service.DeleteTemplate(template);

        this.fixture.Ssh.Verify(s => s.FileDelete($"{TemplatesDir}{template.FileName}.png"), Times.Once);
        this.fixture.Ssh.Verify(s => s.FileDelete($"{TemplatesDir}{template.FileName}.svg"), Times.Once);
        this.fixture.Ssh.Verify(s => s.FileWrite(TemplatesFilePath, "{serialized}", true), Times.Once);
    }
}
