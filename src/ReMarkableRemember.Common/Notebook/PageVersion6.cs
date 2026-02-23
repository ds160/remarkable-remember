using System;
using System.Collections.Generic;
using ReMarkableRemember.Common.Notebook.Exceptions;

namespace ReMarkableRemember.Common.Notebook;

internal sealed class PageVersion6(PageBuffer buffer, Int32 index, Int32 resolution) : Page(index, resolution, Parse(buffer))
{
    private static List<Line> Parse(PageBuffer buffer)
    {
        List<Line> lines = new List<Line>();

        while (buffer.Position < buffer.Length)
        {
            BlockHeader blockHeader = ReadBlockHeader(buffer);
            if (blockHeader.Type != 5)
            {
                buffer.Skip(blockHeader.Length);
            }
            else
            {
                buffer.ReadId(1); // parent_id
                buffer.ReadId(2); // item_id
                buffer.ReadId(3); // left_id
                buffer.ReadId(4); // right_id
                buffer.ReadInt32(5); // deleted_length
                if (buffer.Position < blockHeader.EndPosition)
                {
                    Int32 subBlockLength = buffer.ReadSubBlockLength(6);
                    Int32 itemType = buffer.ReadByte();
                    if (itemType != 3)
                    {
                        buffer.Skip(subBlockLength);
                    }
                    else
                    {
                        lines.Add(ReadLine(buffer, blockHeader.Version));
                    }

                    Int32 remaining = blockHeader.EndPosition - buffer.Position;
                    buffer.Skip(remaining);
                }
            }
        }

        return lines;
    }

    private static BlockHeader ReadBlockHeader(PageBuffer buffer)
    {
        Int32 length = buffer.ReadInt32();
        Byte unknown = buffer.ReadByte();
        Byte versionMinimum = buffer.ReadByte();
        Byte versionCurrent = buffer.ReadByte();
        Byte type = buffer.ReadByte();

        if (unknown != 0) { throw new NotebookException($"Invalid reMarkable .lines file block header: '{unknown}'."); }
        if (versionMinimum > versionCurrent) { throw new NotebookException("Invalid reMarkable .lines file block header version."); }
        if (versionCurrent > 2) { throw new NotebookException($"Unknown reMarkable .lines file block header version: '{versionCurrent}'."); }

        return new BlockHeader(buffer.Position + length, length, type, versionCurrent);
    }

    private static Line ReadLine(PageBuffer buffer, Int32 version)
    {
        PenType type = (PenType)buffer.ReadInt32(1);
        PenColor color = (PenColor)buffer.ReadInt32(2);
        buffer.ReadDouble(3); // thickness_scale
        buffer.ReadSingle(4); // starting_length

        List<Point> points = new List<Point>();
        Int32 pointsDataLength = buffer.ReadSubBlockLength(5);
        Int32 pointsEndPosition = buffer.Position + pointsDataLength;
        while (buffer.Position < pointsEndPosition)
        {
            points.Add(ReadPoint(buffer, version));
        }

        buffer.ReadId(6); // timestamp

        return new Line(color, type, points);
    }

    private static Point ReadPoint(PageBuffer buffer, Int32 version)
    {
        Single x = buffer.ReadSingle();
        Single y = buffer.ReadSingle();

        if (version == 1)
        {
            buffer.ReadSingle(); // speed
            buffer.ReadSingle(); // direction
            buffer.ReadSingle(); // width
            buffer.ReadSingle(); // pressure
        }
        else
        {
            buffer.ReadInt16(); // speed
            buffer.ReadInt16(); // direction
            buffer.ReadByte(); // width
            buffer.ReadByte(); // pressure
        }

        return new Point(x, y);
    }

    private sealed class BlockHeader
    {
        public BlockHeader(Int32 endPosition, Int32 length, Byte type, Byte version)
        {
            this.EndPosition = endPosition;
            this.Length = length;
            this.Type = type;
            this.Version = version;
        }

        public Int32 EndPosition { get; }
        public Int32 Length { get; }
        public Byte Type { get; }
        public Byte Version { get; }
    }
}
