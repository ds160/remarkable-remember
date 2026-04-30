using System;
using System.Collections.Generic;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Common.Notebook.Exceptions;

namespace ReMarkableRemember.Common.Notebook;

public abstract partial class Page
{
    private Page(Int32 index, Int32 resolution, List<Line> lines)
    {
        this.Index = index;
        this.Resolution = resolution;

        this.Lines = lines;
    }

    public Int32 Index { get; }

    public IEnumerable<Line> Lines { get; }

    public Int32 Resolution { get; }

    internal static Page Parse(Byte[] buffer, Int32 index, Int32 resolution)
    {
        Buffer pageBuffer = new Buffer(buffer);
        return pageBuffer.ReadString(43) switch
        {
            "reMarkable .lines file, version=5          " => new Version5(pageBuffer, index, resolution),
            "reMarkable .lines file, version=6          " => new Version6(pageBuffer, index, resolution),
            _ => throw new NotebookException(Language.Current.NotebookHeaderUnknown),
        };
    }
}
