using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReMarkableRemember.Entities;

namespace ReMarkableRemember.Models;

internal sealed class Controller : IDisposable
{
    private const String PATH_BACKUP = "/home/daniel/SynologyDrive/Remarkable/Backups";

    private readonly String dataSource;
    private readonly MyScript myScript;
    private readonly Tablet tablet;

    public Controller(String dataSource)
    {
        this.dataSource = dataSource;

        using DatabaseContext database = new DatabaseContext(this.dataSource);
        database.Database.Migrate();

        String? myScriptApplicationKey = database.Settings.Find(Setting.Keys.MyScriptApplicationKey)?.Value;
        String? myScriptHmacKey = database.Settings.Find(Setting.Keys.MyScriptHmacKey)?.Value;
        this.myScript = new MyScript(myScriptApplicationKey, myScriptHmacKey);

        String? tabletIp = database.Settings.Find(Setting.Keys.TabletIp)?.Value;
        String? tabletPassword = database.Settings.Find(Setting.Keys.TabletPassword)?.Value;
        this.tablet = new Tablet(tabletIp, tabletPassword);
    }

    public void Dispose()
    {
        this.tablet.Dispose();

        GC.SuppressFinalize(this);
    }

    public async Task<Boolean> BackupItem(Item item)
    {
        if (item.BackupHint is Item.Hint.None) { return false; }
        if (item.Trashed) { return false; }

        IEnumerable<String> directories = Directory.GetDirectories(PATH_BACKUP, $"{item.Id}*");
        foreach (String directory in directories)
        {
            FileSystem.Delete(directory);
        }

        IEnumerable<String> files = Directory.GetFiles(PATH_BACKUP).Where(file => file.StartsWith(Path.Combine(PATH_BACKUP, item.Id), StringComparison.OrdinalIgnoreCase));
        foreach (String file in files)
        {
            FileSystem.Delete(file);
        }

        await this.tablet.Backup(item.Id, PATH_BACKUP).ConfigureAwait(false);

        item.BackupDone();

        return true;
    }

    public async Task<TabletConnectionError?> GetConnectionStatus()
    {
        return await this.tablet.GetConnectionStatus().ConfigureAwait(false);
    }

    public async Task<IEnumerable<Item>> GetItems()
    {
        using DatabaseContext database = new DatabaseContext(this.dataSource);
        IEnumerable<Tablet.Item> tabletItems = await this.tablet.GetItems().ConfigureAwait(false);
        return tabletItems.Select(tabletItem => this.MapItem(database, tabletItem)).ToArray();
    }

    public async Task<String> HandWritingRecognition(Item item, String language)
    {
        Notebook notebook = await this.tablet.GetNotebook(item.Id).ConfigureAwait(false);
        IEnumerable<String> myScriptPages = await Task.WhenAll(notebook.Pages.Select(page => this.myScript.Recognize(page, language))).ConfigureAwait(false);
        return String.Join(Environment.NewLine, myScriptPages);
    }

    public async Task<Boolean> SyncItem(Item item)
    {
        if (item.SyncHint is Item.Hint.None or >= Item.Hint.ExistsInTarget) { return false; }
        if (item.SyncPath == null) { return false; }
        if (item.Trashed) { return false; }

        if (item.Sync != null && item.SyncHint is Item.Hint.SyncPathChanged)
        {
            FileSystem.Delete(item.Sync.Path);
        }

        using Stream sourceStream = await this.tablet.Download(item.Id).ConfigureAwait(false);
        using Stream targetStream = FileSystem.Create(item.SyncPath);
        await sourceStream.CopyToAsync(targetStream).ConfigureAwait(false);

        item.SyncDone(item.SyncPath);

        return true;
    }

    public async Task UploadTemplate(TabletTemplate template)
    {
        await this.tablet.UploadTemplate(template).ConfigureAwait(false);
    }

    private Item MapItem(DatabaseContext database, Tablet.Item tabletItem, String? parentTargetDirectory = null)
    {
        String? targetDirectory = MapItemGetTargetDirectory(database, tabletItem, parentTargetDirectory);
        IEnumerable<Item>? collection = tabletItem.Collection?.Select(childTabletItem => this.MapItem(database, childTabletItem, targetDirectory)).ToArray();
        String? syncPath = (targetDirectory != null && collection == null) ? Path.Combine(targetDirectory, tabletItem.Name) : targetDirectory;

        return new Item(this.dataSource, tabletItem, collection, syncPath);
    }

    private static String? MapItemGetTargetDirectory(DatabaseContext database, Tablet.Item tabletItem, String? parentTargetDirectory)
    {
        SyncConfiguration? syncConfiguration = database.SyncConfigurations.Find(tabletItem.Id);
        if (syncConfiguration != null) { return syncConfiguration.TargetDirectory; }

        return (parentTargetDirectory != null && tabletItem.Collection != null) ? Path.Combine(parentTargetDirectory, tabletItem.Name) : parentTargetDirectory;
    }
}
