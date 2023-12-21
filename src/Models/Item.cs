using System;
using System.Collections.Generic;
using System.IO;
using ReMarkableRemember.Entities;

namespace ReMarkableRemember.Models;

internal sealed class Item
{
    public enum Hint
    {
        DocumentExistsInTarget,
        DocumentNotFoundInTarget,
        DocumentSyncPathChanged,
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
        if (previousSync.Downloaded != syncPath) { return Hint.DocumentSyncPathChanged; }
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
