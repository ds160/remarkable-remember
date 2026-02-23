using System;
using System.Collections.Generic;
using System.Linq;
using ReMarkableRemember.Common.Notebook.Exceptions;

namespace ReMarkableRemember.Common.Notebook;

public sealed class Notebook
{
    private Notebook(List<Page> pages)
    {
        this.Pages = pages;
    }

    public IEnumerable<Page> Pages { get; }

    public static Notebook Parse(IEnumerable<Byte[]> pageBuffers, Int32 resolution)
    {
        return new Notebook(pageBuffers.Select((buffer, index) => ParsePage(buffer, index, resolution)).ToList());
    }

    private static Page ParsePage(Byte[] buffer, Int32 index, Int32 resolution)
    {
        PageBuffer pageBuffer = new PageBuffer(buffer);
        return pageBuffer.ReadString(43) switch
        {
            "reMarkable .lines file, version=5          " => new PageVersion5(pageBuffer, index, resolution),
            "reMarkable .lines file, version=6          " => new PageVersion6(pageBuffer, index, resolution),
            _ => throw new NotebookException("Unknown reMarkable .lines file header."),
        };
    }
}
