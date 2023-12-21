using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReMarkableRemember.Entities;
using ReMarkableRemember.Helper;

namespace ReMarkableRemember.Models;

internal sealed class Controller : IDisposable
{
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

    public async Task<TabletConnectionError?> GetConnectionStatus()
    {
        return await this.tablet.GetConnectionStatus().ConfigureAwait(false);
    }

    public async Task<IEnumerable<Item>> GetItems()
    {
        using DatabaseContext database = new DatabaseContext(this.dataSource);
        IEnumerable<Tablet.Item> tabletItems = await this.tablet.GetItems().ConfigureAwait(false);
        return tabletItems.Select(tabletItem => MapItem(database, tabletItem)).ToArray();
    }

    public async Task<String> HandWritingRecognition(Item item, String language)
    {
        Notebook notebook = await this.tablet.GetNotebook(item.Id).ConfigureAwait(false);
        IEnumerable<String> myScriptPages = await Task.WhenAll(notebook.Pages.Select(page => this.myScript.Recognize(page, language))).ConfigureAwait(false);
        return String.Join(Environment.NewLine, myScriptPages);
    }

    public async Task ProcessItem(Item item)
    {
        if (item.Collection != null)
        {
            await Task.WhenAll(item.Collection.Select(this.ProcessItem)).ConfigureAwait(false);
        }

        String? syncPath = await this.ItemSync(item).ConfigureAwait(false);

        this.ItemSave(item.Id, item.Modified, syncPath);
    }

    private void ItemSave(String id, DateTime modified, String? syncPath)
    {
        using DatabaseContext database = new DatabaseContext(this.dataSource);

        if (syncPath != null)
        {
            Sync? sync = database.Syncs.Find(id);
            if (sync != null)
            {
                sync.Modified = modified;
                sync.Downloaded = syncPath;
            }
            else
            {
                database.Syncs.Add(new Sync(id, modified, syncPath));
            }
        }

        database.SaveChanges();
    }

    private async Task<String?> ItemSync(Item item)
    {
        if (item.SyncHint == null) { return null; }
        if (item.SyncHint is Item.Hint.DocumentExistsInTarget or Item.Hint.ItemTrashed) { return null; }
        if (item.SyncPath == null) { return null; }

        if (item.Sync != null && item.SyncHint is Item.Hint.DocumentDownloadPathChanged)
        {
            FileHelper.Delete(item.Sync.Path);
        }

        using Stream sourceStream = await this.tablet.Download(item.Id).ConfigureAwait(false);
        using Stream targetStream = FileHelper.Create(item.SyncPath);
        await sourceStream.CopyToAsync(targetStream).ConfigureAwait(false);

        return item.SyncPath;
    }

    private static Item MapItem(DatabaseContext database, Tablet.Item tabletItem, String? parentTargetDirectory = null)
    {
        String? targetDirectory = MapItemGetTargetDirectory(database, tabletItem, parentTargetDirectory);
        IEnumerable<Item>? collection = tabletItem.Collection?.Select(childTabletItem => MapItem(database, childTabletItem, targetDirectory)).ToArray();
        String? syncPath = (targetDirectory != null && collection == null) ? Path.Combine(targetDirectory, tabletItem.Name) : targetDirectory;

        Backup? previousBackup = database.Backups.Find(tabletItem.Id);
        Sync? previousSync = database.Syncs.Find(tabletItem.Id);

        return new Item(tabletItem, collection, previousBackup, previousSync, syncPath);
    }

    private static String? MapItemGetTargetDirectory(DatabaseContext database, Tablet.Item tabletItem, String? parentTargetDirectory)
    {
        SyncConfiguration? syncConfiguration = database.SyncConfigurations.Find(tabletItem.Id);
        if (syncConfiguration != null) { return syncConfiguration.TargetDirectory; }

        return (parentTargetDirectory != null && tabletItem.Collection != null) ? Path.Combine(parentTargetDirectory, tabletItem.Name) : parentTargetDirectory;
    }
}
