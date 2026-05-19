using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using ReMarkableRemember.Services.DataService.Models;
using ReMarkableRemember.Services.DataService.Tests.Fixtures;

namespace ReMarkableRemember.Services.DataService.Tests;

[TestFixture]
public sealed class DataServiceSqliteTemplateTests
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
    public async Task GetTemplates_EmptyDatabase_ReturnsEmpty()
    {
        IEnumerable<TemplateData> templates = await this.service.GetTemplates();

        templates.Should().BeEmpty();
    }

    [Test]
    public async Task SetTemplate_AddsNewTemplate()
    {
        TemplateData template = new TemplateData("Cat", "Name", "icon", [1, 2], [3, 4]);

        await this.service.SetTemplate(template);

        TemplateData[] fetched = (await this.service.GetTemplates()).ToArray();
        fetched.Should().HaveCount(1);
        fetched[0].Category.Should().Be("Cat");
        fetched[0].Name.Should().Be("Name");
        fetched[0].IconCode.Should().Be("icon");
        fetched[0].BytesPng.Should().Equal([1, 2]);
        fetched[0].BytesSvg.Should().Equal([3, 4]);
    }

    [Test]
    public async Task SetTemplate_UpdatesExistingByCategoryAndName()
    {
        await this.service.SetTemplate(new TemplateData("Cat", "Name", "icon-1", [1], [2]));
        await this.service.SetTemplate(new TemplateData("Cat", "Name", "icon-2", [9], [8]));

        TemplateData[] fetched = (await this.service.GetTemplates()).ToArray();
        fetched.Should().HaveCount(1);
        fetched[0].IconCode.Should().Be("icon-2");
        fetched[0].BytesPng.Should().Equal([9]);
        fetched[0].BytesSvg.Should().Equal([8]);
    }

    [Test]
    public async Task SetTemplate_SameNameDifferentCategory_AreSeparateRecords()
    {
        await this.service.SetTemplate(new TemplateData("CatA", "Name", "iA", [1], [1]));
        await this.service.SetTemplate(new TemplateData("CatB", "Name", "iB", [2], [2]));

        TemplateData[] fetched = (await this.service.GetTemplates()).ToArray();

        fetched.Should().HaveCount(2);
        fetched.Select(t => t.Category).Should().BeEquivalentTo(["CatA", "CatB"]);
    }

    [Test]
    public async Task DeleteTemplate_RemovesMatchingTemplate()
    {
        await this.service.SetTemplate(new TemplateData("Cat", "Name", "icon", [1], [1]));

        await this.service.DeleteTemplate("Cat", "Name");

        (await this.service.GetTemplates()).Should().BeEmpty();
    }

    [Test]
    public async Task DeleteTemplate_NonExistent_DoesNotThrow()
    {
        Func<Task> act = () => this.service.DeleteTemplate("Missing", "Missing");

        await act.Should().NotThrowAsync();
    }
}
