using System;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace ReMarkableRemember.Entities;

public class DatabaseContext : DbContext
{
    private readonly String databaseFile;

    public DatabaseContext(String? databaseFile = null)
    {
        this.databaseFile = databaseFile ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ReMarkableRemember.db");
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        if (configurationBuilder == null) { throw new ArgumentNullException(nameof(configurationBuilder)); }

        configurationBuilder.Properties<DateTime>().HaveConversion<DateTimeConverter>();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={this.databaseFile}");
    }

    public DbSet<Backup> Backups { get; private set; }

    public DbSet<Setting> Settings { get; private set; }

    public DbSet<SyncConfiguration> SyncConfigurations { get; private set; }

    public DbSet<SyncDocument> SyncDocuments { get; private set; }
}
