using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ReMarkableRemember.Common.Notebook.Enumerations;
using ReMarkableRemember.Common.Notebook.Tests.Helpers;

namespace ReMarkableRemember.Common.Notebook.Tests;

[TestFixture]
public sealed class LineAndPointTests
{
    [Test]
    public void Line_ExposesColorTypeAndPoints()
    {
        // Lines/Points have internal constructors so we instantiate them via parsing.
        Byte[] buffer = new Version5BufferBuilder()
            .AddLayer(new LineSpec
            {
                Type = PenType.Highlighter1,
                Color = PenColor.Yellow1,
                Points = new System.Collections.Generic.List<(Single, Single)>
                {
                    (0f, 0f),
                    (10f, 20f),
                },
            })
            .Build();

        Notebook notebook = Notebook.Parse(new[] { buffer }, 226);

        Line line = notebook.Pages.Single().Lines.Single();
        line.Type.Should().Be(PenType.Highlighter1);
        line.Color.Should().Be(PenColor.Yellow1);
        line.Points.Should().HaveCount(2);
        line.Points.First().X.Should().Be(0f);
        line.Points.Last().X.Should().Be(10f);
        line.Points.Last().Y.Should().Be(20f);
    }
}
