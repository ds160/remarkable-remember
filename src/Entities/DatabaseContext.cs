using System;
using Microsoft.EntityFrameworkCore;
using ReMarkableRemember.Helper;

namespace ReMarkableRemember.Entities;

public class DatabaseContext : DbContext
{
    public DatabaseContext(String dataSource) : base(new DbContextOptionsBuilder<DatabaseContext>().UseSqlite($"Data Source={dataSource}").Options)
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        if (configurationBuilder == null) { throw new ArgumentNullException(nameof(configurationBuilder)); }

        configurationBuilder.Properties<DateTime>().HaveConversion<DateTimeToStringConverter>();
    }

    public DbSet<Backup> Backups { get; private set; }

    public DbSet<Setting> Settings { get; private set; }

    public DbSet<SyncConfiguration> SyncConfigurations { get; private set; }

    public DbSet<Sync> Syncs { get; private set; }
}
