using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Design;

namespace ReMarkableRemember.Services.DataService.Database;

internal sealed class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(String[] args)
    {
        String dataSource = DatabaseSource.GetDataSource(args.FirstOrDefault());
        return new DatabaseContext(dataSource);
    }
}
