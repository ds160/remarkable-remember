using System;
using System.Linq;
using Avalonia.Media;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ReMarkableRemember.Services.TabletService.Models;
using ReMarkableRemember.Tests.Fakes;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Tests;

[TestFixture]
public sealed class TemplateIconViewModelTests
{
    private ServicesFixture fixture = null!;

    [SetUp]
    public void SetUp()
    {
        this.fixture = new ServicesFixture();
    }

    [Test]
    public void GetIcons_ReturnsAllRegisteredIconsAsViewModels()
    {
        TemplateIconViewModel[] icons = TemplateIconViewModel.GetIcons(this.fixture.Services.Object).ToArray();

        icons.Length.Should().Be(TabletTemplateIcon.Icons.ToArray().Length);
    }

    [Test]
    public void GetIcons_EachIcon_HasCodeAndImage()
    {
        TemplateIconViewModel[] icons = TemplateIconViewModel.GetIcons(this.fixture.Services.Object).ToArray();

        icons.Should().OnlyContain(i => !String.IsNullOrEmpty(i.Code));
        icons.Should().OnlyContain(i => i.Image != null);
    }

    [Test]
    public void GetIcons_LoadsBitmapForEachIconViaImageLoader()
    {
        TemplateIconViewModel[] icons = TemplateIconViewModel.GetIcons(this.fixture.Services.Object).ToArray();

        foreach (TabletTemplateIcon icon in TabletTemplateIcon.Icons)
        {
            this.fixture.ImageLoader.Verify(l => l.Bitmap($"Templates/{icon.ImageName}.png"), Times.Once);
        }
    }

    [Test]
    public void GetIcons_PortraitIconsAppearBeforeLandscape()
    {
        TemplateIconViewModel[] icons = TemplateIconViewModel.GetIcons(this.fixture.Services.Object).ToArray();

        // Verify ordering: any portrait icon's index is <= any landscape's index.
        Int32 lastPortraitIndex = -1;
        Int32 firstLandscapeIndex = Int32.MaxValue;
        for (Int32 i = 0; i < icons.Length; i++)
        {
            TabletTemplateIcon underlying = TabletTemplateIcon.Icons.Single(t => t.Code == icons[i].Code);
            if (underlying.IsLandscape) { firstLandscapeIndex = Math.Min(firstLandscapeIndex, i); }
            else { lastPortraitIndex = Math.Max(lastPortraitIndex, i); }
        }

        lastPortraitIndex.Should().BeLessThan(firstLandscapeIndex);
    }

    [Test]
    public void GetIcons_NameIncludesOrientationSuffix()
    {
        TemplateIconViewModel[] icons = TemplateIconViewModel.GetIcons(this.fixture.Services.Object).ToArray();

        icons.Should().OnlyContain(i => i.Name.EndsWith(')'));
    }

    [Test]
    public void GetIcons_ImageObjectIsTheOneFromImageLoader()
    {
        TabletTemplateIcon firstIcon = TabletTemplateIcon.Icons.First();
        IImage expected = Mock.Of<IImage>();
        this.fixture.ImageLoader
            .Setup(l => l.Bitmap($"Templates/{firstIcon.ImageName}.png"))
            .Returns(expected);

        TemplateIconViewModel matching = TemplateIconViewModel.GetIcons(this.fixture.Services.Object)
            .Single(i => i.Code == firstIcon.Code);

        matching.Image.Should().BeSameAs(expected);
    }
}
