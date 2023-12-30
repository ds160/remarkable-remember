using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReMarkableRemember.Entities;
using ReMarkableRemember.Helper;
using ReMarkableRemember.Models.Interfaces;

namespace ReMarkableRemember.Models;

internal sealed class Controller : IController
{
    private readonly String dataSource;
    private readonly MyScript myScript;
    private readonly Tablet tablet;

    public Controller(String dataSource)
    {
        using DatabaseContext database = new DatabaseContext(dataSource);
        database.Database.Migrate();

        this.Settings = new Settings(dataSource);

        this.dataSource = dataSource;
        this.myScript = new MyScript(this.Settings);
        this.tablet = new Tablet(this.Settings);
    }

    void IDisposable.Dispose()
    {
        this.tablet.Dispose();

        GC.SuppressFinalize(this);
    }

    public Settings Settings { get; }

    public async Task<Boolean> BackupItem(Item item)
    {
        if (item.BackupHint is Item.Hint.None) { return false; }
        if (item.Trashed) { return false; }

        if (!Directory.Exists(this.Settings.Backup)) { throw new SettingsException("Backup directory not set or not found."); }

        IEnumerable<String> directories = Directory.GetDirectories(this.Settings.Backup, $"{item.Id}*");
        foreach (String directory in directories)
        {
            FileSystem.Delete(directory);
        }

        IEnumerable<String> files = Directory.GetFiles(this.Settings.Backup).Where(file => file.StartsWith(Path.Combine(this.Settings.Backup, item.Id), StringComparison.Ordinal));
        foreach (String file in files)
        {
            FileSystem.Delete(file);
        }

        await this.tablet.Backup(item.Id, this.Settings.Backup).ConfigureAwait(false);

        item.BackupDone();

        return true;
    }

    public async Task<TabletConnectionError?> GetConnectionStatus()
    {
        return await this.tablet.GetConnectionStatus().ConfigureAwait(false);
    }

    public async Task<IEnumerable<Item>> GetItems()
    {
        IEnumerable<Tablet.Item> tabletItems = await this.tablet.GetItems().ConfigureAwait(false);
        return tabletItems.Select(tabletItem => new Item(this.dataSource, tabletItem, null)).ToArray();
    }

    public async Task<String> HandWritingRecognition(Item item)
    {
        Notebook notebook = await this.tablet.GetNotebook(item.Id).ConfigureAwait(false);
        IEnumerable<String> myScriptPages = await Task.WhenAll(notebook.Pages.Select(this.myScript.Recognize)).ConfigureAwait(false);
        return String.Join(Environment.NewLine, myScriptPages);
    }

    public async Task<Boolean> SyncItem(Item item)
    {
        if (item.SyncHint is Item.Hint.None or >= Item.Hint.ExistsInTarget) { return false; }
        if (item.SyncPath == null) { return false; }
        if (item.Trashed) { return false; }

        if (item.Sync != null && item.SyncHint is Item.Hint.SyncPathChanged)
        {
            FileSystem.Delete(item.Sync.Path);
        }

        using Stream sourceStream = await this.tablet.Download(item.Id).ConfigureAwait(false);
        using Stream targetStream = FileSystem.Create(item.SyncPath);
        await sourceStream.CopyToAsync(targetStream).ConfigureAwait(false);

        item.SyncDone(item.SyncPath);

        return true;
    }

    public async Task UploadTemplate(TabletTemplate template)
    {
        await this.tablet.UploadTemplate(template).ConfigureAwait(false);
    }
}
