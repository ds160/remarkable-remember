using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ReMarkableRemember.Common.Localization.Interfaces;

namespace ReMarkableRemember.Common.Localization.Tests;

[TestFixture]
public sealed class LanguageTests
{
    private ILanguageProvider originalProvider = null!;

    [SetUp]
    public void SetUp()
    {
        this.originalProvider = Language.Provider;
    }

    [TearDown]
    public void TearDown()
    {
        Language.SetProvioder(this.originalProvider);
    }

    [Test]
    public void Current_ReturnsCurrentFromProvider()
    {
        Mock<ILocalStrings> stringsMock = new Mock<ILocalStrings>();
        Mock<ILanguageProvider> providerMock = new Mock<ILanguageProvider>();
        providerMock.Setup(p => p.Current).Returns(stringsMock.Object);
        providerMock.Setup(p => p.CurrentCode).Returns(String.Empty);
        providerMock.Setup(p => p.SupportedCodes).Returns(new List<String>());

        Language.SetProvioder(providerMock.Object);

        Language.Current.Should().BeSameAs(stringsMock.Object);
    }

    [Test]
    public void SetProvioder_PreservesCurrentCodeAcrossSwap()
    {
        Language.SetProvioder(new LanguageProvider());
        Language.Provider.Switch("en");

        Mock<ILanguageProvider> newProvider = new Mock<ILanguageProvider>();
        newProvider.Setup(p => p.CurrentCode).Returns(String.Empty);
        newProvider.Setup(p => p.Current).Returns(Mock.Of<ILocalStrings>());

        Language.SetProvioder(newProvider.Object);

        newProvider.Verify(p => p.Switch("en"), Times.Once);
    }

    [Test]
    public void SetProvioder_NewProviderBecomesActive()
    {
        Mock<ILanguageProvider> newProvider = new Mock<ILanguageProvider>();
        newProvider.Setup(p => p.CurrentCode).Returns(String.Empty);
        newProvider.Setup(p => p.Current).Returns(Mock.Of<ILocalStrings>());

        Language.SetProvioder(newProvider.Object);

        Language.Provider.Should().BeSameAs(newProvider.Object);
    }
}
