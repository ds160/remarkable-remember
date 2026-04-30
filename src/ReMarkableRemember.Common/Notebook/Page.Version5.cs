using System;
using System.Collections.Generic;
using ReMarkableRemember.Common.Notebook.Enumerations;

namespace ReMarkableRemember.Common.Notebook;

public partial class Page
{
    private sealed class Version5(Buffer buffer, Int32 index, Int32 resolution) : Page(index, resolution, Parse(buffer))
    {
        private static List<Line> Parse(Buffer buffer)
        {
            List<Line> lines = new List<Line>();

            Int32 layersCounter = buffer.ReadInt32();
            while (layersCounter-- > 0)
            {
                Int32 linesCounter = buffer.ReadInt32();
                while (linesCounter-- > 0)
                {
                    lines.Add(ReadLine(buffer));
                }
            }

            return lines;
        }

        private static Line ReadLine(Buffer buffer)
        {
            PenType type = (PenType)buffer.ReadInt32();
            PenColor color = (PenColor)buffer.ReadInt32();
            buffer.ReadInt32(); // unknown
            buffer.ReadSingle(); // thickness_scale
            buffer.ReadSingle(); // unknown

            List<Point> points = new List<Point>();
            Int32 pointsCounter = buffer.ReadInt32();
            while (pointsCounter-- > 0)
            {
                points.Add(ReadPoint(buffer));
            }

            return new Line(color, type, points);
        }

        private static Point ReadPoint(Buffer buffer)
        {
            Single x = buffer.ReadSingle();
            Single y = buffer.ReadSingle();
            buffer.ReadSingle(); // speed
            buffer.ReadSingle(); // direction
            buffer.ReadSingle(); // width
            buffer.ReadSingle(); // pressure

            return new Point(x, y);
        }
    }
}
