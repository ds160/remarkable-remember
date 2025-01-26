using System;
using System.IO;

namespace ReMarkableRemember.Common.FileSystem;

public static class FileSystem
{
    public static FileStream Create(String path)
    {
        CreateDirectory(path);
        return File.Create(path);
    }

    public static String CreateApplicationDataFilePath(String fileName)
    {
        String path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(ReMarkableRemember), fileName);
        CreateDirectory(path);
        return path;
    }

    private static void CreateDirectory(String path)
    {
        String? directory = Path.GetDirectoryName(path);
        if (!String.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public static void Delete(String path)
    {
        if (Path.Exists(path))
        {
            FileAttributes attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                Directory.Delete(path, true);
            }
            else
            {
                File.Delete(path);
            }
        }
    }
}