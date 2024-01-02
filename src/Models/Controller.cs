using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReMarkableRemember.Entities;

namespace ReMarkableRemember.Models;

public sealed class Controller : IDisposable
{
    private readonly String dataSource;

    public Controller(String dataSource)
    {
        this.dataSource = dataSource;

        using DatabaseContext database = this.CreateDatabaseContext();
        database.Database.Migrate();

        this.Settings = new Settings(this);
        this.MyScript = new MyScript(this.Settings);
        this.Tablet = new Tablet(this.Settings);
    }

    internal MyScript MyScript { get; }

    internal Tablet Tablet { get; }

    public Settings Settings { get; }

    internal DatabaseContext CreateDatabaseContext()
    {
        return new DatabaseContext(this.dataSource);
    }

    public void Dispose()
    {
        this.Tablet.Dispose();

        GC.SuppressFinalize(this);
    }

    public async Task<TabletConnectionError?> GetConnectionStatus()
    {
        return await this.Tablet.GetConnectionStatus().ConfigureAwait(false);
    }

    public async Task<IEnumerable<Item>> GetItems()
    {
        IEnumerable<Tablet.Item> tabletItems = await this.Tablet.GetItems().ConfigureAwait(false);
        return tabletItems.Select(tabletItem => new Item(this, tabletItem, null)).ToArray();
    }

    public async Task UploadTemplate(TabletTemplate template)
    {
        await this.Tablet.UploadTemplate(template).ConfigureAwait(false);
    }
}
