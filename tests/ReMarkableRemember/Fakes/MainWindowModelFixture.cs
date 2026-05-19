using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using ReactiveUI.Builder;
using ReMarkableRemember.Services.HandWritingRecognitionService.Configuration;
using ReMarkableRemember.Services.TabletService.Models;
using ReMarkableRemember.Services.TabletService.Models.Enumerations;
using ReMarkableRemember.ViewModels;

namespace ReMarkableRemember.Tests.Fakes;

internal sealed class MainWindowModelFixture
{
    public ServicesFixture Services { get; } = new ServicesFixture();
    public Mock<IHandWritingRecognitionConfiguration> HwrConfiguration { get; } = new Mock<IHandWritingRecognitionConfiguration>();

    static MainWindowModelFixture()
    {
        // ReactiveUI requires explicit initialization before WhenAnyValue is used.
        RxAppBuilder.CreateReactiveUIBuilder().BuildApp();
    }

    public MainWindowModelFixture()
    {
        this.Services.Tablet.Setup(t => t.GetConnectionStatus()).Returns(() => Task.FromResult(this.ConnectionStatus));

        this.Services.Tablet.Setup(t => t.GetItems()).Returns(() => Task.FromResult(this.Items));

        this.HwrConfiguration.SetupProperty(c => c.Language, "en_US");
        this.HwrConfiguration.Setup(c => c.Save()).Returns(Task.CompletedTask);
        this.Services.HandWritingRecognition.Setup(h => h.SupportedLanguages).Returns(["en_US", "de_DE"]);
        this.Services.HandWritingRecognition.Setup(h => h.Configuration).Returns(this.HwrConfiguration.Object);

        this.Services.SettingsConfiguration.SetupGet(c => c.ApplicationTheme).Returns("Default");
        this.Services.SettingsConfiguration.SetupGet(c => c.ApplicationLanguage).Returns(String.Empty);

        this.Services.Data.Setup(d => d.GetTemplates()).ReturnsAsync(Array.Empty<Services.DataService.Models.TemplateData>());
    }

    public TabletConnectionStatus ConnectionStatus { get; set; } = TabletConnectionStatus.Default;

    public TabletItems Items { get; set; } = new TabletItems(Array.Empty<TabletItem>(), new Dictionary<String, Exception>());

    public MainWindowModel Build()
    {
        return MainWindowModel.CreateForTesting(this.Services.Services.Object);
    }

    public static TabletConnectionStatus MakeStatus(TabletInformation? info, TabletError? error)
    {
        return new TabletConnectionStatus(info, error);
    }
}
