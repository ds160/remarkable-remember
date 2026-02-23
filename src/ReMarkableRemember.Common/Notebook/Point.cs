using System;

namespace ReMarkableRemember.Common.Notebook;

public sealed class Point
{
    internal Point(Single x, Single y)
    {
        this.X = x;
        this.Y = y;
    }

    public Single X { get; }
    public Single Y { get; }
}
