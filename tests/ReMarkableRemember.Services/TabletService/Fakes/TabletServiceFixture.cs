using System.Threading.Tasks;
using Moq;
using ReMarkableRemember.Services.ConfigurationService;
using ReMarkableRemember.Services.ConfigurationService.Configuration;
using ReMarkableRemember.Services.TabletService.Communication.Interfaces;
using ReMarkableRemember.Services.TabletService.Files.Interfaces;

namespace ReMarkableRemember.Services.TabletService.Tests.Fakes;

internal sealed class TabletServiceFixture
{
    public TabletServiceFixture()
    {
        this.Communication = new Mock<ITabletCommunication>();
        this.Ssh = new Mock<ISshCommunication>();
        this.Usb = new Mock<IUsbCommunication>();
        this.GitHub = new Mock<IGitHubCommunication>();
        this.FileSerializer = new Mock<ITabletFileSerializer>();
        this.ConfigurationService = new Mock<IConfigurationService>();

        this.ConfigurationService
            .Setup(s => s.Load(It.IsAny<ConfigurationBase>()))
            .Returns(Task.CompletedTask);

        this.Communication.Setup(c => c.Ssh()).ReturnsAsync(this.Ssh.Object);
        this.Communication.Setup(c => c.Usb()).ReturnsAsync(this.Usb.Object);
        this.Communication.Setup(c => c.GitHub()).ReturnsAsync(this.GitHub.Object);
    }

    public Mock<ITabletCommunication> Communication { get; }
    public Mock<ISshCommunication> Ssh { get; }
    public Mock<IUsbCommunication> Usb { get; }
    public Mock<IGitHubCommunication> GitHub { get; }
    public Mock<ITabletFileSerializer> FileSerializer { get; }
    public Mock<IConfigurationService> ConfigurationService { get; }

    public TabletService Build()
    {
        return new TabletService(this.Communication.Object, this.FileSerializer.Object, this.ConfigurationService.Object);
    }
}
