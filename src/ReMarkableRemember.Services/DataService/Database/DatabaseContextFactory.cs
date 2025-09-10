using System;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ReMarkableRemember.Common.FileSystem;

namespace ReMarkableRemember.Services.DataService.Database;

internal sealed class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    private const String DEFAULT_DATABASE_FILE_NAME = "database.db";

    public static String BuildConnectionString(String? arg)
    {
        String? dataSource = Directory.Exists(arg)
            ? Path.Combine(arg, DEFAULT_DATABASE_FILE_NAME)
            : FileSystem.EnsureExists(arg) ?? FileSystem.CreateApplicationDataFilePath(DEFAULT_DATABASE_FILE_NAME);

        return new SqliteConnectionStringBuilder() { DataSource = dataSource }.ToString();
    }

    public DatabaseContext CreateDbContext(String[] args)
    {
        String connectionString = BuildConnectionString(args.FirstOrDefault());
        return new DatabaseContext(new DbContextOptionsBuilder().UseSqlite(connectionString).Options);
    }
}
