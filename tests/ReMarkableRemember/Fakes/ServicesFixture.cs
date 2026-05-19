using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media;
using Moq;
using ReMarkableRemember.DependencyInjection;
using ReMarkableRemember.Images;
using ReMarkableRemember.Services.DataService;
using ReMarkableRemember.Services.DataService.Models;
using ReMarkableRemember.Services.HandWritingRecognitionService;
using ReMarkableRemember.Services.TabletService;
using ReMarkableRemember.Services.TabletService.Configuration;
using ReMarkableRemember.Settings;
using ReMarkableRemember.Settings.Configuration;

namespace ReMarkableRemember.Tests.Fakes;

internal sealed class ServicesFixture
{
    public Mock<IServices> Services { get; } = new Mock<IServices>();
    public Mock<IDataService> Data { get; } = new Mock<IDataService>();
    public Mock<IHandWritingRecognitionService> HandWritingRecognition { get; } = new Mock<IHandWritingRecognitionService>();
    public Mock<IImageLoader> ImageLoader { get; } = new Mock<IImageLoader>();
    public Mock<ISettingsService> Settings { get; } = new Mock<ISettingsService>();
    public Mock<ISettingsConfiguration> SettingsConfiguration { get; } = new Mock<ISettingsConfiguration>();
    public Mock<ITabletService> Tablet { get; } = new Mock<ITabletService>();
    public Mock<ITabletConfiguration> TabletConfiguration { get; } = new Mock<ITabletConfiguration>();
    public Dictionary<String, IImage> StubbedImages { get; } = new Dictionary<String, IImage>();

    public ServicesFixture()
    {
        this.Services.SetupGet(s => s.Data).Returns(this.Data.Object);
        this.Services.SetupGet(s => s.HandWritingRecognition).Returns(this.HandWritingRecognition.Object);
        this.Services.SetupGet(s => s.ImageLoader).Returns(this.ImageLoader.Object);
        this.Services.SetupGet(s => s.Settings).Returns(this.Settings.Object);
        this.Services.SetupGet(s => s.Tablet).Returns(this.Tablet.Object);

        this.Settings.SetupGet(s => s.Configuration).Returns(this.SettingsConfiguration.Object);
        this.Settings.Setup(s => s.GetDateTimeFormat()).Returns("yyyy-MM-dd HH:mm");

        this.Tablet.SetupGet(t => t.Configuration).Returns(this.TabletConfiguration.Object);
        this.TabletConfiguration.SetupGet(c => c.Backup).Returns(String.Empty);
        this.TabletConfiguration.SetupGet(c => c.IP).Returns(String.Empty);
        this.TabletConfiguration.SetupGet(c => c.Password).Returns(String.Empty);

        // Default GetItem returns empty ItemData so DataItem is non-null after Update().
        this.Data
            .Setup(d => d.GetItem(It.IsAny<String>()))
            .Returns<String>(id => Task.FromResult(CreateItemData(id, null, null, null, null)));

        // Default image loader returns a stub IImage so consumers don't NPE.
        this.ImageLoader
            .Setup(l => l.Svg(It.IsAny<String>()))
            .Returns(this.GetOrCreateImage);
        this.ImageLoader
            .Setup(l => l.Bitmap(It.IsAny<String>()))
            .Returns(this.GetOrCreateImage);
    }

    public IImage GetOrCreateImage(String path)
    {
        if (!this.StubbedImages.TryGetValue(path, out IImage? image))
        {
            image = Mock.Of<IImage>();
            this.StubbedImages.Add(path, image);
        }
        return image;
    }

    public static ItemData CreateItemData(String id, DateTime? backupDate, DateTime? syncData, String? syncPath, String? syncTargetDirectory)
    {
        // ItemData's constructor is internal — reachable via InternalsVisibleTo from DataService.
        return new ItemData(id, backupDate, syncData, syncPath, syncTargetDirectory);
    }
}
