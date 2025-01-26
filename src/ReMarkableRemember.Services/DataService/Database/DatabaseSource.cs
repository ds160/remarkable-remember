using System;
using System.IO;
using ReMarkableRemember.Common.FileSystem;

namespace ReMarkableRemember.Services.DataService.Database;

internal static class DatabaseSource
{
    public static String GetDataSource(String? arg)
    {
        if (File.Exists(arg)) { return arg; }

        return FileSystem.CreateApplicationDataFilePath("database.db");
    }
}
