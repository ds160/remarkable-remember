using System;
using Avalonia.Media;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ReMarkableRemember.Tests.Fakes;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Tests;

[TestFixture]
public sealed class MessageViewModelTests
{
    private ServicesFixture fixture = null!;

    [SetUp]
    public void SetUp()
    {
        this.fixture = new ServicesFixture();
    }

    [Test]
    public void Error_FromString_LoadsErrorSvgAndSetsMessage()
    {
        MessageViewModel vm = MessageViewModel.Error("things broke", this.fixture.Services.Object);

        vm.Message.Should().Be("things broke");
        this.fixture.ImageLoader.Verify(l => l.Svg("Messages/Error.svg"), Times.Once);
    }

    [Test]
    public void Error_FromException_UsesExceptionMessage()
    {
        InvalidOperationException ex = new InvalidOperationException("inner-message");

        MessageViewModel vm = MessageViewModel.Error(ex, this.fixture.Services.Object);

        vm.Message.Should().Be("inner-message");
    }

    [Test]
    public void Error_OnlyHasCloseButton()
    {
        MessageViewModel vm = MessageViewModel.Error("x", this.fixture.Services.Object);

        vm.TextClose.Should().NotBeNullOrEmpty();
        vm.TextCancel.Should().BeNull("Error dialogs only need OK");
    }

    [Test]
    public void Question_HasCloseAndCancelButtonsAndLoadsQuestionIcon()
    {
        MessageViewModel vm = MessageViewModel.Question("Title", "Are you sure?", this.fixture.Services.Object);

        vm.Title.Should().Be("Title");
        vm.Message.Should().Be("Are you sure?");
        vm.TextClose.Should().NotBeNullOrEmpty();
        vm.TextCancel.Should().NotBeNullOrEmpty();
        this.fixture.ImageLoader.Verify(l => l.Svg("Messages/Question.svg"), Times.Once);
    }

    [Test]
    public void Image_IsSameInstanceImageLoaderReturned()
    {
        IImage stub = Mock.Of<IImage>();
        this.fixture.ImageLoader.Setup(l => l.Svg("Messages/Error.svg")).Returns(stub);

        MessageViewModel vm = MessageViewModel.Error("x", this.fixture.Services.Object);

        vm.Image.Should().BeSameAs(stub);
    }
}
