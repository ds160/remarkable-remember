using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace ReMarkableRemember.Models;

public sealed class TabletException : Exception
{
    public TabletException() { }

    public TabletException(String message) : base(message) { }

    public TabletException(String message, Exception innerException) : base(message, innerException) { }
}

internal struct TabletFileContent
{
    public String FileType { get; set; }
    public Int32 FormatVersion { get; set; }
    public TabletFileContentPageCollection CPages { get; set; }
}

internal struct TabletFileContentPage
{
    public Object? Deleted { get; set; }
    public String Id { get; set; }
}

internal struct TabletFileContentPageCollection
{
    public Collection<TabletFileContentPage> Pages { get; set; }
}

internal struct TabletFileMetaData
{
    public Boolean? Deleted { get; set; }
    public String LastModified { get; set; }
    public String Parent { get; set; }
    public String Type { get; set; }
    public String VisibleName { get; set; }
}

public class TabletItem
{
    internal TabletItem(String id, TabletFileMetaData fileMetaData)
    {
        this.Collection = (fileMetaData.Type == "CollectionType") ? new Collection<TabletItem>() : null;
        this.Id = id;
        this.Modified = DateTime.UnixEpoch.AddMilliseconds(Double.Parse(fileMetaData.LastModified, CultureInfo.InvariantCulture));
        this.Name = fileMetaData.VisibleName;
        this.ParentCollectionId = fileMetaData.Parent;
        this.Trashed = fileMetaData.Parent == "trash";
    }

    public Collection<TabletItem>? Collection { get; }
    public String Id { get; }
    public DateTime Modified { get; }
    public String Name { get; }
    public String ParentCollectionId { get; }
    public Boolean Trashed { get; set; }
}

public sealed class Tablet : IDisposable
{
    private const String IP = "10.11.99.1";
    private const String PATH_NOTEBOOKS = "/home/root/.local/share/remarkable/xochitl";
    private const String SSH_AUTHENTICATION_MESSAGE = "SSH protocol information not configured (Menu > Settings > Help > Copyrights and licenses)";
    private const Int32 SSH_TIMEOUT = 2;
    private const String SSH_TIMEOUT_MESSAGE = "Not connected via WiFi or USB";
    private const String SSH_USER = "root";
    private const String USB_REFUSED_MESSAGE = "USB web interface not activated (Menu > Settings > Storage)";
    private const Int32 USB_TIMEOUT_DOCUMENT = 1;
    private const Int32 USB_TIMEOUT_DOWNLOAD = 10;
    private const String USB_TIMEOUT_MESSAGE = "Not connected via USB";

    private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

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

    public async Task<IEnumerable<TabletItem>> GetItems()
    {
        await this.sshSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            using SftpClient client = this.CreateSftpClient();

            IEnumerable<ISftpFile> files = client
                .ListDirectory(PATH_NOTEBOOKS)
                .Where(file => file.IsRegularFile && file.Name.EndsWith(".metadata", StringComparison.OrdinalIgnoreCase));

            List<TabletItem> tabletItems = new List<TabletItem>();
            foreach (ISftpFile file in files)
            {
                String fileText = client.ReadAllText(file.FullName);
                TabletFileMetaData fileMetaData = JsonSerializer.Deserialize<TabletFileMetaData>(fileText, jsonSerializerOptions);
                if (fileMetaData.Deleted != true)
                {
                    tabletItems.Add(new TabletItem(Path.GetFileNameWithoutExtension(file.Name), fileMetaData));
                }
            }

            IEnumerable<TabletItem> items = tabletItems.Where(item => String.IsNullOrEmpty(item.ParentCollectionId) || item.Trashed);
            foreach (TabletItem item in items) { this.UpdateTabletItems(item, tabletItems); }
            return items;
        }
        finally
        {
            this.sshSemaphore.Release();
        }
    }

    public async Task<IEnumerable<Notebook>> GetNotebook(String id)
    {
        await this.sshSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            using SftpClient client = this.CreateSftpClient();

            String fileText = client.ReadAllText(Path.Combine(PATH_NOTEBOOKS, $"{id}.content"));
            TabletFileContent fileContent = JsonSerializer.Deserialize<TabletFileContent>(fileText, jsonSerializerOptions);

            if (fileContent.FileType != "notebook") { throw new NotebookException("Unsupported file type."); }
            if (fileContent.FormatVersion != 2) { throw new NotebookException("Unsupported file format version."); }

            IEnumerable<Notebook> notebook = fileContent.CPages.Pages.Where(page => page.Deleted == null).Select(page =>
            {
                Byte[] notebookBuffer = client.ReadAllBytes(Path.Combine(PATH_NOTEBOOKS, id, $"{page.Id}.rm"));
                return new Notebook(notebookBuffer);
            }).ToList();

            return notebook;
        }
        finally
        {
            this.sshSemaphore.Release();
        }
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

    private void UpdateTabletItems(TabletItem parentItem, IEnumerable<TabletItem> tabletItems)
    {
        IEnumerable<TabletItem> children = tabletItems.Where(item => item.ParentCollectionId == parentItem.Id);
        foreach (TabletItem child in children)
        {
            child.Trashed = parentItem.Trashed;
            parentItem.Collection?.Add(child);

            this.UpdateTabletItems(child, tabletItems);
        }
    }
}
