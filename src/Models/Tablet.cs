using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace ReMarkableRemember.Models;

public sealed class Tablet : IDisposable
{
    private const String IP = "10.11.99.1";
    private const String SSH_AUTHENTICATION_MESSAGE = "SSH protocol information not configured (Menu > Settings > Help > Copyrights and licenses)";
    private const Int32 SSH_TIMEOUT = 2;
    private const String SSH_TIMEOUT_MESSAGE = "Not connected via WiFi or USB";
    private const String SSH_USER = "root";
    private const String USB_REFUSED_MESSAGE = "USB web interface not activated (Menu > Settings > Storage)";
    private const Int32 USB_TIMEOUT_DOCUMENT = 1;
    private const Int32 USB_TIMEOUT_DOWNLOAD = 10;
    private const String USB_TIMEOUT_MESSAGE = "Not connected via USB";

    private ConnectionInfo sshConnectionInfo;
    private readonly SemaphoreSlim sshSemaphore;
    private readonly HttpClient usbClientDocument;
    private readonly HttpClient usbClientDownload;
    private readonly SemaphoreSlim usbSemaphore;

    public Tablet(String? host, String? password)
    {
        this.sshConnectionInfo = CreateSshConnectionInfo(host, password);
        this.sshSemaphore = new SemaphoreSlim(1, 1);
        this.usbClientDocument = new HttpClient() { Timeout = TimeSpan.FromSeconds(USB_TIMEOUT_DOCUMENT) };
        this.usbClientDownload = new HttpClient() { Timeout = TimeSpan.FromSeconds(USB_TIMEOUT_DOWNLOAD) };
        this.usbSemaphore = new SemaphoreSlim(1, 1);
    }

    public void Dispose()
    {
        this.sshSemaphore.Dispose();
        this.usbClientDocument.Dispose();
        this.usbClientDownload.Dispose();
        this.usbSemaphore.Dispose();

        GC.SuppressFinalize(this);
    }

    public async Task<Stream> Download(String id)
    {
        await this.usbSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            Stream stream = await ExecuteHttp(() => this.usbClientDownload.GetStreamAsync(new Uri($"http://{IP}/download/{id}/placeholder"))).ConfigureAwait(false);
            return stream;
        }
        finally
        {
            this.usbSemaphore.Release();
        }
    }

    public async Task<String?> GetConnectionStatus()
    {
        await this.sshSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            using SftpClient client = this.CreateSftpClient();
        }
        catch (TabletException exception)
        {
            return exception.Message;
        }
        finally
        {
            this.sshSemaphore.Release();
        }

        await this.usbSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            await ExecuteHttp(() => this.usbClientDocument.GetStringAsync(new Uri($"http://{IP}/documents/"))).ConfigureAwait(false);
        }
        catch (TabletException exception)
        {
            return exception.Message;
        }
        finally
        {
            this.usbSemaphore.Release();
        }

        return null;
    }

    public void Setup(String? host, String? password)
    {
        this.sshConnectionInfo = CreateSshConnectionInfo(host, password);
    }

    private SftpClient CreateSftpClient()
    {
        try
        {
            SftpClient client = new SftpClient(this.sshConnectionInfo);
            client.Connect();
            return client;
        }
        catch (ProxyException exception)
        {
            throw new TabletException(SSH_TIMEOUT_MESSAGE, exception);
        }
        catch (SocketException exception)
        {
            throw new TabletException(SSH_TIMEOUT_MESSAGE, exception);
        }
        catch (SshAuthenticationException exception)
        {
            throw new TabletException(SSH_AUTHENTICATION_MESSAGE, exception);
        }
        catch (SshOperationTimeoutException exception)
        {
            throw new TabletException(SSH_TIMEOUT_MESSAGE, exception);
        }
    }

    private static ConnectionInfo CreateSshConnectionInfo(String? host, String? password)
    {
        AuthenticationMethod authenticationMethod = new PasswordAuthenticationMethod(SSH_USER, password ?? "");
        return new ConnectionInfo(host ?? IP, SSH_USER, authenticationMethod) { Timeout = TimeSpan.FromSeconds(SSH_TIMEOUT) };
    }

    private static async Task<T> ExecuteHttp<T>(Func<Task<T>> httpClientRequest)
    {
        try
        {
            return await httpClientRequest().ConfigureAwait(false);
        }
        catch (HttpRequestException exception)
        {
            if (exception.InnerException is SocketException socketException)
            {
                if (socketException.SocketErrorCode is SocketError.ConnectionRefused)
                {
                    throw new TabletException(USB_REFUSED_MESSAGE, exception);
                }
                else if (socketException.SocketErrorCode is SocketError.NetworkDown or SocketError.NetworkUnreachable)
                {
                    throw new TabletException(USB_TIMEOUT_MESSAGE, exception);
                }
            }

            throw new TabletException(exception.Message, exception);
        }
        catch (TaskCanceledException exception)
        {
            throw new TabletException(USB_TIMEOUT_MESSAGE, exception);
        }
    }
}
