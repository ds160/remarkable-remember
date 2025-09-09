using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ReMarkableRemember.Common.FileSystem;
using ReMarkableRemember.Common.Notebook;
using ReMarkableRemember.Services.ConfigurationService;
using ReMarkableRemember.Services.ConfigurationService.Service;
using ReMarkableRemember.Services.TabletService.Configuration;
using ReMarkableRemember.Services.TabletService.Exceptions;
using ReMarkableRemember.Services.TabletService.Models;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace ReMarkableRemember.Services.TabletService;

public sealed partial class TabletService : ServiceBase<TabletConfiguration>, ITabletService
{
    private const String IP = "10.11.99.1";
    private const String PATH_NOTEBOOKS = "/home/root/.local/share/remarkable/xochitl/";
    private const String PATH_OS_RELEASE = "/usr/lib/os-release";
    private const String PATH_TEMPLATES = "/usr/share/remarkable/templates/";
    private const String PATH_TEMPLATES_FILE = "templates.json";
    private const String PATH_VERSION_INFORMATION_FILE = "/proc/version";
    private const String SOFTWARE_VERSION_REGEX = "IMG_VERSION=\"(\\d+\\.\\d+\\.\\d+.\\d+)\"";
    private const Int32 SSH_TIMEOUT = 2;
    private const String SSH_USER = "root";
    private const Int32 USB_TIMEOUT = 1;
    private const String VERSION_INFORMATION_RM1 = "-rm10x";
    private const String VERSION_INFORMATION_RM2 = "-rm11x";
    private const String VERSION_INFORMATION_RMPP = "imx8mm-ferrari";
    private const String VERSION_INFORMATION_RMPP_MOVE = "imx93-chiappa";

    private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

    private readonly HttpClient gitHubClient;
    private readonly SemaphoreSlim sshSemaphore;
    private readonly HttpClient usbClient;
    private readonly HttpClient usbClientConnection;
    private readonly SemaphoreSlim usbSemaphore;

    public TabletService(IConfigurationService configurationService)
        : base(configurationService)
    {
        this.gitHubClient = new HttpClient();
        this.sshSemaphore = new SemaphoreSlim(1, 1);
        this.usbClient = new HttpClient();
        this.usbClientConnection = new HttpClient() { Timeout = TimeSpan.FromSeconds(USB_TIMEOUT) };
        this.usbSemaphore = new SemaphoreSlim(1, 1);
    }

    ITabletConfiguration ITabletService.Configuration
    {
        get { return this.Configuration; }
    }

    public async Task Backup(String id)
    {
        await this.sshSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            String targetDirectory = this.Configuration.Backup;
            if (!Path.Exists(targetDirectory)) { return; }

            IEnumerable<String> directories = Directory.GetDirectories(targetDirectory, $"{id}*");
            foreach (String directory in directories)
            {
                FileSystem.Delete(directory);
            }

            IEnumerable<String> files = Directory.GetFiles(targetDirectory).Where(file => file.StartsWith(Path.Combine(targetDirectory, id), StringComparison.Ordinal));
            foreach (String file in files)
            {
                FileSystem.Delete(file);
            }

            using SftpClient client = await this.CreateSftpClient().ConfigureAwait(false);
            await BackupFiles(client, PATH_NOTEBOOKS, targetDirectory, file => file.Name.StartsWith(id, StringComparison.Ordinal)).ConfigureAwait(false);
        }
        finally
        {
            this.sshSemaphore.Release();
        }
    }

    public async Task DeleteTemplate(TabletTemplate tabletTemplate)
    {
        await this.sshSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            using SftpClient client = await this.CreateSftpClient().ConfigureAwait(false);

            String templatesFilePath = $"{PATH_TEMPLATES}{PATH_TEMPLATES_FILE}";
            String templatesFileText = await Task.Run(() => client.ReadAllText(templatesFilePath)).ConfigureAwait(false);
            TemplatesFile templatesFile = JsonSerializer.Deserialize<TemplatesFile>(templatesFileText, jsonSerializerOptions);

            Int32 index = templatesFile.Templates.FindIndex((item) => String.CompareOrdinal(item.Filename, tabletTemplate.FileName) == 0);
            if (index > -1)
            {
                templatesFile.Templates.RemoveAt(index);
            }

            await FileDelete(client, $"{PATH_TEMPLATES}{tabletTemplate.FileName}.png").ConfigureAwait(false);
            await FileDelete(client, $"{PATH_TEMPLATES}{tabletTemplate.FileName}.svg").ConfigureAwait(false);
            await FileWrite(client, templatesFilePath, JsonSerializer.Serialize(templatesFile, jsonSerializerOptions)).ConfigureAwait(false);
        }
        finally
        {
            this.sshSemaphore.Release();
        }
    }

    public void Dispose()
    {
        this.gitHubClient.Dispose();
        this.sshSemaphore.Dispose();
        this.usbClient.Dispose();
        this.usbClientConnection.Dispose();
        this.usbSemaphore.Dispose();

        GC.SuppressFinalize(this);
    }

    public async Task Download(String id, String targetPath)
    {
        await this.usbSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            using Stream sourceStream = await ExecuteHttp(() => this.usbClient.GetStreamAsync(new Uri($"http://{IP}/download/{id}/placeholder"))).ConfigureAwait(false);
            using Stream targetStream = FileSystem.Create(targetPath);
            await sourceStream.CopyToAsync(targetStream).ConfigureAwait(false);
        }
        finally
        {
            this.usbSemaphore.Release();
        }
    }

    public async Task<TabletConnectionStatus> GetConnectionStatus()
    {
        await this.sshSemaphore.WaitAsync().ConfigureAwait(false);

        TabletType? type = null;

        try
        {
            (SftpClient SftpClient, TabletType Type) tablet = await this.CreateSftpClientWithType().ConfigureAwait(false);
            using SftpClient client = tablet.SftpClient;

            type = tablet.Type;
        }
        catch (TabletException exception)
        {
            return new TabletConnectionStatus(type, exception.Error);
        }
        finally
        {
            this.sshSemaphore.Release();
        }

        await this.usbSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            await ExecuteHttp(() => this.usbClientConnection.GetStringAsync(new Uri($"http://{IP}/documents/"))).ConfigureAwait(false);
        }
        catch (TabletException exception)
        {
            return new TabletConnectionStatus(type, exception.Error);
        }
        finally
        {
            this.usbSemaphore.Release();
        }

        return new TabletConnectionStatus(type, null);
    }

    public async Task<IEnumerable<TabletItem>> GetItems()
    {
        await this.sshSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            using SftpClient client = await this.CreateSftpClient().ConfigureAwait(false);

            IEnumerable<ISftpFile> files = await Task.Run(() => client.ListDirectory(PATH_NOTEBOOKS)).ConfigureAwait(false);

            List<TabletItem> allItems = new List<TabletItem>();
            foreach (ISftpFile file in files.Where(file => file.IsRegularFile && file.Name.EndsWith(".metadata", StringComparison.Ordinal)))
            {
                String metaDataFileText = await Task.Run(() => client.ReadAllText(file.FullName)).ConfigureAwait(false);
                MetaDataFile metaDataFile = JsonSerializer.Deserialize<MetaDataFile>(metaDataFileText, jsonSerializerOptions);
                if (metaDataFile.Deleted != true)
                {
                    String id = Path.GetFileNameWithoutExtension(file.Name);
                    allItems.Add(new TabletItem(id, metaDataFile.LastModified, metaDataFile.Parent, metaDataFile.Type, metaDataFile.VisibleName));
                }
            }

            IEnumerable<TabletItem> items = allItems.Where(item => String.IsNullOrEmpty(item.ParentCollectionId) || item.Trashed);
            foreach (TabletItem item in items) { UpdateItems(item, allItems); }
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
            (SftpClient SftpClient, TabletType Type) tablet = await this.CreateSftpClientWithType().ConfigureAwait(false);
            using SftpClient client = tablet.SftpClient;

            String contentFileText = await Task.Run(() => client.ReadAllText($"{PATH_NOTEBOOKS}{id}.content")).ConfigureAwait(false);
            ContentFile contentFile = JsonSerializer.Deserialize<ContentFile>(contentFileText, jsonSerializerOptions);

            if (contentFile.FileType != "notebook") { throw new TabletException("Invalid reMarkable file type."); }
            if (contentFile.FormatVersion is not (1 or 2)) { throw new TabletException($"Invalid reMarkable file format version: '{contentFile.FormatVersion}'."); }

            List<Byte[]> pageBuffers = new List<Byte[]>();
            IEnumerable<String> pages = contentFile.FormatVersion == 1 ? contentFile.Pages : contentFile.CPages.Pages.Where(page => page.Deleted == null).Select(page => page.Id);
            foreach (String page in pages)
            {
                Byte[] pageBuffer = await Task.Run(() => client.ReadAllBytes($"{PATH_NOTEBOOKS}{id}/{page}.rm")).ConfigureAwait(false);
                pageBuffers.Add(pageBuffer);
            }

            Int32 resolution = tablet.Type switch
            {
                TabletType.rM1 => 226,
                TabletType.rM2 => 226,
                TabletType.rMPaperPro => 229,
                TabletType.rMPaperProMove => 264,
                _ => throw new NotImplementedException(),
            };

            return new Notebook(pageBuffers, resolution);
        }
        finally
        {
            this.sshSemaphore.Release();
        }
    }

    public async Task InstallLamyEraser(Boolean press, Boolean undo, Boolean leftHanded)
    {
        await this.sshSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            (SftpClient SftpClient, TabletType Type) tablet = await this.CreateSftpClientWithType().ConfigureAwait(false);
            using SftpClient sftpClient = tablet.SftpClient;
            using SshClient sshClient = await this.CreateSshClient().ConfigureAwait(false);

            if (tablet.Type is TabletType.rMPaperPro or TabletType.rMPaperProMove) { throw new TabletException(TabletError.NotSupported, "Lamy Eraser is not supported on reMarkable Paper Pro and Paper Pro Move."); }

            await ExecuteSshCommand(sshClient, "systemctl disable --now LamyEraser.service", false).ConfigureAwait(false);

            String serviceText = await this.gitHubClient.GetStringAsync(new Uri("https://raw.githubusercontent.com/isaacwisdom/RemarkableLamyEraser/v1/RemarkableLamyEraser/LamyEraser.service")).ConfigureAwait(false);
            serviceText = InstallLamyEraserOptions(serviceText, press, undo, leftHanded);
            await FileWrite(sftpClient, "/lib/systemd/system/LamyEraser.service", serviceText).ConfigureAwait(false);

            Byte[] serviceBytes = await this.gitHubClient.GetByteArrayAsync(new Uri("https://raw.githubusercontent.com/isaacwisdom/RemarkableLamyEraser/v1/RemarkableLamyEraser/RemarkableLamyEraser")).ConfigureAwait(false);
            await FileWrite(sftpClient, "/usr/sbin/RemarkableLamyEraser", serviceBytes).ConfigureAwait(false);

            await ExecuteSshCommand(sshClient, "chmod +x /usr/sbin/RemarkableLamyEraser").ConfigureAwait(false);
            await ExecuteSshCommand(sshClient, "systemctl daemon-reload").ConfigureAwait(false);
            await ExecuteSshCommand(sshClient, "systemctl enable --now LamyEraser.service").ConfigureAwait(false);
        }
        finally
        {
            this.sshSemaphore.Release();
        }
    }

    public async Task InstallWebInterfaceOnBoot(String release = "v1.2.4")
    {
        await this.sshSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            using SftpClient sftpClient = await this.CreateSftpClient().ConfigureAwait(false);
            using SshClient sshClient = await this.CreateSshClient().ConfigureAwait(false);

            Version softwareVersion = await GetSoftwareVersion(sftpClient).ConfigureAwait(false);
            if (softwareVersion.Major >= 3 && softwareVersion.Minor >= 16) { throw new TabletException(TabletError.NotSupported, "WebInterface-OnBoot is currently not supported by reMarkable software version 3.16 or higher."); }

            await ExecuteSshCommand(sshClient, "systemctl disable --now webinterface-onboot.service", false).ConfigureAwait(false);

            String serviceText = await this.gitHubClient.GetStringAsync(new Uri($"https://github.com/rM-self-serve/webinterface-onboot/releases/download/{release}/webinterface-onboot.service")).ConfigureAwait(false);
            await FileWrite(sftpClient, "/lib/systemd/system/webinterface-onboot.service", serviceText).ConfigureAwait(false);

            Byte[] serviceBytes = await this.gitHubClient.GetByteArrayAsync(new Uri($"https://github.com/rM-self-serve/webinterface-onboot/releases/download/{release}/webinterface-onboot")).ConfigureAwait(false);
            await FileWrite(sftpClient, "/home/root/.local/bin/webinterface-onboot", serviceBytes).ConfigureAwait(false);

            await ExecuteSshCommand(sshClient, "chmod +x /home/root/.local/bin/webinterface-onboot").ConfigureAwait(false);

            Boolean applyHack = softwareVersion.Major >= 2 && softwareVersion.Minor >= 15;
            if (applyHack) { await ExecuteSshCommand(sshClient, "/home/root/.local/bin/webinterface-onboot apply-hack -y").ConfigureAwait(false); }

            await ExecuteSshCommand(sshClient, "systemctl enable --now webinterface-onboot.service").ConfigureAwait(false);
        }
        finally
        {
            this.sshSemaphore.Release();
        }
    }

    public async Task Restart()
    {
        await this.sshSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            using SshClient client = await this.CreateSshClient().ConfigureAwait(false);
            await ExecuteSshCommand(client, "systemctl restart xochitl").ConfigureAwait(false);
        }
        finally
        {
            this.sshSemaphore.Release();
        }
    }

    public async Task UploadFile(String path, String? parentId)
    {
        await this.usbSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            await ExecuteHttp(() => this.usbClient.GetStringAsync(new Uri($"http://{IP}/documents/{parentId}"))).ConfigureAwait(false);

            FileInfo file = new FileInfo(path);
            String fileName = Encoding.GetEncoding("ISO-8859-1").GetString(Encoding.UTF8.GetBytes(file.Name));
            String mediaType = UploadFileCheck(file);

            using StreamContent fileContent = new StreamContent(File.OpenRead(path));
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
            fileContent.Headers.ContentDisposition.Parameters.Add(new NameValueHeaderValue("name", "\"file\""));
            fileContent.Headers.ContentDisposition.Parameters.Add(new NameValueHeaderValue("filename", $"\"{fileName}\""));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);

            using MultipartFormDataContent multipartContent = new MultipartFormDataContent() { { fileContent } };

            HttpResponseMessage response = await ExecuteHttp(() => this.usbClient.PostAsync(new Uri($"http://{IP}/upload"), multipartContent)).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
        finally
        {
            this.usbSemaphore.Release();
        }
    }

    public async Task UploadTemplate(TabletTemplate tabletTemplate)
    {
        await this.sshSemaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            using SftpClient client = await this.CreateSftpClient().ConfigureAwait(false);

            String templatesFilePath = $"{PATH_TEMPLATES}{PATH_TEMPLATES_FILE}";
            String templatesFileText = await Task.Run(() => client.ReadAllText(templatesFilePath)).ConfigureAwait(false);
            TemplatesFile templatesFile = JsonSerializer.Deserialize<TemplatesFile>(templatesFileText, jsonSerializerOptions);

            Int32 index = templatesFile.Templates.FindIndex((item) => String.CompareOrdinal(item.Filename, tabletTemplate.FileName) == 0);
            if (index > -1)
            {
                templatesFile.Templates[index] = TemplatesFile.Template.Convert(tabletTemplate);
            }
            else
            {
                templatesFile.Templates.Add(TemplatesFile.Template.Convert(tabletTemplate));
            }

            await FileWrite(client, $"{PATH_TEMPLATES}{tabletTemplate.FileName}.png", tabletTemplate.BytesPng, false).ConfigureAwait(false);
            await FileWrite(client, $"{PATH_TEMPLATES}{tabletTemplate.FileName}.svg", tabletTemplate.BytesSvg, false).ConfigureAwait(false);
            await FileWrite(client, templatesFilePath, JsonSerializer.Serialize(templatesFile, jsonSerializerOptions)).ConfigureAwait(false);
        }
        finally
        {
            this.sshSemaphore.Release();
        }
    }

    private static async Task BackupFiles(SftpClient client, String sourceDirectory, String targetDirectory, Func<ISftpFile, Boolean> filter)
    {
        IEnumerable<ISftpFile> files = await Task.Run(() => client.ListDirectory(sourceDirectory)).ConfigureAwait(false);
        foreach (ISftpFile file in files.Where(filter))
        {
            String targetPath = Path.Combine(targetDirectory, file.Name);

            if (file.IsDirectory)
            {
                await BackupFiles(client, file.FullName, targetPath, file => file.Name is not "." and not "..").ConfigureAwait(false);
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
        (SftpClient SftpClient, TabletType Type) tablet = await this.CreateSftpClientWithType().ConfigureAwait(false);
        return tablet.SftpClient;
    }

    private async Task<(SftpClient, TabletType)> CreateSftpClientWithType()
    {
        SftpClient client = new SftpClient(this.CreateSshConnectionInfo());
        await ConnectClient(client).ConfigureAwait(false);
        TabletType type = await GetType(client).ConfigureAwait(false);
        return (client, type);
    }

    private async Task<SshClient> CreateSshClient()
    {
        SshClient client = new SshClient(this.CreateSshConnectionInfo());
        await ConnectClient(client).ConfigureAwait(false);
        return client;
    }

    private ConnectionInfo CreateSshConnectionInfo()
    {
        String host = String.IsNullOrEmpty(this.Configuration.IP) ? IP : this.Configuration.IP;
        AuthenticationMethod authenticationMethod = new PasswordAuthenticationMethod(SSH_USER, this.Configuration.Password);
        return new ConnectionInfo(host, SSH_USER, authenticationMethod) { Timeout = TimeSpan.FromSeconds(SSH_TIMEOUT) };
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
            if (exception.SocketErrorCode is SocketError.ConnectionRefused)
            {
                throw new TabletException(TabletError.SshNotConfigured, "SSH protocol information are not configured or wrong.", exception);
            }

            if (exception.SocketErrorCode is SocketError.HostDown or SocketError.HostUnreachable or SocketError.NetworkDown or SocketError.NetworkUnreachable)
            {
                throw new TabletException(TabletError.SshNotConnected, "reMarkable is not connected via WiFi or USB.", exception);
            }

            throw new TabletException(exception.Message, exception);
        }
        catch (SshAuthenticationException exception)
        {
            throw new TabletException(TabletError.SshNotConfigured, "SSH protocol information are not configured or wrong.", exception);
        }
        catch (SshConnectionException exception)
        {
            throw new TabletException(TabletError.SshNotConnected, "reMarkable is not connected via WiFi or USB.", exception);
        }
        catch (SshOperationTimeoutException exception)
        {
            throw new TabletException(TabletError.SshNotConnected, "reMarkable is not connected via WiFi or USB.", exception);
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
            Exception? innerException = exception.InnerException;
            while (innerException != null)
            {
                if (innerException is SocketException socketException)
                {
                    if (socketException.SocketErrorCode is SocketError.ConnectionRefused)
                    {
                        throw new TabletException(TabletError.UsbNotActived, "USB web interface is not activated.", exception);
                    }

                    if (socketException.SocketErrorCode is SocketError.HostDown or SocketError.HostUnreachable or SocketError.NetworkDown or SocketError.NetworkUnreachable)
                    {
                        throw new TabletException(TabletError.UsbNotConnected, "reMarkable is not connected via USB.", exception);
                    }
                }

                innerException = innerException.InnerException;
            }

            throw new TabletException(exception.Message, exception);
        }
        catch (TaskCanceledException exception)
        {
            throw new TabletException(TabletError.UsbNotConnected, "reMarkable is not connected via USB.", exception);
        }
    }

    private static async Task ExecuteSshCommand(SshClient client, String command, Boolean checkResult = true)
    {
        SshCommand result = await Task.Run(() => client.RunCommand(command)).ConfigureAwait(false);
        if (checkResult && result.ExitStatus != 0)
        {
            throw new TabletException(result.Error);
        }
    }

    private static async Task FileDelete(SftpClient client, String path)
    {
        await Task.Run(() => { if (client.Exists(path)) { client.DeleteFile(path); } }).ConfigureAwait(false);
    }

    private static async Task FileWrite(SftpClient client, String path, Object content, Boolean contentRequired = true)
    {
        await FileDelete(client, path).ConfigureAwait(false);

        if (content is String text)
        {
            if (text.Length > 0) { await Task.Run(() => client.WriteAllText(path, text)).ConfigureAwait(false); }
            else if (contentRequired) { throw new InvalidOperationException(); }
        }
        else if (content is Byte[] bytes)
        {
            if (bytes.Length > 0) { await Task.Run(() => client.WriteAllBytes(path, bytes)).ConfigureAwait(false); }
            else if (contentRequired) { throw new InvalidOperationException(); }
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private static async Task<Version> GetSoftwareVersion(SftpClient client)
    {
        String osReleaseInformation = await Task.Run(() => client.ReadAllText(PATH_OS_RELEASE)).ConfigureAwait(false);
        Match match = SoftwareVersionRegex().Match(osReleaseInformation);
        if (match.Success)
        {
            return new Version(match.Groups[1].Value);
        }
        else
        {
            throw new TabletException(TabletError.NotSupported, "The reMarkable software verion cannot be identified.");
        }
    }

    private static async Task<TabletType> GetType(SftpClient client)
    {
        String versionInformation = await Task.Run(() => client.ReadAllText(PATH_VERSION_INFORMATION_FILE)).ConfigureAwait(false);

        if (versionInformation.Contains(VERSION_INFORMATION_RM1)) { return TabletType.rM1; }
        if (versionInformation.Contains(VERSION_INFORMATION_RM2)) { return TabletType.rM2; }
        if (versionInformation.Contains(VERSION_INFORMATION_RMPP)) { return TabletType.rMPaperPro; }
        if (versionInformation.Contains(VERSION_INFORMATION_RMPP_MOVE)) { return TabletType.rMPaperProMove; }

        throw new TabletException(TabletError.NotSupported, "The connected reMarkable is not supported.");
    }

    private static String InstallLamyEraserOptions(String serviceText, Boolean press, Boolean undo, Boolean leftHanded)
    {
        String pressText = press ? " --press" : " --toggle";
        String undoText = undo ? " --double-press undo" : " --double-press redo";
        String leftHandedText = leftHanded ? " --left-handed" : String.Empty;

        return serviceText.Replace(
            "ExecStart=/usr/sbin/RemarkableLamyEraser",
            $"ExecStart=/usr/sbin/RemarkableLamyEraser{pressText}{undoText}{leftHandedText}",
            StringComparison.Ordinal
        );
    }

    [GeneratedRegex(SOFTWARE_VERSION_REGEX)]
    private static partial Regex SoftwareVersionRegex();

    private static void UpdateItems(TabletItem parentItem, IEnumerable<TabletItem> allItems)
    {
        IEnumerable<TabletItem> children = allItems.Where(item => item.ParentCollectionId == parentItem.Id);
        foreach (TabletItem child in children)
        {
            child.Trashed = parentItem.Trashed;
            parentItem.Collection?.Add(child);

            UpdateItems(child, allItems);
        }
    }

    private static String UploadFileCheck(FileInfo file)
    {
        if (file.Length >= 100 * 1024 * 1024) { throw new TabletException("File is to large."); }

        switch (file.Extension.ToUpperInvariant())
        {
            case ".PDF": return "application/pdf";
            case ".EPUB": return "application/epub+zip";
            default: throw new TabletException("File type is not supported.");
        }
    }

    private struct ContentFile
    {
        public PagesContainer CPages { get; set; }
        public String FileType { get; set; }
        public Int32 FormatVersion { get; set; }
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
                    Landscape = TabletTemplate.IsLandscape(template.IconCode),
                    Name = template.Name
                };
            }
        }
    }
}
