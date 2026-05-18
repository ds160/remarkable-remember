using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ReMarkableRemember.Common.FileSystem;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Services.TabletService.Communication.Interfaces;
using ReMarkableRemember.Services.TabletService.Exceptions;
using ReMarkableRemember.Services.TabletService.Files;
using ReMarkableRemember.Services.TabletService.Files.Interfaces;
using ReMarkableRemember.Services.TabletService.Models.Enumerations;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace ReMarkableRemember.Services.TabletService.Communication;

internal sealed class SshCommunication : CommunicationBase, ISshCommunication
{
    private const Int32 SSH_TIMEOUT = 2;
    private const String SSH_USER = "root";

    private readonly ConnectionInfo connectionInfo;
    private readonly SftpClient sftpClient;
    private SshClient? sshClient;

    public SshCommunication(String ip, String password, SemaphoreSlim semaphore)
        : base(semaphore)
    {
        this.connectionInfo = new ConnectionInfo(ip, SSH_USER, new PasswordAuthenticationMethod(SSH_USER, password)) { Timeout = TimeSpan.FromSeconds(SSH_TIMEOUT) };
        this.sftpClient = new SftpClient(this.connectionInfo);
    }

    public async Task Connect()
    {
        await Connect(this.sftpClient).ConfigureAwait(false);
    }

    public sealed override void Dispose()
    {
        this.sftpClient.Dispose();
        this.sshClient?.Dispose();

        base.Dispose();
    }

    public async Task Execute(String command, Boolean checkResult = true)
    {
        if (this.sshClient == null)
        {
            this.sshClient = new SshClient(this.connectionInfo);
            await Connect(this.sshClient).ConfigureAwait(false);
        }

        SshCommand result = await Task.Run(() => this.sshClient.RunCommand(command)).ConfigureAwait(false);
        if (checkResult && result.ExitStatus != 0)
        {
            throw new TabletException(result.Error);
        }
    }

    public async Task FileDelete(String path)
    {
        CancellationToken cancellationToken = default;
        Boolean exists = await this.sftpClient.ExistsAsync(path, cancellationToken).ConfigureAwait(false);
        if (exists)
        {
            await this.sftpClient.DeleteFileAsync(path, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task FileDownload(String path, String targetPath)
    {
        using Stream fileStream = FileSystem.Create(targetPath);
        await this.sftpClient.DownloadFileAsync(path, fileStream).ConfigureAwait(false);
    }

    public async Task<IEnumerable<ITabletFileInfo>> FileList(String directoryPath)
    {
        List<ITabletFileInfo> result = new List<ITabletFileInfo>();

        IEnumerable<ISftpFile> files = await Task.Run(() => this.sftpClient.ListDirectory(directoryPath)).ConfigureAwait(false);
        foreach (ISftpFile file in files)
        {
            result.Add(new TabletFileInfo(file));
        }

        return result;
    }

    public async Task<Byte[]> FileReadBytes(String path)
    {
        return await Task.Run(() => this.sftpClient.ReadAllBytes(path)).ConfigureAwait(false);
    }

    public async Task<String> FileReadText(String path)
    {
        return await Task.Run(() => this.sftpClient.ReadAllText(path)).ConfigureAwait(false);
    }

    public async Task FileWrite(String path, Object content, Boolean contentRequired = true)
    {
        await this.FileDelete(path).ConfigureAwait(false);

        if (content is String text)
        {
            if (text.Length > 0) { await Task.Run(() => this.sftpClient.WriteAllText(path, text)).ConfigureAwait(false); }
            else if (contentRequired) { throw new InvalidOperationException(); }
        }
        else if (content is Byte[] bytes)
        {
            if (bytes.Length > 0) { await Task.Run(() => this.sftpClient.WriteAllBytes(path, bytes)).ConfigureAwait(false); }
            else if (contentRequired) { throw new InvalidOperationException(); }
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private static async Task Connect<T>(T client) where T : BaseClient
    {
        try
        {
            await Task.Run(client.Connect).ConfigureAwait(false);
        }
        catch (ProxyException exception)
        {
            throw new TabletException(exception.Message, exception);
        }
        catch (SocketException exception)
        {
            if (exception.SocketErrorCode is SocketError.ConnectionRefused)
            {
                throw new TabletException(TabletError.SshNotConfigured, Language.Current.TabletSshNotConfigured, exception);
            }

            if (exception.SocketErrorCode is SocketError.HostDown or SocketError.HostUnreachable or SocketError.NetworkDown or SocketError.NetworkUnreachable)
            {
                throw new TabletException(TabletError.SshNotConnected, Language.Current.TabletSshNotConnected, exception);
            }

            throw new TabletException(exception.Message, exception);
        }
        catch (SshAuthenticationException exception)
        {
            throw new TabletException(TabletError.SshNotConfigured, Language.Current.TabletSshNotConfigured, exception);
        }
        catch (SshConnectionException exception)
        {
            throw new TabletException(TabletError.SshNotConnected, Language.Current.TabletSshNotConnected, exception);
        }
        catch (SshOperationTimeoutException exception)
        {
            throw new TabletException(TabletError.SshNotConnected, Language.Current.TabletSshNotConnected, exception);
        }
        catch (Exception exception)
        {
            throw new TabletException(exception.Message, exception);
        }
    }
}
