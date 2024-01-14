using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

    public async Task UploadTemplate([NotNull] TabletTemplate template)
    {
        await this.Tablet.UploadTemplate(template).ConfigureAwait(false);

        this.SaveTemplate(template);
    }

    private void SaveTemplate(TabletTemplate tabletTemplate)
    {
        using DatabaseContext database = this.CreateDatabaseContext();

        Template? template = database.Templates.Find(tabletTemplate.Category, tabletTemplate.Name);
        if (template != null)
        {
            template.IconCode = tabletTemplate.IconCode;
            template.BytesPng = tabletTemplate.BytesPng;
            template.BytesSvg = tabletTemplate.BytesSvg;
        }
        else
        {
            database.Templates.Add(new Template(tabletTemplate.Category, tabletTemplate.Name, tabletTemplate.IconCode, tabletTemplate.BytesPng, tabletTemplate.BytesSvg));
        }

        database.SaveChanges();
    }
}
