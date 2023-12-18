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
    private readonly MyScript myScript;
    private readonly Tablet tablet;

    public Controller(String dataSource)
    {
        this.database = new DatabaseContext(dataSource);
        this.database.Database.Migrate();

        String? myScriptApplicationKey = this.database.Settings.Find(Setting.Keys.MyScriptApplicationKey)?.Value;
        String? myScriptHmacKey = this.database.Settings.Find(Setting.Keys.MyScriptHmacKey)?.Value;
        this.myScript = new MyScript(myScriptApplicationKey, myScriptHmacKey);

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

    // public async Task<String> HandWritingRecognition(String id, String language)
    public async Task HandWritingRecognition()
    {
        String id = "476b9716-4b20-481f-bd71-044f0d682b48";
        String language = "de_DE";

        Notebook notebook = await this.tablet.GetNotebook(id).ConfigureAwait(false);
        IEnumerable<String> myScriptPages = await Task.WhenAll(notebook.Pages.Select(page => this.myScript.Recognize(page, language))).ConfigureAwait(false);

        Console.WriteLine(String.Join(Environment.NewLine, myScriptPages));
        // return String.Join(Environment.NewLine, myScriptPages);
    }

    public async Task Sync()
    {
        IEnumerable<Tablet.Item> tabletItems = await this.tablet.GetItems().ConfigureAwait(false);

        SyncConfiguration? configuration = await this.database.SyncConfigurations.FirstOrDefaultAsync().ConfigureAwait(false);
        if (configuration == null) { return; }

        Tablet.Item tabletItem = tabletItems.Single(item => item.Id == configuration.Id);
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
