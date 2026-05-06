using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ReMarkableRemember.Common.FileSystem;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Common.Notebook;
using ReMarkableRemember.Services.ConfigurationService;
using ReMarkableRemember.Services.ConfigurationService.Service;
using ReMarkableRemember.Services.TabletService.Communication;
using ReMarkableRemember.Services.TabletService.Configuration;
using ReMarkableRemember.Services.TabletService.Exceptions;
using ReMarkableRemember.Services.TabletService.Files;
using ReMarkableRemember.Services.TabletService.Models;
using Renci.SshNet.Sftp;

namespace ReMarkableRemember.Services.TabletService;

public sealed partial class TabletService : ServiceBase<TabletConfiguration>, ITabletService
{
    private const String PATH_NOTEBOOKS = "/home/root/.local/share/remarkable/xochitl/";
    private const String PATH_OS_RELEASE = "/usr/lib/os-release";
    private const String PATH_TEMPLATES = "/usr/share/remarkable/templates/";
    private const String PATH_TEMPLATES_FILE = "templates.json";
    private const String PATH_VERSION_INFORMATION_FILE = "/proc/version";

    private const String VERSION_INFORMATION_RM1 = "-rm10x";
    private const String VERSION_INFORMATION_RM2 = "-rm11x";
    private const String VERSION_INFORMATION_RMPP = "imx8mm-ferrari";
    private const String VERSION_INFORMATION_RMPP_MOVE = "imx93-chiappa";

    private readonly CommunicationManager communicationManager;

    public TabletService(IConfigurationService configurationService)
        : base(configurationService)
    {
        this.communicationManager = new CommunicationManager(this.Configuration);
    }

    ITabletConfiguration ITabletService.Configuration
    {
        get { return this.Configuration; }
    }

    public async Task Backup(String id)
    {
        using SshCommunication ssh = await this.communicationManager.Ssh().ConfigureAwait(false);

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

        await BackupFiles(ssh, PATH_NOTEBOOKS, targetDirectory, file => file.Name.StartsWith(id, StringComparison.Ordinal)).ConfigureAwait(false);
    }

    public async Task DeleteTemplate(TabletTemplate tabletTemplate)
    {
        using SshCommunication ssh = await this.communicationManager.Ssh().ConfigureAwait(false);

        String templatesFilePath = $"{PATH_TEMPLATES}{PATH_TEMPLATES_FILE}";
        String templatesFileText = await ssh.FileReadText(templatesFilePath).ConfigureAwait(false);
        TemplatesFile templatesFile = JsonFile.Deserialize<TemplatesFile>(templatesFileText);

        Int32 index = templatesFile.Templates.FindIndex((item) => String.Equals(item.Filename, tabletTemplate.FileName, StringComparison.Ordinal));
        if (index > -1)
        {
            templatesFile.Templates.RemoveAt(index);
        }

        await ssh.FileDelete($"{PATH_TEMPLATES}{tabletTemplate.FileName}.png").ConfigureAwait(false);
        await ssh.FileDelete($"{PATH_TEMPLATES}{tabletTemplate.FileName}.svg").ConfigureAwait(false);
        await ssh.FileWrite(templatesFilePath, JsonFile.Serialize(templatesFile)).ConfigureAwait(false);
    }

    public async Task Download(String id, String targetPath)
    {
        using UsbCommunication usb = await this.communicationManager.Usb().ConfigureAwait(false);

        using Stream sourceStream = await usb.Download(id).ConfigureAwait(false);
        using Stream targetStream = FileSystem.Create(targetPath);
        await sourceStream.CopyToAsync(targetStream).ConfigureAwait(false);
    }

    public async Task<TabletConnectionStatus> GetConnectionStatus()
    {
        TabletInformation? information = null;

        try
        {
            using SshCommunication ssh = await this.communicationManager.Ssh().ConfigureAwait(false);
            information = await GetInformation(ssh).ConfigureAwait(false);
        }
        catch (TabletException exception)
        {
            return new TabletConnectionStatus(information, exception.Error);
        }

        try
        {
            using UsbCommunication usb = await this.communicationManager.Usb().ConfigureAwait(false);
            await usb.CheckConnection().ConfigureAwait(false);
        }
        catch (TabletException exception)
        {
            return new TabletConnectionStatus(information, exception.Error);
        }

        return new TabletConnectionStatus(information, null);
    }

    public async Task<TabletItems> GetItems()
    {
        using SshCommunication ssh = await this.communicationManager.Ssh().ConfigureAwait(false);

        List<TabletItem> allItems = new List<TabletItem>();
        Dictionary<String, Exception> notReadable = new Dictionary<String, Exception>();
        IAsyncEnumerable<ISftpFile> files = ssh.ListDirectory(PATH_NOTEBOOKS);
        await foreach (ISftpFile file in files.ConfigureAwait(false))
        {
            if (file.IsRegularFile && file.Name.EndsWith(".metadata", StringComparison.Ordinal))
            {
                try
                {
                    String metaDataFileText = await ssh.FileReadText(file.FullName).ConfigureAwait(false);
                    MetaDataFile metaDataFile = JsonFile.Deserialize<MetaDataFile>(metaDataFileText);
                    if (metaDataFile.Deleted != true)
                    {
                        String id = Path.GetFileNameWithoutExtension(file.Name);
                        allItems.Add(new TabletItem(id, metaDataFile.LastModified, metaDataFile.Parent, metaDataFile.Type, metaDataFile.VisibleName));
                    }
                }
                catch (Exception exception)
                {
                    notReadable.Add(file.FullName, exception);
                }
            }
        }

        IEnumerable<TabletItem> items = allItems.Where(item => String.IsNullOrEmpty(item.ParentCollectionId) || item.Trashed);
        foreach (TabletItem item in items) { UpdateItems(item, allItems); }
        return new TabletItems(items, notReadable);
    }

    public async Task<Notebook> GetNotebook(String id)
    {
        using SshCommunication ssh = await this.communicationManager.Ssh().ConfigureAwait(false);

        String contentFileText = await ssh.FileReadText($"{PATH_NOTEBOOKS}{id}.content").ConfigureAwait(false);
        ContentFile contentFile = JsonFile.Deserialize<ContentFile>(contentFileText);

        if (contentFile.FileType != "notebook") { throw new TabletException(Language.Current.TabletFileTypeInvalid(contentFile.FileType)); }
        if (contentFile.FormatVersion is not (1 or 2)) { throw new TabletException(Language.Current.TabletFileFormatVersionInvalid(contentFile.FormatVersion)); }

        List<Byte[]> pageBuffers = new List<Byte[]>();
        IEnumerable<String> pages = contentFile.CPages?.Pages.Where(page => page.Deleted == null).Select(page => page.Id) ?? contentFile.Pages ?? [];
        foreach (String page in pages)
        {
            Byte[] pageBuffer = await ssh.FileReadBytes($"{PATH_NOTEBOOKS}{id}/{page}.rm").ConfigureAwait(false);
            pageBuffers.Add(pageBuffer);
        }

        TabletInformation information = await GetInformation(ssh).ConfigureAwait(false);
        return Notebook.Parse(pageBuffers, information.Resolution);
    }

    public async Task InstallLamyEraser(Boolean press, Boolean undo, Boolean leftHanded)
    {
        using GitHubCommunication gitHub = await this.communicationManager.GitHub().ConfigureAwait(false);
        using SshCommunication ssh = await this.communicationManager.Ssh().ConfigureAwait(false);

        TabletInformation information = await GetInformation(ssh).ConfigureAwait(false);
        if (!information.LamyEraserSupport) { throw new TabletException(TabletError.NotSupported, Language.Current.TabletLamyEraserNotSupported(information.Type.GetDisplayText())); }

        await ssh.Execute("systemctl disable --now LamyEraser.service", false).ConfigureAwait(false);

        String serviceText = await gitHub.GetText(new Uri("https://raw.githubusercontent.com/isaacwisdom/RemarkableLamyEraser/v1/RemarkableLamyEraser/LamyEraser.service")).ConfigureAwait(false);
        serviceText = InstallLamyEraserOptions(serviceText, press, undo, leftHanded);
        await ssh.FileWrite("/lib/systemd/system/LamyEraser.service", serviceText).ConfigureAwait(false);

        Byte[] serviceBytes = await gitHub.GetBytes(new Uri("https://raw.githubusercontent.com/isaacwisdom/RemarkableLamyEraser/v1/RemarkableLamyEraser/RemarkableLamyEraser")).ConfigureAwait(false);
        await ssh.FileWrite("/usr/sbin/RemarkableLamyEraser", serviceBytes).ConfigureAwait(false);

        await ssh.Execute("chmod +x /usr/sbin/RemarkableLamyEraser").ConfigureAwait(false);
        await ssh.Execute("systemctl daemon-reload").ConfigureAwait(false);
        await ssh.Execute("systemctl enable --now LamyEraser.service").ConfigureAwait(false);
    }

    public async Task Restart()
    {
        using SshCommunication ssh = await this.communicationManager.Ssh().ConfigureAwait(false);
        await ssh.Execute("systemctl restart xochitl").ConfigureAwait(false);
    }

    public async Task UploadFile(String path, String? parentId)
    {
        using UsbCommunication usb = await this.communicationManager.Usb().ConfigureAwait(false);

        FileInfo file = new FileInfo(path);
        String fileName = Encoding.GetEncoding("ISO-8859-1").GetString(Encoding.UTF8.GetBytes(file.Name));
        String mediaType = UploadFileCheck(file);

        using StreamContent fileContent = new StreamContent(File.OpenRead(path));
        fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
        fileContent.Headers.ContentDisposition.Parameters.Add(new NameValueHeaderValue("name", "\"file\""));
        fileContent.Headers.ContentDisposition.Parameters.Add(new NameValueHeaderValue("filename", $"\"{fileName}\""));
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);

        using MultipartFormDataContent multipartContent = new MultipartFormDataContent() { { fileContent } };
        await usb.Upload(parentId, multipartContent);
    }

    public async Task UploadTemplate(TabletTemplate tabletTemplate)
    {
        using SshCommunication ssh = await this.communicationManager.Ssh().ConfigureAwait(false);

        String templatesFilePath = $"{PATH_TEMPLATES}{PATH_TEMPLATES_FILE}";
        String templatesFileText = await ssh.FileReadText(templatesFilePath).ConfigureAwait(false);
        TemplatesFile templatesFile = JsonFile.Deserialize<TemplatesFile>(templatesFileText);

        Int32 index = templatesFile.Templates.FindIndex((item) => String.Equals(item.Filename, tabletTemplate.FileName, StringComparison.Ordinal));
        if (index > -1)
        {
            templatesFile.Templates[index] = TemplatesFile.Template.Convert(tabletTemplate);
        }
        else
        {
            templatesFile.Templates.Add(TemplatesFile.Template.Convert(tabletTemplate));
        }

        await ssh.FileWrite($"{PATH_TEMPLATES}{tabletTemplate.FileName}.png", tabletTemplate.BytesPng, false).ConfigureAwait(false);
        await ssh.FileWrite($"{PATH_TEMPLATES}{tabletTemplate.FileName}.svg", tabletTemplate.BytesSvg, false).ConfigureAwait(false);
        await ssh.FileWrite(templatesFilePath, JsonFile.Serialize(templatesFile)).ConfigureAwait(false);
    }

    private static async Task BackupFiles(SshCommunication ssh, String sourceDirectory, String targetDirectory, Func<ISftpFile, Boolean> filter)
    {
        IAsyncEnumerable<ISftpFile> files = ssh.ListDirectory(sourceDirectory);
        await foreach (ISftpFile file in files.ConfigureAwait(false))
        {
            if (!filter(file)) { continue; }

            String targetPath = Path.Combine(targetDirectory, file.Name);

            if (file.IsDirectory)
            {
                await BackupFiles(ssh, file.FullName, targetPath, file => file.Name is not "." and not "..").ConfigureAwait(false);
            }

            if (file.IsRegularFile)
            {
                await ssh.FileDownload(file.FullName, targetPath).ConfigureAwait(false);
            }
        }
    }

    private static async Task<TabletInformation> GetInformation(SshCommunication ssh)
    {
        TabletType type = await GetTabletType(ssh).ConfigureAwait(false);
        Version softwareVersion = await GetSoftwareVersion(ssh).ConfigureAwait(false);

        return new TabletInformation(type, softwareVersion);
    }

    private static async Task<Version> GetSoftwareVersion(SshCommunication ssh)
    {
        String osReleaseInformation = await ssh.FileReadText(PATH_OS_RELEASE).ConfigureAwait(false);
        Match match = GetSoftwareVersionRegex().Match(osReleaseInformation);
        if (match.Success)
        {
            return new Version(match.Groups[1].Value);
        }
        else
        {
            throw new TabletException(TabletError.NotSupported, Language.Current.TabletSoftwareVersionUnknown);
        }
    }

    [GeneratedRegex("IMG_VERSION=\"(\\d+\\.\\d+\\.\\d+.\\d+)\"")]
    private static partial Regex GetSoftwareVersionRegex();

    private static async Task<TabletType> GetTabletType(SshCommunication ssh)
    {
        String versionInformation = await ssh.FileReadText(PATH_VERSION_INFORMATION_FILE).ConfigureAwait(false);

        if (versionInformation.Contains(VERSION_INFORMATION_RM1)) { return TabletType.rM1; }
        if (versionInformation.Contains(VERSION_INFORMATION_RM2)) { return TabletType.rM2; }
        if (versionInformation.Contains(VERSION_INFORMATION_RMPP)) { return TabletType.rMPaperPro; }
        if (versionInformation.Contains(VERSION_INFORMATION_RMPP_MOVE)) { return TabletType.rMPaperProMove; }

        throw new TabletException(TabletError.NotSupported, Language.Current.TabletNotSupported);
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
        if (file.Length >= 100 * 1024 * 1024) { throw new TabletException(Language.Current.TabletFileTooLarge); }

        return file.Extension.ToUpperInvariant() switch
        {
            ".PDF" => "application/pdf",
            ".EPUB" => "application/epub+zip",
            _ => throw new TabletException(Language.Current.TabletFileTypeNotSupported(file.Extension)),
        };
    }

    void IDisposable.Dispose()
    {
        this.communicationManager.Dispose();
    }
}
