using System;
using System.IO;
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
        SyncConfiguration configuration = await this.database.SyncConfigurations.FirstAsync().ConfigureAwait(false);

        using Stream sourceStream = await this.tablet.Download(configuration.Id).ConfigureAwait(false);
        using Stream targetStream = File.Create(Path.Combine(configuration.Destination, "Sepp.pdf"));

        await sourceStream.CopyToAsync(targetStream).ConfigureAwait(false);

        SyncDocument? document = await this.database.SyncDocuments.FindAsync(configuration.Id).ConfigureAwait(false);
        if (document != null)
        {
            document.Modified = DateTime.Now;
        }
        else
        {
            this.database.SyncDocuments.Add(new SyncDocument(configuration.Id, DateTime.Now, "Sepp"));
        }
        await this.database.SaveChangesAsync().ConfigureAwait(false);
    }
}
