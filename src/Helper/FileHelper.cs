using System;
using System.IO;

namespace ReMarkableRemember.Helper;

public static class FileHelper
{
    public static FileStream Create(String path)
    {
        String? directory = Path.GetDirectoryName(path);
        if (!String.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return File.Create(path);
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
