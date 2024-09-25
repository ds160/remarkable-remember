using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.Design;
using ReMarkableRemember.Helper;

namespace ReMarkableRemember.Entities;

internal sealed class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(String[] args)
    {
        String dataSource = GetDataSource(args.FirstOrDefault());
        return new DatabaseContext(dataSource);
    }

    public static String GetDataSource(String? arg)
    {
        if (File.Exists(arg)) { return arg; }

        return FileSystem.CreateApplicationDataFilePath("database.db");
    }
}
