using System.Collections.Generic;

namespace ReMarkableRemember.Common.Notebook;

public sealed class Line
{
    internal Line(PenColor color, PenType type, List<Point> points)
    {
        this.Color = color;
        this.Type = type;

        this.Points = points;
    }
    public PenColor Color { get; }
    public IEnumerable<Point> Points { get; }
    public PenType Type { get; }
}
