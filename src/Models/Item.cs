using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ReMarkableRemember.Entities;

namespace ReMarkableRemember.Models;

internal sealed class Item
{
    public enum Hint
    {
        None = 0,
        NotFoundInTarget = 1,
        SyncPathChanged = 2,
        Modified = 4,
        New = 8,
        ExistsInTarget = 16
    }

    private readonly String dataSource;

    public Item(String dataSource, Tablet.Item tabletItem, Item? parent)
    {
        this.dataSource = dataSource;

        this.Collection = tabletItem.Collection?.Select(childTabletItem => new Item(dataSource, childTabletItem, this)).ToArray();
        this.Id = tabletItem.Id;
        this.Modified = tabletItem.Modified;
        this.Name = tabletItem.Name;
        this.Parent = parent;
        this.Trashed = tabletItem.Trashed;

        using DatabaseContext database = new DatabaseContext(dataSource);
        this.Update(database);
    }

    public DateTime? Backup { get; private set; }
    public Hint BackupHint { get; private set; }
    public IEnumerable<Item>? Collection { get; }
    public String Id { get; }
    public DateTime Modified { get; }
    public String Name { get; }
    internal Item? Parent { get; }
    public SyncDetail? Sync { get; private set; }
    public Hint SyncHint { get; private set; }
    public String? SyncPath { get; private set; }
    public Boolean Trashed { get; }

    internal void BackupDone()
    {
        using DatabaseContext database = new DatabaseContext(this.dataSource);
        Backup? backup = database.Backups.Find(this.Id);
        if (backup != null)
        {
            backup.Deleted = null;
            backup.Modified = this.Modified;
        }
        else
        {
            database.Backups.Add(new Backup(this.Id, this.Modified));
        }
        database.SaveChanges();

        this.Backup = this.Modified;
        this.BackupHint = this.GetBackupHint();
    }

    public void SetSyncTargetDirectory(String? targetDirectory)
    {
        using DatabaseContext database = new DatabaseContext(this.dataSource);
        SyncConfiguration? syncConfiguration = database.SyncConfigurations.Find(this.Id);
        if (targetDirectory != null)
        {
            if (syncConfiguration != null)
            {
                syncConfiguration.TargetDirectory = targetDirectory;
            }
            else
            {
                database.SyncConfigurations.Add(new SyncConfiguration(this.Id, targetDirectory));
            }
        }
        else
        {
            if (syncConfiguration != null)
            {
                database.SyncConfigurations.Remove(syncConfiguration);
            }
        }
        database.SaveChanges();

        this.Update(database);
    }

    internal void SyncDone(String path)
    {
        using DatabaseContext database = new DatabaseContext(this.dataSource);
        Sync? sync = database.Syncs.Find(this.Id);
        if (sync != null)
        {
            sync.Modified = this.Modified;
            sync.Path = path;
        }
        else
        {
            database.Syncs.Add(new Sync(this.Id, this.Modified, path));
        }
        database.SaveChanges();

        this.Sync = new SyncDetail(this.Modified, path);
        this.SyncHint = this.GetSyncHint();
    }

    private Hint GetBackupHint()
    {
        if (this.Backup == null) { return Hint.New; }
        if (this.Backup < this.Modified) { return Hint.Modified; }

        return Hint.None;
    }

    private Hint GetSyncHint()
    {
        if (this.Collection != null) { return Hint.None; }
        if (this.SyncPath == null) { return Hint.None; }

        if (this.Sync == null && Path.Exists(this.SyncPath)) { return Hint.ExistsInTarget; }
        if (this.Sync == null) { return Hint.New; }
        if (this.Sync.Path != this.SyncPath) { return Hint.SyncPathChanged; }
        if (this.Sync.Modified < this.Modified) { return Hint.Modified; }
        if (!Path.Exists(this.SyncPath)) { return Hint.NotFoundInTarget; }

        return Hint.None;
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

        Backup? backup = database.Backups.Find(this.Id);
        this.Backup = backup?.Modified;
        this.BackupHint = this.GetBackupHint();

        Sync? sync = database.Syncs.Find(this.Id);
        this.Sync = (sync != null) ? new SyncDetail(sync.Modified, sync.Path) : null;
        this.SyncHint = this.GetSyncHint();

        this.Collection?.ToList()?.ForEach(childItem => childItem.Update(database));
    }

    internal sealed class SyncDetail
    {
        public SyncDetail(DateTime modified, String path)
        {
            this.Modified = modified;
            this.Path = path;
        }

        public DateTime Modified { get; }
        public String Path { get; }
    }
}
