using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ReMarkableRemember.Entities;
using System.Threading.Tasks;
using ReMarkableRemember.Helper;

namespace ReMarkableRemember.Models;

internal sealed class Item
{
    private readonly Controller controller;
    private Backup? dataBackup;
    private Sync? dataSync;

    public Item(Controller controller, Tablet.Item tabletItem, Item? parent)
    {
        this.controller = controller;

        this.Collection = tabletItem.Collection?.Select(childTabletItem => new Item(controller, childTabletItem, this)).ToArray();
        this.Id = tabletItem.Id;
        this.Modified = tabletItem.Modified;
        this.Name = tabletItem.Name;
        this.Parent = parent;
        this.Trashed = tabletItem.Trashed;

        using DatabaseContext database = this.controller.CreateDatabaseContext();
        this.Update(database);
    }

    public DateTime? BackupDate { get { return this.dataBackup?.Modified; } }

    public ItemHint BackupHint { get; private set; }

    public IEnumerable<Item>? Collection { get; }

    public String Id { get; }

    public DateTime Modified { get; }

    public String Name { get; }

    internal Item? Parent { get; }

    public DateTime? SyncDate { get { return this.dataSync?.Modified; } }

    public ItemHint SyncHint { get; private set; }

    public String? SyncPath { get; private set; }

    public Boolean Trashed { get; }

    public async Task<Boolean> Backup()
    {
        if (this.BackupHint is ItemHint.None) { return false; }
        if (this.Trashed) { return false; }

        String targetDirectory = this.controller.Settings.Backup;
        if (!Directory.Exists(targetDirectory)) { throw new SettingsException("Backup directory not set or not found."); }

        IEnumerable<String> directories = Directory.GetDirectories(targetDirectory, $"{this.Id}*");
        foreach (String directory in directories)
        {
            FileSystem.Delete(directory);
        }

        IEnumerable<String> files = Directory.GetFiles(targetDirectory).Where(file => file.StartsWith(Path.Combine(targetDirectory, this.Id), StringComparison.Ordinal));
        foreach (String file in files)
        {
            FileSystem.Delete(file);
        }

        await this.controller.Tablet.Backup(this.Id, targetDirectory).ConfigureAwait(false);

        this.BackupDone();

        return true;
    }

    private void BackupDone()
    {
        using DatabaseContext database = this.controller.CreateDatabaseContext();
        if (this.dataBackup != null)
        {
            this.dataBackup.Deleted = null;
            this.dataBackup.Modified = this.Modified;
        }
        else
        {
            this.dataBackup = new Backup(this.Id, this.Modified);
            database.Backups.Add(this.dataBackup);
        }
        database.SaveChanges();

        this.BackupHint = this.GetBackupHint();
    }

    public async Task<String> HandWritingRecognition()
    {
        Notebook notebook = await this.controller.Tablet.GetNotebook(this.Id).ConfigureAwait(false);
        IEnumerable<String> myScriptPages = await Task.WhenAll(notebook.Pages.Select(this.controller.MyScript.Recognize)).ConfigureAwait(false);
        return String.Join(Environment.NewLine, myScriptPages);
    }

    public async Task SetSyncTargetDirectory(String? targetDirectory)
    {
        using DatabaseContext database = this.controller.CreateDatabaseContext();
        SyncConfiguration? syncConfiguration = await database.SyncConfigurations.FindAsync(this.Id).ConfigureAwait(false);
        if (targetDirectory != null)
        {
            if (syncConfiguration != null)
            {
                syncConfiguration.TargetDirectory = targetDirectory;
            }
            else
            {
                await database.SyncConfigurations.AddAsync(new SyncConfiguration(this.Id, targetDirectory)).ConfigureAwait(false);
            }
        }
        else
        {
            if (syncConfiguration != null)
            {
                database.SyncConfigurations.Remove(syncConfiguration);
            }
        }
        await database.SaveChangesAsync().ConfigureAwait(false);

        this.Update(database);
    }

    public async Task<Boolean> Sync()
    {
        if (this.SyncHint is ItemHint.None or >= ItemHint.ExistsInTarget) { return false; }
        if (this.SyncPath == null) { return false; }
        if (this.Trashed) { return false; }

        if (this.dataSync != null && this.SyncHint is ItemHint.SyncPathChanged)
        {
            FileSystem.Delete(this.dataSync.Path);
        }

        using Stream sourceStream = await this.controller.Tablet.Download(this.Id).ConfigureAwait(false);
        using Stream targetStream = FileSystem.Create(this.SyncPath);
        await sourceStream.CopyToAsync(targetStream).ConfigureAwait(false);

        this.SyncDone(this.SyncPath);

        return true;
    }

    private void SyncDone(String path)
    {
        using DatabaseContext database = this.controller.CreateDatabaseContext();
        if (this.dataSync != null)
        {
            this.dataSync.Modified = this.Modified;
            this.dataSync.Path = path;
        }
        else
        {
            this.dataSync = new Sync(this.Id, this.Modified, path);
            database.Syncs.Add(this.dataSync);
        }
        database.SaveChanges();

        this.SyncHint = this.GetSyncHint();
    }

    private ItemHint GetBackupHint()
    {
        if (this.dataBackup == null) { return ItemHint.New; }
        if (this.dataBackup.Modified < this.Modified) { return ItemHint.Modified; }

        return ItemHint.None;
    }

    private ItemHint GetSyncHint()
    {
        if (this.Collection != null) { return ItemHint.None; }
        if (this.SyncPath == null) { return ItemHint.None; }

        if (this.dataSync == null && Path.Exists(this.SyncPath)) { return ItemHint.ExistsInTarget; }
        if (this.dataSync == null) { return ItemHint.New; }
        if (this.dataSync.Path != this.SyncPath) { return ItemHint.SyncPathChanged; }
        if (this.dataSync.Modified < this.Modified) { return ItemHint.Modified; }
        if (!Path.Exists(this.SyncPath)) { return ItemHint.NotFoundInTarget; }

        return ItemHint.None;
    }

    private String? GetSyncPath(SyncConfiguration? syncConfiguration)
    {
        String? targetDirectory = null;

        if (syncConfiguration != null)
        {
            targetDirectory = syncConfiguration.TargetDirectory;
        }
        else if (this.Parent != null && this.Parent.SyncPath != null)
        {
            targetDirectory = (this.Collection != null) ? Path.Combine(this.Parent.SyncPath, this.Name) : this.Parent.SyncPath;
        }

        return (targetDirectory != null && this.Collection == null) ? Path.Combine(targetDirectory, this.Name) : targetDirectory;
    }

    private void Update(DatabaseContext database)
    {
        SyncConfiguration? syncConfiguration = database.SyncConfigurations.Find(this.Id);
        this.SyncPath = this.GetSyncPath(syncConfiguration);

        this.dataBackup = database.Backups.Find(this.Id);
        this.BackupHint = this.GetBackupHint();

        this.dataSync = database.Syncs.Find(this.Id);
        this.SyncHint = this.GetSyncHint();

        this.Collection?.ToList()?.ForEach(childItem => childItem.Update(database));
    }
}
