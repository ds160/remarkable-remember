using System;
using System.Threading.Tasks;

namespace ReMarkableRemember.Services.TabletService.Communication;

internal interface ICommunicationManager : IDisposable
{
    Task<GitHubCommunication> GitHub();
    Task<SshCommunication> Ssh();
    Task<UsbCommunication> Usb();
}
