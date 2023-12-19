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

internal sealed class Tablet : IDisposable
{
    private const String IP = "10.11.99.1";
    private const String PATH_NOTEBOOKS = "/home/root/.local/share/remarkable/xochitl";
    private const Int32 SSH_TIMEOUT = 2;
    private const String SSH_USER = "root";
    private const Int32 USB_TIMEOUT_DOCUMENT = 1;
    private const Int32 USB_TIMEOUT_DOWNLOAD = 10;

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

    public async Task<TabletConnectionError?> GetConnectionStatus()
    {
        await this.sshSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            using SftpClient client = this.CreateSftpClient();
        }
        catch (TabletException exception)
        {
            return exception.Error;
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
            return exception.Error;
        }
        finally
        {
            this.usbSemaphore.Release();
        }

        return null;
    }

    public async Task<IEnumerable<Item>> GetItems()
    {
        await this.sshSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            using SftpClient client = this.CreateSftpClient();

            IEnumerable<ISftpFile> files = client
                .ListDirectory(PATH_NOTEBOOKS)
                .Where(file => file.IsRegularFile && file.Name.EndsWith(".metadata", StringComparison.OrdinalIgnoreCase));

            List<Item> allItems = new List<Item>();
            foreach (ISftpFile file in files)
            {
                String metaDataFileText = client.ReadAllText(file.FullName);
                MetaDataFile metaDataFile = JsonSerializer.Deserialize<MetaDataFile>(metaDataFileText, jsonSerializerOptions);
                if (metaDataFile.Deleted != true)
                {
                    String id = Path.GetFileNameWithoutExtension(file.Name);
                    allItems.Add(new Item(id, metaDataFile.LastModified, metaDataFile.Parent, metaDataFile.Type, metaDataFile.VisibleName));
                }
            }

            IEnumerable<Item> items = allItems.Where(item => String.IsNullOrEmpty(item.ParentCollectionId) || item.Trashed);
            foreach (Item item in items) { this.UpdateItems(item, allItems); }
            return items;
        }
        finally
        {
            this.sshSemaphore.Release();
        }
    }

    public async Task<Notebook> GetNotebook(String id)
    {
        await this.sshSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            using SftpClient client = this.CreateSftpClient();

            String contentFileText = client.ReadAllText(Path.Combine(PATH_NOTEBOOKS, $"{id}.content"));
            ContentFile contentFile = JsonSerializer.Deserialize<ContentFile>(contentFileText, jsonSerializerOptions);

            if (contentFile.FileType != "notebook") { throw new NotebookException($"Invalid reMarkable file type: '{contentFile.FileType}'."); }
            if (contentFile.FormatVersion != 2) { throw new NotebookException($"Invalid reMarkable file format version: '{contentFile.FormatVersion}'."); }

            IEnumerable<Byte[]> pageBuffers = contentFile.CPages.Pages
                .Where(page => page.Deleted == null)
                .Select(page => client.ReadAllBytes(Path.Combine(PATH_NOTEBOOKS, id, $"{page.Id}.rm")))
                .ToArray();

            return new Notebook(pageBuffers);
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
            throw new TabletException(exception.Message, exception);
        }
        catch (SocketException exception)
        {
            throw new TabletException(exception.Message, exception);
        }
        catch (SshAuthenticationException exception)
        {
            throw new TabletException(TabletConnectionError.SshNotConfigured, "SSH protocol information are not configured or wrong (Menu > Settings > Help > Copyrights and licenses).", exception);
        }
        catch (SshOperationTimeoutException exception)
        {
            throw new TabletException(TabletConnectionError.SshNotConnected, "reMarkable is not connected via WiFi or USB.", exception);
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
                if (socketException.SocketErrorCode is SocketError.ConnectionRefused or SocketError.NetworkDown or SocketError.NetworkUnreachable)
                {
                    throw new TabletException(TabletConnectionError.UsbNotActived, "USB web interface is not activated (Menu > Settings > Storage).", exception);
                }
            }

            throw new TabletException(exception.Message, exception);
        }
        catch (TaskCanceledException exception)
        {
            throw new TabletException(TabletConnectionError.UsbNotConnected, "reMarkable is not connected via USB.", exception);
        }
    }

    private void UpdateItems(Item parentItem, IEnumerable<Item> allItems)
    {
        IEnumerable<Item> children = allItems.Where(item => item.ParentCollectionId == parentItem.Id);
        foreach (Item child in children)
        {
            child.Trashed = parentItem.Trashed;
            parentItem.Collection?.Add(child);

            this.UpdateItems(child, allItems);
        }
    }

    private struct ContentFile
    {
        public String FileType { get; set; }
        public Int32 FormatVersion { get; set; }
        public PagesContainer CPages { get; set; }

        public struct PagesContainer
        {
            public Collection<Page> Pages { get; set; }

            public struct Page
            {
                public Object? Deleted { get; set; }
                public String Id { get; set; }
            }
        }
    }

    private struct MetaDataFile
    {
        public Boolean? Deleted { get; set; }
        public String LastModified { get; set; }
        public String Parent { get; set; }
        public String Type { get; set; }
        public String VisibleName { get; set; }
    }

    internal sealed class Item
    {
        public Item(String id, String lastModified, String parent, String type, String visibleName)
        {
            this.Collection = (type == "CollectionType") ? new Collection<Item>() : null;
            this.Id = id;
            this.Modified = DateTime.UnixEpoch.AddMilliseconds(Double.Parse(lastModified, CultureInfo.InvariantCulture));
            this.Name = visibleName;
            this.ParentCollectionId = parent;
            this.Trashed = parent == "trash";
        }

        public Collection<Item>? Collection { get; }
        public String Id { get; }
        public DateTime Modified { get; }
        public String Name { get; }
        public String ParentCollectionId { get; }
        public Boolean Trashed { get; set; }
    }
}
