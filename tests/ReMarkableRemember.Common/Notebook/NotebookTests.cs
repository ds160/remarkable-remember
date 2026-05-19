using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ReMarkableRemember.Common.Notebook.Enumerations;
using ReMarkableRemember.Common.Notebook.Exceptions;
using ReMarkableRemember.Common.Notebook.Tests.Helpers;

namespace ReMarkableRemember.Common.Notebook.Tests;

[TestFixture]
public sealed class NotebookTests
{
    [Test]
    public void Parse_EmptyPageList_ReturnsNotebookWithNoPages()
    {
        Notebook notebook = Notebook.Parse(Array.Empty<Byte[]>(), 226);

        notebook.Pages.Should().BeEmpty();
    }

    [Test]
    public void Parse_MultiplePages_AssignsAscendingIndexes()
    {
        Byte[] page1 = new Version5BufferBuilder().Build();
        Byte[] page2 = new Version5BufferBuilder().AddLayer(new LineSpec()).Build();
        Byte[] page3 = new Version5BufferBuilder().Build();

        Notebook notebook = Notebook.Parse(new[] { page1, page2, page3 }, 226);

        notebook.Pages.Select(p => p.Index).Should().Equal(0, 1, 2);
    }

    [Test]
    public void Parse_PassesResolutionToEachPage()
    {
        Byte[] page = new Version5BufferBuilder().Build();

        Notebook notebook = Notebook.Parse(new[] { page }, 229);

        notebook.Pages.Single().Resolution.Should().Be(229);
    }

    [Test]
    public void Parse_UnknownHeader_ThrowsNotebookException()
    {
        Byte[] buffer = Version5BufferBuilder.WithUnknownHeader();

        Action act = () => Notebook.Parse(new[] { buffer }, 226);

        act.Should().Throw<NotebookException>();
    }

    [Test]
    public void Parse_Version5SingleLineSinglePoint_ReturnsExpectedPenAndColor()
    {
        Byte[] buffer = new Version5BufferBuilder()
            .AddLayer(new LineSpec
            {
                Type = PenType.Marker1,
                Color = PenColor.Blue,
                Points = new List<(Single, Single)> { (1.5f, 2.5f) },
            })
            .Build();

        Notebook notebook = Notebook.Parse(new[] { buffer }, 226);

        Page page = notebook.Pages.Single();
        Line line = page.Lines.Single();
        line.Type.Should().Be(PenType.Marker1);
        line.Color.Should().Be(PenColor.Blue);
        Point point = line.Points.Single();
        point.X.Should().Be(1.5f);
        point.Y.Should().Be(2.5f);
    }

    [Test]
    public void Parse_Version5MultipleLayers_FlattensLines()
    {
        Byte[] buffer = new Version5BufferBuilder()
            .AddLayer(new LineSpec { Color = PenColor.Black }, new LineSpec { Color = PenColor.Red })
            .AddLayer(new LineSpec { Color = PenColor.Blue })
            .Build();

        Notebook notebook = Notebook.Parse(new[] { buffer }, 226);

        Page page = notebook.Pages.Single();
        page.Lines.Select(l => l.Color).Should().BeEquivalentTo(new[] { PenColor.Black, PenColor.Red, PenColor.Blue });
    }

    [Test]
    public void Parse_Version5EmptyLayers_ProducesNoLines()
    {
        Byte[] buffer = new Version5BufferBuilder().Build();

        Notebook notebook = Notebook.Parse(new[] { buffer }, 226);

        notebook.Pages.Single().Lines.Should().BeEmpty();
    }
}
