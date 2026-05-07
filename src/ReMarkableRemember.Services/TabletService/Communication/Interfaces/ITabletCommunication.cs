using System;
using System.Threading.Tasks;
using ReMarkableRemember.Services.TabletService.Configuration;

namespace ReMarkableRemember.Services.TabletService.Communication.Interfaces;

public interface ITabletCommunication : IDisposable
{
    void Configuration(ITabletConfiguration configuration);

    Task<IGitHubCommunication> GitHub();

    Task<ISshCommunication> Ssh();

    Task<IUsbCommunication> Usb();
}
