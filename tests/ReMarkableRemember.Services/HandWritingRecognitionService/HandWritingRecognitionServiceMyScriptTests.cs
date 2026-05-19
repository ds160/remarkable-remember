using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ReMarkableRemember.Common.Notebook;
using ReMarkableRemember.Services.ConfigurationService;
using ReMarkableRemember.Services.ConfigurationService.Configuration;
using ReMarkableRemember.Services.HandWritingRecognitionService.Exceptions;
using ReMarkableRemember.Services.HandWritingRecognitionService.MyScript;
using ReMarkableRemember.Services.HandWritingRecognitionService.MyScript.Interfaces;
using ReMarkableRemember.Services.HandWritingRecognitionService.Tests.Helpers;

namespace ReMarkableRemember.Services.HandWritingRecognitionService.Tests;

[TestFixture]
public sealed class HandWritingRecognitionServiceMyScriptTests
{
    private Mock<IMyScriptCommunication> communicationMock = null!;
    private Mock<IConfigurationService> configurationServiceMock = null!;

    [SetUp]
    public void SetUp()
    {
        this.communicationMock = new Mock<IMyScriptCommunication>();
        this.configurationServiceMock = new Mock<IConfigurationService>();
        this.configurationServiceMock
            .Setup(s => s.Load(It.IsAny<ConfigurationBase>()))
            .Returns(Task.CompletedTask);
    }

    private HandWritingRecognitionServiceMyScript CreateService()
    {
        return new HandWritingRecognitionServiceMyScript(this.communicationMock.Object, this.configurationServiceMock.Object);
    }

    [Test]
    public void Constructor_PassesConfigurationToCommunication()
    {
        IHandWritingRecognitionService service = this.CreateService();

        this.communicationMock.Verify(c => c.Configuration(service.Configuration), Times.Once);
    }

    [Test]
    public void SupportedLanguages_ReturnsMyScriptSupportedList()
    {
        IHandWritingRecognitionService service = this.CreateService();

        service.SupportedLanguages.Should().BeEquivalentTo(MyScriptLanguages.Supported);
    }

    [Test]
    public async Task Recognize_UnsupportedLanguage_ThrowsHandWritingRecognitionException()
    {
        IHandWritingRecognitionService service = this.CreateService();
        service.Configuration.Language = "xx_NOT_A_LANG";
        Notebook notebook = NotebookFixture.EmptyVersion5Notebook();

        Func<Task> act = () => service.Recognize(notebook);

        await act.Should().ThrowAsync<HandWritingRecognitionException>();
        this.communicationMock.Verify(c => c.Recognize(It.IsAny<String>(), It.IsAny<String>()), Times.Never);
    }

    [Test]
    public async Task Recognize_UnauthorizedResponse_ThrowsHandWritingRecognitionException()
    {
        Mock<IMyScriptResponse> responseMock = new Mock<IMyScriptResponse>();
        responseMock.Setup(r => r.Unauthorized).Returns(true);
        responseMock.Setup(r => r.RequestTooLarge).Returns(false);
        this.communicationMock
            .Setup(c => c.Recognize(It.IsAny<String>(), It.IsAny<String>()))
            .ReturnsAsync(responseMock.Object);

        HandWritingRecognitionServiceMyScript service = this.CreateService();
        Notebook notebook = NotebookFixture.EmptyVersion5Notebook();

        Func<Task> act = () => service.Recognize(notebook);

        await act.Should().ThrowAsync<HandWritingRecognitionException>();
    }

    [Test]
    public async Task Recognize_RequestTooLargeResponse_ThrowsHandWritingRecognitionException()
    {
        Mock<IMyScriptResponse> responseMock = new Mock<IMyScriptResponse>();
        responseMock.Setup(r => r.Unauthorized).Returns(false);
        responseMock.Setup(r => r.RequestTooLarge).Returns(true);
        this.communicationMock
            .Setup(c => c.Recognize(It.IsAny<String>(), It.IsAny<String>()))
            .ReturnsAsync(responseMock.Object);

        HandWritingRecognitionServiceMyScript service = this.CreateService();
        Notebook notebook = NotebookFixture.EmptyVersion5Notebook();

        Func<Task> act = () => service.Recognize(notebook);

        await act.Should().ThrowAsync<HandWritingRecognitionException>();
    }

    [Test]
    public async Task Recognize_HappyPath_ReturnsResponseText()
    {
        Mock<IMyScriptResponse> responseMock = new Mock<IMyScriptResponse>();
        responseMock.Setup(r => r.Unauthorized).Returns(false);
        responseMock.Setup(r => r.RequestTooLarge).Returns(false);
        responseMock.Setup(r => r.Read()).ReturnsAsync("hello world");
        this.communicationMock
            .Setup(c => c.Recognize(It.IsAny<String>(), It.IsAny<String>()))
            .ReturnsAsync(responseMock.Object);

        HandWritingRecognitionServiceMyScript service = this.CreateService();
        Notebook notebook = NotebookFixture.EmptyVersion5Notebook(pageCount: 1);

        String result = await service.Recognize(notebook);

        result.Should().Be("hello world");
    }

    [Test]
    public async Task Recognize_MultiplePages_JoinsResultsWithNewLine()
    {
        Int32 callCount = 0;
        this.communicationMock
            .Setup(c => c.Recognize(It.IsAny<String>(), It.IsAny<String>()))
            .ReturnsAsync(() =>
            {
                Int32 index = System.Threading.Interlocked.Increment(ref callCount);
                Mock<IMyScriptResponse> responseMock = new Mock<IMyScriptResponse>();
                responseMock.Setup(r => r.Unauthorized).Returns(false);
                responseMock.Setup(r => r.RequestTooLarge).Returns(false);
                responseMock.Setup(r => r.Read()).ReturnsAsync($"page-{index}");
                return responseMock.Object;
            });

        HandWritingRecognitionServiceMyScript service = this.CreateService();
        Notebook notebook = NotebookFixture.EmptyVersion5Notebook(pageCount: 3);

        String result = await service.Recognize(notebook);

        result.Split(Environment.NewLine).Should().HaveCount(3);
        // The pages are processed concurrently, so the specific assignment order is non-deterministic
        // but every "page-N" should appear exactly once.
        foreach (Int32 i in new[] { 1, 2, 3 })
        {
            result.Should().Contain($"page-{i}");
        }
    }

    [Test]
    public async Task Recognize_DisposesResponse()
    {
        Mock<IMyScriptResponse> responseMock = new Mock<IMyScriptResponse>();
        responseMock.Setup(r => r.Read()).ReturnsAsync(String.Empty);
        this.communicationMock
            .Setup(c => c.Recognize(It.IsAny<String>(), It.IsAny<String>()))
            .ReturnsAsync(responseMock.Object);

        HandWritingRecognitionServiceMyScript service = this.CreateService();
        Notebook notebook = NotebookFixture.EmptyVersion5Notebook();

        await service.Recognize(notebook);

        responseMock.Verify(r => r.Dispose(), Times.Once);
    }
}
