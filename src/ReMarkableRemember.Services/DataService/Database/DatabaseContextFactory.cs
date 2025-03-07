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
    public static String BuildConnectionString(String? arg)
    {
        String dataSource = File.Exists(arg) ? arg : FileSystem.CreateApplicationDataFilePath("database.db");
        return new SqliteConnectionStringBuilder() { DataSource = dataSource }.ToString();
    }

    public DatabaseContext CreateDbContext(String[] args)
    {
        String connectionString = BuildConnectionString(args.FirstOrDefault());
        return new DatabaseContext(new DbContextOptionsBuilder().UseSqlite(connectionString).Options);
    }
}
