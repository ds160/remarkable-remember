using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ReMarkableRemember.Services.DataService.Database;
using ReMarkableRemember.Services.DataService.Entities;
using ReMarkableRemember.Services.DataService.Models;

namespace ReMarkableRemember.Services.DataService;

public sealed class DataServiceSqlite : IDataService
{
    private readonly String dataSource;

    public DataServiceSqlite(String? arg)
    {
        this.dataSource = DatabaseSource.GetDataSource(arg);

        using DatabaseContext database = this.CreateDatabaseContext();
        IMigrator migrator = database.GetService<IMigrator>();
        migrator.Migrate();
    }

    private DatabaseContext CreateDatabaseContext()
    {
        return new DatabaseContext(this.dataSource);
    }



    public async Task<ItemData> GetItem(String id)
    {
        using DatabaseContext database = this.CreateDatabaseContext();

        Backup? backup = await database.Backups.FindAsync(id).ConfigureAwait(false);
        Sync? sync = await database.Syncs.FindAsync(id).ConfigureAwait(false);
        SyncConfiguration? syncConfiguration = await database.SyncConfigurations.FindAsync(id).ConfigureAwait(false);

        return new ItemData(id, backup?.Modified, sync?.Modified, sync?.Path, syncConfiguration?.TargetDirectory);
    }

    public async Task<ItemData> SetItemBackup(String id, DateTime modified)
    {
        using DatabaseContext database = this.CreateDatabaseContext();

        Backup? backup = await database.Backups.FindAsync(id).ConfigureAwait(false);
        if (backup != null)
        {
            backup.Deleted = null;
            backup.Modified = modified;
        }
        else
        {
            backup = new Backup(id, modified);
            await database.Backups.AddAsync(backup).ConfigureAwait(false);
        }

        await database.SaveChangesAsync().ConfigureAwait(false);

        Sync? sync = await database.Syncs.FindAsync(id).ConfigureAwait(false);
        SyncConfiguration? syncConfiguration = await database.SyncConfigurations.FindAsync(id).ConfigureAwait(false);
        return new ItemData(id, modified, sync?.Modified, sync?.Path, syncConfiguration?.TargetDirectory);
    }

    public async Task<ItemData> SetItemSync(String id, DateTime modified, String path)
    {
        using DatabaseContext database = this.CreateDatabaseContext();

        Sync? sync = await database.Syncs.FindAsync(id).ConfigureAwait(false);
        if (sync != null)
        {
            sync.Modified = modified;
            sync.Path = path;
        }
        else
        {
            sync = new Sync(id, modified, path);
            await database.Syncs.AddAsync(sync).ConfigureAwait(false);
        }

        await database.SaveChangesAsync().ConfigureAwait(false);

        Backup? backup = await database.Backups.FindAsync(id).ConfigureAwait(false);
        SyncConfiguration? syncConfiguration = await database.SyncConfigurations.FindAsync(id).ConfigureAwait(false);
        return new ItemData(id, backup?.Modified, modified, path, syncConfiguration?.TargetDirectory);
    }

    public async Task<ItemData> SetItemSyncTargetDirectory(String id, String? targetDirectory)
    {
        using DatabaseContext database = this.CreateDatabaseContext();

        SyncConfiguration? syncConfiguration = await database.SyncConfigurations.FindAsync(id).ConfigureAwait(false);
        if (targetDirectory != null)
        {
            if (syncConfiguration != null)
            {
                syncConfiguration.TargetDirectory = targetDirectory;
            }
            else
            {
                syncConfiguration = new SyncConfiguration(id, targetDirectory);
                await database.SyncConfigurations.AddAsync(syncConfiguration).ConfigureAwait(false);
            }
        }
        else
        {
            if (syncConfiguration != null)
            {
                database.SyncConfigurations.Remove(syncConfiguration);
            }
        }

        await database.SaveChangesAsync();

        Backup? backup = await database.Backups.FindAsync(id).ConfigureAwait(false);
        Sync? sync = await database.Syncs.FindAsync(id).ConfigureAwait(false);
        return new ItemData(id, backup?.Modified, sync?.Modified, sync?.Path, targetDirectory);
    }



    public async Task LoadSettings(IEnumerable<SettingData> settings)
    {
        using DatabaseContext database = this.CreateDatabaseContext();

        foreach (SettingData setting in settings)
        {
            Setting? databaseSetting = await database.Settings.FindAsync($"{setting.Prefix} {setting.Key}").ConfigureAwait(false);
            if (databaseSetting != null)
            {
                setting.Value = databaseSetting.Value;
            }
        }
    }

    public async Task SaveSettings(IEnumerable<SettingData> settings)
    {
        using DatabaseContext database = this.CreateDatabaseContext();

        foreach (SettingData setting in settings)
        {
            Setting? databaseSetting = await database.Settings.FindAsync($"{setting.Prefix} {setting.Key}").ConfigureAwait(false);
            if (databaseSetting != null)
            {
                databaseSetting.Value = setting.Value;
            }
            else
            {
                databaseSetting = new Setting(setting.Key, setting.Value);
                await database.Settings.AddAsync(databaseSetting).ConfigureAwait(false);
            }
        }

        await database.SaveChangesAsync().ConfigureAwait(false);
    }



    public async Task DeleteTemplate(String category, String name)
    {
        using DatabaseContext database = this.CreateDatabaseContext();

        Template? template = await database.Templates.FindAsync(category, name).ConfigureAwait(false);
        if (template != null)
        {
            database.Templates.Remove(template);
            await database.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    public Task<IEnumerable<TemplateData>> GetTemplates()
    {
        using DatabaseContext database = this.CreateDatabaseContext();

        IEnumerable<TemplateData> templates = database.Templates.Select(template => new TemplateData(template.Category, template.Name, template.IconCode, template.BytesPng, template.BytesSvg)).ToArray();

        return Task.FromResult(templates);
    }

    public async Task SetTemplate(TemplateData templateData)
    {
        using DatabaseContext database = this.CreateDatabaseContext();

        Template? template = await database.Templates.FindAsync(templateData.Category, templateData.Name).ConfigureAwait(false);
        if (template != null)
        {
            template.IconCode = templateData.IconCode;
            template.BytesPng = templateData.BytesPng;
            template.BytesSvg = templateData.BytesSvg;
        }
        else
        {
            template = new Template(templateData.Category, templateData.Name, templateData.IconCode, templateData.BytesPng, templateData.BytesSvg);
            await database.Templates.AddAsync(template).ConfigureAwait(false);
        }

        await database.SaveChangesAsync().ConfigureAwait(false);
    }
}