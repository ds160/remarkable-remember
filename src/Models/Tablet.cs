using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    private const String PATH_TEMPLATES = "/usr/share/remarkable/templates/";
    private const String PATH_TEMPLATES_FILE = "templates.json";
    private const Int32 SSH_TIMEOUT = 2;
    private const String SSH_USER = "root";
    private const Int32 USB_TIMEOUT = 1;

    private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

    private readonly Settings settings;
    private readonly SemaphoreSlim sshSemaphore;
    private readonly HttpClient usbClientDocument;
    private readonly HttpClient usbClientDownload;
    private readonly SemaphoreSlim usbSemaphore;

    public Tablet(Settings settings)
    {
        this.settings = settings;
        this.sshSemaphore = new SemaphoreSlim(1, 1);
        this.usbClientDocument = new HttpClient() { Timeout = TimeSpan.FromSeconds(USB_TIMEOUT) };
        this.usbClientDownload = new HttpClient();
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

    public async Task Backup(String id, String targetDirectory)
    {
        await this.sshSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            using SftpClient client = await this.CreateSftpClient().ConfigureAwait(false);

            await this.BackupFiles(client, PATH_NOTEBOOKS, targetDirectory, file => file.Name.StartsWith(id, StringComparison.OrdinalIgnoreCase)).ConfigureAwait(false);
        }
        finally
        {
            this.sshSemaphore.Release();
        }
    }

    public async Task<Stream> Download(String id)
    {
        await this.usbSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            return await ExecuteHttp(() => this.usbClientDownload.GetStreamAsync(new Uri($"http://{IP}/download/{id}/placeholder"))).ConfigureAwait(false);
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
            using SftpClient client = await this.CreateSftpClient().ConfigureAwait(false);
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
            using SftpClient client = await this.CreateSftpClient().ConfigureAwait(false);

            IEnumerable<ISftpFile> files = await Task.Run(() => client.ListDirectory(PATH_NOTEBOOKS)).ConfigureAwait(false);

            List<Item> allItems = new List<Item>();
            foreach (ISftpFile file in files.Where(file => file.IsRegularFile && file.Name.EndsWith(".metadata", StringComparison.OrdinalIgnoreCase)))
            {
                String metaDataFileText = await Task.Run(() => client.ReadAllText(file.FullName)).ConfigureAwait(false);
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
            using SftpClient client = await this.CreateSftpClient().ConfigureAwait(false);

            String contentFileText = await Task.Run(() => client.ReadAllText(Path.Combine(PATH_NOTEBOOKS, $"{id}.content"))).ConfigureAwait(false);
            ContentFile contentFile = JsonSerializer.Deserialize<ContentFile>(contentFileText, jsonSerializerOptions);

            if (contentFile.FileType != "notebook") { throw new NotebookException("Invalid reMarkable file type."); }
            if (contentFile.FormatVersion is not (1 or 2)) { throw new NotebookException($"Invalid reMarkable file format version: '{contentFile.FormatVersion}'."); }

            List<Byte[]> pageBuffers = new List<Byte[]>();
            IEnumerable<String> pages = (contentFile.FormatVersion == 1) ? contentFile.Pages : contentFile.CPages.Pages.Where(page => page.Deleted == null).Select(page => page.Id);
            foreach (String page in pages)
            {
                Byte[] pageBuffer = await Task.Run(() => client.ReadAllBytes(Path.Combine(PATH_NOTEBOOKS, id, $"{page}.rm"))).ConfigureAwait(false);
                pageBuffers.Add(pageBuffer);
            }
            return new Notebook(pageBuffers);
        }
        finally
        {
            this.sshSemaphore.Release();
        }
    }

    public async Task UploadTemplate(TabletTemplate template)
    {
        await this.sshSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            using SftpClient client = await this.CreateSftpClient().ConfigureAwait(false);

            String templatesFilePath = Path.Combine(PATH_TEMPLATES, PATH_TEMPLATES_FILE);
            String templatesFileText = await Task.Run(() => client.ReadAllText(templatesFilePath)).ConfigureAwait(false);
            TemplatesFile templatesFile = JsonSerializer.Deserialize<TemplatesFile>(templatesFileText, jsonSerializerOptions);

            Int32 currentIndex = templatesFile.Templates.FindIndex((item) => String.CompareOrdinal(item.Name, template.Name) == 0);
            if (currentIndex > -1)
            {
                templatesFile.Templates[currentIndex] = TemplatesFile.Template.Convert(template);
            }
            else
            {
                templatesFile.Templates.Add(TemplatesFile.Template.Convert(template));
            }

            await Task.Run(() => client.WriteAllBytes(Path.Combine(PATH_TEMPLATES, $"{template.FileName}.png"), template.BytesPng)).ConfigureAwait(false);
            await Task.Run(() => client.WriteAllBytes(Path.Combine(PATH_TEMPLATES, $"{template.FileName}.svg"), template.BytesSvg)).ConfigureAwait(false);
            await Task.Run(() => client.WriteAllText(templatesFilePath, JsonSerializer.Serialize(templatesFile, jsonSerializerOptions))).ConfigureAwait(false);

            await this.Restart().ConfigureAwait(false);
        }
        finally
        {
            this.sshSemaphore.Release();
        }
    }

    private async Task BackupFiles(SftpClient client, String sourceDirectory, String targetDirectory, Func<ISftpFile, Boolean> filter)
    {
        IEnumerable<ISftpFile> files = await Task.Run(() => client.ListDirectory(sourceDirectory)).ConfigureAwait(false);
        foreach (ISftpFile file in files.Where(filter))
        {
            String targetPath = Path.Combine(targetDirectory, file.Name);

            if (file.IsDirectory)
            {
                await this.BackupFiles(client, file.FullName, targetPath, file => file.Name is not "." and not "..").ConfigureAwait(false);
            }

            if (file.IsRegularFile)
            {
                using Stream fileStream = FileSystem.Create(targetPath);
                await Task.Run(() => client.DownloadFile(file.FullName, fileStream)).ConfigureAwait(false);
            }
        }
    }

    private async Task<SftpClient> CreateSftpClient()
    {
        SftpClient client = new SftpClient(this.CreateSshConnectionInfo());
        await ConnectClient(client).ConfigureAwait(false);
        return client;
    }

    private ConnectionInfo CreateSshConnectionInfo()
    {
        AuthenticationMethod authenticationMethod = new PasswordAuthenticationMethod(SSH_USER, this.settings.TabletPassword ?? "");
        return new ConnectionInfo(this.settings.TabletIp ?? IP, SSH_USER, authenticationMethod) { Timeout = TimeSpan.FromSeconds(SSH_TIMEOUT) };
    }

    private static async Task ConnectClient(BaseClient client)
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
            throw new TabletException(exception.Message, exception);
        }
        catch (SshAuthenticationException exception)
        {
            throw new TabletException(TabletConnectionError.SshNotConfigured, "SSH protocol information are not configured or wrong.", exception);
        }
        catch (SshOperationTimeoutException exception)
        {
            throw new TabletException(TabletConnectionError.SshNotConnected, "reMarkable is not connected via WiFi or USB.", exception);
        }
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
                    throw new TabletException(TabletConnectionError.UsbNotActived, "USB web interface is not activated.", exception);
                }
            }

            throw new TabletException(exception.Message, exception);
        }
        catch (TaskCanceledException exception)
        {
            throw new TabletException(TabletConnectionError.UsbNotConnected, "reMarkable is not connected via USB.", exception);
        }
    }

    private async Task Restart()
    {
        using SshClient client = new SshClient(this.CreateSshConnectionInfo());
        await ConnectClient(client).ConfigureAwait(false);
        await Task.Run(() => client.RunCommand("systemctl restart xochitl")).ConfigureAwait(false);
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
        public IEnumerable<String> Pages { get; set; }

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
            this.Name = (type == "DocumentType") ? $"{visibleName}.pdf" : visibleName;
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

    private struct TemplatesFile
    {
        public List<Template> Templates { get; set; }

        public struct Template
        {
            public IEnumerable<String> Categories { get; set; }
            public String Filename { get; set; }
            public String IconCode { get; set; }
            public Boolean? Landscape { get; set; }
            public String Name { get; set; }

            public static Template Convert(TabletTemplate template)
            {
                return new Template()
                {
                    Categories = new List<String>() { template.Category },
                    Filename = template.FileName,
                    IconCode = template.IconCode,
                    Landscape = template.Landscape,
                    Name = template.Name
                };
            }
        }
    }
}
