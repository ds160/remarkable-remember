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

    public async Task<IEnumerable<Item>> GetItems(Boolean includeTrashed = false)
    {
        IEnumerable<Tablet.Item> tabletItems = await this.Tablet.GetItems().ConfigureAwait(false);
        IEnumerable<Item> items = tabletItems.Select(tabletItem => new Item(this, tabletItem, null)).ToArray();
        return includeTrashed ? items : items.Where(item => !item.Trashed).ToArray();
    }

    public IEnumerable<TabletTemplate> GetTemplates()
    {
        using DatabaseContext database = this.CreateDatabaseContext();

        IEnumerable<Template> templates = database.Templates;
        return templates.Select(template => new TabletTemplate(this, template)).ToArray();
    }

    public async Task InstallLamyEraser(Boolean press, Boolean undo, Boolean leftHanded)
    {
        await this.Tablet.InstallLamyEraser(press, undo, leftHanded).ConfigureAwait(false);
    }

    public async Task InstallWebInterfaceOnBoot()
    {
        await this.Tablet.InstallWebInterfaceOnBoot().ConfigureAwait(false);
    }

    public async Task Restart()
    {
        await this.Tablet.Restart().ConfigureAwait(false);
    }

    public async Task UploadFile(String path, Item? parentItem)
    {
        await this.Tablet.UploadFile(path, UploadFileParentId(parentItem)).ConfigureAwait(false);
    }

    private static String? UploadFileParentId(Item? parentItem)
    {
        while (parentItem != null)
        {
            if (parentItem.Collection != null)
            {
                return parentItem.Id;
            }

            parentItem = parentItem.Parent;
        }

        return null;
    }
}
