using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ReMarkableRemember.Services.TabletService.Configuration;

namespace ReMarkableRemember.Services.TabletService.Communication;

internal sealed class CommunicationManager : ICommunicationManager
{
    internal const String IP = "10.11.99.1";
    private const Int32 USB_TIMEOUT = 1;

    private readonly ITabletConfiguration configuration;
    private readonly SemaphoreSlim sshSemaphore;
    private readonly HttpClient usbHttpClient;
    private readonly HttpClient usbHttpClientConnection;
    private readonly SemaphoreSlim usbSemaphore;

    public CommunicationManager(ITabletConfiguration configuration)
    {
        this.configuration = configuration;
        this.sshSemaphore = new SemaphoreSlim(1, 1);
        this.usbHttpClient = new HttpClient();
        this.usbHttpClientConnection = new HttpClient() { Timeout = TimeSpan.FromSeconds(USB_TIMEOUT) };
        this.usbSemaphore = new SemaphoreSlim(1, 1);
    }

    public void Dispose()
    {
        this.sshSemaphore.Dispose();
        this.usbHttpClient.Dispose();
        this.usbHttpClientConnection.Dispose();
        this.usbSemaphore.Dispose();
    }

    public Task<GitHubCommunication> GitHub()
    {
        return Task.FromResult(new GitHubCommunication());
    }

    public async Task<SshCommunication> Ssh()
    {
        await this.sshSemaphore.WaitAsync().ConfigureAwait(false);

        SshCommunication ssh = new SshCommunication(String.IsNullOrEmpty(this.configuration.IP) ? IP : this.configuration.IP, this.configuration.Password, this.sshSemaphore);
        await ssh.Connect().ConfigureAwait(false);
        return ssh;
    }

    public async Task<UsbCommunication> Usb()
    {
        await this.usbSemaphore.WaitAsync().ConfigureAwait(false);

        return new UsbCommunication(this.usbHttpClient, this.usbHttpClientConnection, this.usbSemaphore);
    }
}
