using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace ReMarkableRemember.Services.DataService;

public static class DependencyInjection
{
    public static IServiceCollection UseSqliteForDataService(this IServiceCollection services, String[]? args)
    {
        return services.AddSingleton<IDataService>(DataServiceSqlite.Create(args?.FirstOrDefault()));
    }
}
