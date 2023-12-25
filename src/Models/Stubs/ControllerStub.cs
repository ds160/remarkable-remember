using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReMarkableRemember.Models.Interfaces;

namespace ReMarkableRemember.Models.Stubs;

internal sealed class ControllerStub : IController
{
    private readonly String dataSource;

    public ControllerStub(String dataSource)
    {
        this.dataSource = dataSource;
    }

    void IDisposable.Dispose()
    {
    }

    public async Task<Boolean> BackupItem(Item item)
    {
        return await Task.Run(() =>
        {
            item.BackupDone();
            return true;
        }).ConfigureAwait(false);
    }

    public async Task<TabletConnectionError?> GetConnectionStatus()
    {
        return await Task.Run(() =>
        {
            TabletConnectionError? status = null;
            return status;
        }).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Item>> GetItems()
    {
        return await Task.Run(() =>
        {
            Int64 time = (DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks) / 10000;
            List<Item> items = new List<Item>() { new Item(this.dataSource, new Tablet.Item("1", $"{time}", String.Empty, "DocumentType", "Test"), null, null) };
            return items;
        }).ConfigureAwait(false);
    }

    public async Task<String> HandWritingRecognition(Item item, String language)
    {
        return await Task.Run(() =>
        {
            return "Hand Writing Recognition via MyScript done :)";
        }).ConfigureAwait(false);
    }

    public async Task<Boolean> SyncItem(Item item)
    {
        return await Task.Run(() =>
        {
            if (item.SyncPath != null)
            {
                item.SyncDone(item.SyncPath);
                return true;
            }
            else
            {
                return false;
            }
        }).ConfigureAwait(false);
    }

    public async Task UploadTemplate(TabletTemplate template)
    {
        await Task.Delay(1).ConfigureAwait(false);
    }
}
