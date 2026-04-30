using System;
using System.Collections.Generic;
using System.Linq;

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
        return new Notebook(pageBuffers.Select((buffer, index) => Page.Parse(buffer, index, resolution)).ToList());
    }
}
