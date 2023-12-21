using System;
using System.Collections.Generic;
using System.IO;
using ReMarkableRemember.Entities;

namespace ReMarkableRemember.Models;

internal sealed class Item
{
    [Flags]
    public enum Hint
    {
        None = 0,
        NotFoundInTarget = 1,
        SyncPathChanged = 2,
        Modified = 4,
        New = 8,
        ExistsInTarget = 16,
        Trashed = 32
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
    public Hint BackupHint { get; }
    public IEnumerable<Item>? Collection { get; }
    public String Id { get; }
    public DateTime Modified { get; }
    public String Name { get; }
    public SyncDetail? Sync { get; }
    public String? SyncPath { get; }
    public Hint SyncHint { get; }
    public Boolean Trashed { get; }

    private static Hint GetBackupHint(Tablet.Item tabletItem, Backup? previousBackup)
    {
        if (tabletItem.Trashed) { return Hint.Trashed; }
        if (previousBackup == null) { return Hint.New; }
        if (previousBackup.Modified < tabletItem.Modified) { return Hint.Modified; }

        return Hint.None;
    }

    private static Hint GetSyncHint(Tablet.Item tabletItem, Sync? previousSync, String? syncPath)
    {
        if (syncPath == null) { return Hint.None; }
        if (tabletItem.Collection != null) { return Hint.None; }

        if (tabletItem.Trashed) { return Hint.Trashed; }
        if (previousSync == null && Path.Exists(syncPath)) { return Hint.ExistsInTarget; }
        if (previousSync == null) { return Hint.New; }
        if (previousSync.Path != syncPath) { return Hint.SyncPathChanged; }
        if (previousSync.Modified < tabletItem.Modified) { return Hint.Modified; }
        if (!Path.Exists(syncPath)) { return Hint.NotFoundInTarget; }

        return Hint.None;
    }

    internal sealed class SyncDetail
    {
        public SyncDetail(Sync sync)
        {
            this.Date = sync.Modified;
            this.Path = sync.Path;
        }

        public DateTime Date { get; }
        public String Path { get; }
    }
}
