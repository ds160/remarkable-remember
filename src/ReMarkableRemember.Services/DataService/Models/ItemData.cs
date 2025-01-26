using System;

namespace ReMarkableRemember.Services.DataService.Models;

public sealed class ItemData
{
    internal ItemData(String id, DateTime? backupDate, DateTime? syncData, String? syncPath, String? syncTargetDirectory)
    {
        this.BackupDate = backupDate;
        this.Id = id;
        this.SyncData = syncData;
        this.SyncPath = syncPath;
        this.SyncTargetDirectory = syncTargetDirectory;
    }

    public DateTime? BackupDate { get; }

    public String Id { get; }

    public DateTime? SyncData { get; }

    public String? SyncPath { get; }

    public String? SyncTargetDirectory { get; }
}