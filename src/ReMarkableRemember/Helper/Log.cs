using System;
using System.Globalization;
using System.IO;
using ReMarkableRemember.Common.FileSystem;

namespace ReMarkableRemember.Helper;

public static class Log
{
    public static void Exception(Exception exception)
    {
        String date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.InvariantCulture);
        String logFilePath = FileSystem.CreateApplicationDataFilePath("logs.txt");
        File.AppendAllText(logFilePath, $"{date}|ERROR|{exception}{Environment.NewLine}");
    }
}
