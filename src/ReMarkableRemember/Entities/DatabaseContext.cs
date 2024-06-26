using System;
using Microsoft.EntityFrameworkCore;
using ReMarkableRemember.Helper;

namespace ReMarkableRemember.Entities;

internal sealed class DatabaseContext : DbContext
{
    public DatabaseContext(String dataSource) : base(new DbContextOptionsBuilder<DatabaseContext>().UseSqlite($"Data Source={dataSource}").Options)
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<DateTimeToStringConverter>();
    }

    public DbSet<Backup> Backups
    {
        get { return this.Set<Backup>(); }
    }

    public DbSet<Setting> Settings
    {
        get { return this.Set<Setting>(); }
    }

    public DbSet<SyncConfiguration> SyncConfigurations
    {
        get { return this.Set<SyncConfiguration>(); }
    }

    public DbSet<Sync> Syncs
    {
        get { return this.Set<Sync>(); }
    }

    public DbSet<Template> Templates
    {
        get { return this.Set<Template>(); }
    }
}
