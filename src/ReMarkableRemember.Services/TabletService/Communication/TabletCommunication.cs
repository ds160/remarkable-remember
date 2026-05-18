using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ReMarkableRemember.Services.TabletService.Communication.Interfaces;
using ReMarkableRemember.Services.TabletService.Configuration;

namespace ReMarkableRemember.Services.TabletService.Communication;

internal sealed class TabletCommunication : ITabletCommunication
{
    private const String IP = "10.11.99.1";
    private const Int32 USB_TIMEOUT = 1;

    private readonly HttpClient gitHubHttpClient;
    private readonly SemaphoreSlim gitHubSemaphore;
    private readonly SemaphoreSlim sshSemaphore;
    private readonly HttpClient usbHttpClient;
    private readonly HttpClient usbHttpClientConnection;
    private readonly SemaphoreSlim usbSemaphore;

    private ITabletConfiguration? configuration;

    public TabletCommunication()
    {
        this.gitHubHttpClient = new HttpClient() { BaseAddress = new Uri("https://raw.githubusercontent.com") };
        this.gitHubSemaphore = new SemaphoreSlim(1, 1);
        this.sshSemaphore = new SemaphoreSlim(1, 1);
        this.usbHttpClient = new HttpClient() { BaseAddress = new Uri($"http://{IP}") };
        this.usbHttpClientConnection = new HttpClient() { BaseAddress = new Uri($"http://{IP}"), Timeout = TimeSpan.FromSeconds(USB_TIMEOUT) };
        this.usbSemaphore = new SemaphoreSlim(1, 1);
    }

    public void Configuration(ITabletConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public async Task<IGitHubCommunication> GitHub()
    {
        await this.gitHubSemaphore.WaitAsync().ConfigureAwait(false);

        return new GitHubCommunication(this.gitHubHttpClient, this.gitHubSemaphore);
    }

    public async Task<ISshCommunication> Ssh()
    {
        if (this.configuration is null) { throw new InvalidOperationException(); }

        await this.sshSemaphore.WaitAsync().ConfigureAwait(false);

        SshCommunication ssh = new SshCommunication(String.IsNullOrEmpty(this.configuration.IP) ? IP : this.configuration.IP, this.configuration.Password, this.sshSemaphore);

        try
        {
            await ssh.Connect().ConfigureAwait(false);
            return ssh;
        }
        catch
        {
            ssh.Dispose();
            throw;
        }
    }

    public async Task<IUsbCommunication> Usb()
    {
        await this.usbSemaphore.WaitAsync().ConfigureAwait(false);

        return new UsbCommunication(this.usbHttpClient, this.usbHttpClientConnection, this.usbSemaphore);
    }

    void IDisposable.Dispose()
    {
        this.gitHubHttpClient.Dispose();
        this.gitHubSemaphore.Dispose();
        this.sshSemaphore.Dispose();
        this.usbHttpClient.Dispose();
        this.usbHttpClientConnection.Dispose();
        this.usbSemaphore.Dispose();
    }
}
