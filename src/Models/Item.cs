using System;
using System.Collections.Generic;
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

    public Item(String dataSource, Tablet.Item tabletItem, IEnumerable<Item>? collection, String? syncPath)
    {
        this.dataSource = dataSource;

        using DatabaseContext database = new DatabaseContext(this.dataSource);
        Backup? backup = database.Backups.Find(tabletItem.Id);
        Sync? sync = database.Syncs.Find(tabletItem.Id);

        this.Backup = backup?.Modified;
        this.Collection = collection;
        this.Id = tabletItem.Id;
        this.Modified = tabletItem.Modified;
        this.Name = tabletItem.Name;
        this.Sync = (sync != null) ? new SyncDetail(sync.Modified, sync.Path) : null;
        this.SyncPath = syncPath;
        this.Trashed = tabletItem.Trashed;

        this.BackupHint = this.GetBackupHint();
        this.SyncHint = this.GetSyncHint();
    }

    public DateTime? Backup { get; private set; }
    public Hint BackupHint { get; private set; }
    public IEnumerable<Item>? Collection { get; }
    public String Id { get; }
    public DateTime Modified { get; }
    public String Name { get; }
    public SyncDetail? Sync { get; private set; }
    public String? SyncPath { get; }
    public Hint SyncHint { get; private set; }
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
