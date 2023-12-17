using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReMarkableRemember.Entities;

namespace ReMarkableRemember.Models;

public sealed class Controller : IDisposable
{
    private readonly DatabaseContext database;
    private readonly Tablet tablet;

    public Controller(String dataSource)
    {
        this.database = new DatabaseContext(dataSource);
        this.database.Database.Migrate();

        String? tabletIp = this.database.Settings.Find(Setting.Keys.TabletIp)?.Value;
        String? tabletPassword = this.database.Settings.Find(Setting.Keys.TabletPassword)?.Value;
        this.tablet = new Tablet(tabletIp, tabletPassword);
    }

    public void Dispose()
    {
        this.database.Dispose();
        this.tablet.Dispose();

        GC.SuppressFinalize(this);
    }

    public async Task<String?> GetConnectionStatus()
    {
        return await this.tablet.GetConnectionStatus().ConfigureAwait(false);
    }

    public async Task Sync()
    {
        IEnumerable<TabletItem> tabletItems = await this.tablet.GetItems().ConfigureAwait(false);

        SyncConfiguration? configuration = await this.database.SyncConfigurations.FirstOrDefaultAsync().ConfigureAwait(false);
        if (configuration == null) { return; }

        TabletItem tabletItem = tabletItems.Single(item => item.Id == configuration.Id);
        String downloadPath = Path.Combine(configuration.Destination, $"{tabletItem.Name}.pdf");

        using Stream sourceStream = await this.tablet.Download(configuration.Id).ConfigureAwait(false);
        using Stream targetStream = File.Create(downloadPath);

        await sourceStream.CopyToAsync(targetStream).ConfigureAwait(false);

        SyncDocument? document = await this.database.SyncDocuments.FindAsync(configuration.Id).ConfigureAwait(false);
        if (document != null)
        {
            document.Modified = tabletItem.Modified;
            document.Downloaded = downloadPath;
        }
        else
        {
            await this.database.SyncDocuments.AddAsync(new SyncDocument(configuration.Id, tabletItem.Modified, downloadPath)).ConfigureAwait(false);
        }
        await this.database.SaveChangesAsync().ConfigureAwait(false);
    }
}
