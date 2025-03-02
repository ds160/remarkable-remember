using System;
using Microsoft.EntityFrameworkCore;
using ReMarkableRemember.Services.DataService.Entities;
using ReMarkableRemember.Services.DataService.Helper;

namespace ReMarkableRemember.Services.DataService.Database;

internal sealed class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions options) : base(options)
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
