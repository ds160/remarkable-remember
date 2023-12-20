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

    public async Task SyncItem(Item item)
    {
        if (item.SyncPath == null) { return; }

        if (item.Collection != null)
        {
            await Task.WhenAll(item.Collection.Select(this.SyncItem)).ConfigureAwait(false);
        }

        if (item.SyncHint == null) { return; }
        if (item.SyncHint is Item.Hint.DocumentExistsInTarget or Item.Hint.ItemTrashed) { return; }

        if (item.Sync != null && item.SyncHint is Item.Hint.DocumentDownloadPathChanged)
        {
            FileHelper.Delete(item.Sync.Path);
        }

        using Stream sourceStream = await this.tablet.Download(item.Id).ConfigureAwait(false);
        using Stream targetStream = FileHelper.Create(item.SyncPath);
        await sourceStream.CopyToAsync(targetStream).ConfigureAwait(false);

        // TODO: Save in method with semaphore
        using DatabaseContext database = new DatabaseContext(this.dataSource);
        Sync? sync = await database.Syncs.FindAsync(item.Id).ConfigureAwait(false);
        if (sync != null)
        {
            sync.Modified = item.Modified;
            sync.Downloaded = item.SyncPath;
        }
        else
        {
            await database.Syncs.AddAsync(new Sync(item.Id, item.Modified, item.SyncPath)).ConfigureAwait(false);
        }
        await database.SaveChangesAsync().ConfigureAwait(false);
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

    internal sealed class Item
    {
        public enum Hint
        {
            DocumentDownloadPathChanged,
            DocumentExistsInTarget,
            DocumentNotFoundInTarget,
            ItemModified,
            ItemNew,
            ItemTrashed
        }

        public Item(Tablet.Item tabletItem, IEnumerable<Item>? collection, Backup? previousBackup, Sync? previousSync, String? syncPath)
        {
            this.Backup = previousBackup?.Modified;
            this.BackupHint = GetBackupHint(tabletItem, previousBackup);
            this.Collection = collection;
            this.Id = tabletItem.Id;
            this.Modified = tabletItem.Modified;
            this.Name = tabletItem.Name;
            this.Sync = (previousSync != null) ? new SyncDetail(previousSync) : null;
            this.SyncPath = syncPath;
            this.SyncHint = GetSyncHint(tabletItem, previousSync, syncPath);
            this.Trashed = tabletItem.Trashed;
        }

        public DateTime? Backup { get; }
        public Hint? BackupHint { get; }
        public IEnumerable<Item>? Collection { get; }
        public String Id { get; }
        public DateTime Modified { get; }
        public String Name { get; }
        public SyncDetail? Sync { get; }
        public String? SyncPath { get; }
        public Hint? SyncHint { get; }
        public Boolean Trashed { get; }

        private static Hint? GetBackupHint(Tablet.Item tabletItem, Backup? previousBackup)
        {
            if (tabletItem.Trashed) { return Hint.ItemTrashed; }
            if (previousBackup == null) { return Hint.ItemNew; }
            if (previousBackup.Modified < tabletItem.Modified) { return Hint.ItemModified; }

            return null;
        }

        private static Hint? GetSyncHint(Tablet.Item tabletItem, Sync? previousSync, String? syncPath)
        {
            if (syncPath == null) { return null; }
            if (tabletItem.Collection != null) { return null; }

            if (tabletItem.Trashed) { return Hint.ItemTrashed; }
            if (previousSync == null && Path.Exists(syncPath)) { return Hint.DocumentExistsInTarget; }
            if (previousSync == null) { return Hint.ItemNew; }
            if (previousSync.Downloaded != syncPath) { return Hint.DocumentDownloadPathChanged; }
            if (previousSync.Modified < tabletItem.Modified) { return Hint.ItemModified; }
            if (!Path.Exists(syncPath)) { return Hint.DocumentNotFoundInTarget; }

            return null;
        }

        internal sealed class SyncDetail
        {
            public SyncDetail(Sync sync)
            {
                this.Date = sync.Modified;
                this.Path = sync.Downloaded;
            }

            public DateTime Date { get; }
            public String Path { get; }
        }
    }
}
