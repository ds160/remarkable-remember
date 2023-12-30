using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReMarkableRemember.Entities;
using ReMarkableRemember.Models.Interfaces;

namespace ReMarkableRemember.Models.Stubs;

internal sealed class ControllerStub : IController
{
    private readonly String dataSource;

    public ControllerStub(String dataSource)
    {
        using DatabaseContext database = new DatabaseContext(dataSource);
        database.Database.Migrate();

        this.Settings = new Settings(dataSource);

        this.dataSource = dataSource;
    }

    void IDisposable.Dispose()
    {
    }

    public Settings Settings { get; }

    public async Task<Boolean> BackupItem(Item item)
    {
        await Task.Delay(500).ConfigureAwait(false);
        item.BackupDone();
        return true;
    }

    public async Task<TabletConnectionError?> GetConnectionStatus()
    {
        await Task.Delay(100).ConfigureAwait(false);
        return null;
    }

    public async Task<IEnumerable<Item>> GetItems()
    {
        await Task.Delay(1000).ConfigureAwait(false);
        Int64 time = (DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks) / 10000;
        List<Item> items = new List<Item>() { new Item(this.dataSource, new Tablet.Item("1", $"{time}", String.Empty, "DocumentType", "Test"), null) };
        return items;
    }

    public async Task<String> HandWritingRecognition(Item item)
    {
        await Task.Delay(5000).ConfigureAwait(false);
        return "Hand Writing Recognition via MyScript done :)";
    }

    public async Task<Boolean> SyncItem(Item item)
    {
        await Task.Delay(500).ConfigureAwait(false);
        if (item.SyncPath != null)
        {
            item.SyncDone(item.SyncPath);
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task UploadTemplate(TabletTemplate template)
    {
        await Task.Delay(1000).ConfigureAwait(false);
    }
}
