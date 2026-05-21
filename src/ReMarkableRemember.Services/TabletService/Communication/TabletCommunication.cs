using System;
using System.Threading;
using System.Threading.Tasks;
using ReMarkableRemember.Services.TabletService.Communication.Interfaces;
using ReMarkableRemember.Services.TabletService.Configuration;

namespace ReMarkableRemember.Services.TabletService.Communication;

internal sealed class TabletCommunication : ITabletCommunication
{
    private readonly SemaphoreSlim gitHubSemaphore;
    private readonly SemaphoreSlim sshSemaphore;
    private readonly SemaphoreSlim usbSemaphore;

    private ITabletConfiguration? configuration;

    public TabletCommunication()
    {
        this.gitHubSemaphore = new SemaphoreSlim(1, 1);
        this.sshSemaphore = new SemaphoreSlim(1, 1);
        this.usbSemaphore = new SemaphoreSlim(1, 1);
    }

    public void Configuration(ITabletConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public async Task<IGitHubCommunication> GitHub()
    {
        await this.gitHubSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            return new GitHubCommunication(this.gitHubSemaphore);
        }
        catch
        {
            this.gitHubSemaphore.Release();
            throw;
        }
    }

    public async Task<ISshCommunication> Ssh()
    {
        if (this.configuration is null) { throw new InvalidOperationException(); }

        await this.sshSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            SshCommunication ssh = new SshCommunication(this.configuration.IP, this.configuration.Password, this.sshSemaphore);
            await ssh.Connect().ConfigureAwait(false);
            return ssh;
        }
        catch
        {
            this.sshSemaphore.Release();
            throw;
        }
    }

    public async Task<IUsbCommunication> Usb()
    {
        await this.usbSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            UsbCommunication usb = new UsbCommunication(this.usbSemaphore);
            await usb.CheckConnection().ConfigureAwait(false);
            return usb;
        }
        catch
        {
            this.usbSemaphore.Release();
            throw;
        }
    }

    void IDisposable.Dispose()
    {
        this.gitHubSemaphore.Dispose();
        this.sshSemaphore.Dispose();
        this.usbSemaphore.Dispose();
    }
}
