using System;
using System.Collections.Generic;

namespace ReMarkableRemember.Common.Notebook;

public abstract class Page
{
    protected Page(Int32 index, Int32 resolution, List<Line> lines)
    {
        this.Index = index;
        this.Resolution = resolution;

        this.Lines = lines;
    }

    public Int32 Index { get; }

    public IEnumerable<Line> Lines { get; }

    public Int32 Resolution { get; }
}
