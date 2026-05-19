using System;
using System.IO;
using System.Text;
using ReMarkableRemember.Common.Notebook;

namespace ReMarkableRemember.Services.HandWritingRecognitionService.Tests.Helpers;

internal static class NotebookFixture
{
    public static Notebook EmptyVersion5Notebook(Int32 pageCount = 1, Int32 resolution = 226)
    {
        Byte[][] pages = new Byte[pageCount][];
        for (Int32 i = 0; i < pageCount; i++)
        {
            pages[i] = BuildEmptyVersion5();
        }

        return Notebook.Parse(pages, resolution);
    }

    private static Byte[] BuildEmptyVersion5()
    {
        using MemoryStream stream = new MemoryStream();
        using BinaryWriter writer = new BinaryWriter(stream, Encoding.Default);
        writer.Write(Encoding.Default.GetBytes("reMarkable .lines file, version=5          "));
        writer.Write(0); // zero layers
        return stream.ToArray();
    }
}
